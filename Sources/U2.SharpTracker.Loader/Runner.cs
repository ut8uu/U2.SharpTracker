using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using U2.SharpTracker.Core;
using U2.SharpTracker.Core.Storage;
using U2.SharpTracker.Core.Trackers.RuTracker;

namespace U2.SharpTracker.Loader
{
    internal sealed class Runner
    {
        private readonly SharpTrackerService _service;
        private readonly IParser _parser = new RutrackerParser();

        public Runner()
        {
            var settings = new TrackerSvcSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "RuTracker",
            };
            var db = CreateMongoDatabase(settings);
            var storage = new TrackerStorage(db);
            _service = new SharpTrackerService(storage);
        }

        public async Task ResetAsync(CancellationToken token)
        {
            var branches = await _service.GetBranchesAsync(token);
            foreach (var b in branches)
            {
                var topicsToUpdate = new List<TopicDto>();
                Console.WriteLine($"{Environment.NewLine}Processing topics from {b.Title}");
                var pages = _service.GetTopicsAsync(b.Id, token);
                await foreach (var t in pages)
                {
                    if (t.UrlLoadState == UrlLoadState.Unknown)
                    {
                        continue;
                    }
                    t.UrlLoadState = UrlLoadState.Unknown;
                    await _service.AddOrUpdateTopicAsync(t, token);
                    Console.Write(".");
                }
            }
            Console.WriteLine();
        }


        public async Task RunAsync(CancellationToken token)
        {
            //await ResetAsync(token);
            await RuTrackerCatalogue.InitCatalogue(_service);
            await _service.ResetLoadingBranchesAsync(token);

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(100, token);

                var branch = await GetWaitingBranchAsync(token);
                if (branch != null)
                {
                    await ProcessBranchAsync(branch, token);
                    continue;
                }

                var filter = new BsonDocument("UrlLoadState", 0);
                var topic = await GetWaitingTopicAsync2(filter, token);
                if (topic != null)
                {
                    await ProcessTopicAsync(topic, token);
                    continue;
                }
            }
        }

        private async Task ProcessTopicAsync(TopicDto topic, CancellationToken token)
        {
            Debug.Assert(topic != null);
            Debug.Assert(!string.IsNullOrEmpty(topic.Url));

            var dt = DateTime.Now.ToString("yyMMdd:HHmm");

            Console.WriteLine($"{dt} Processing topic: {topic.Url}");

            topic.UrlLoadState = UrlLoadState.Loading;
            await _service.AddOrUpdateTopicAsync(topic, token);

            TorrentPageInfo torrentInfo = null;
            var info = new UrlInfo(topic.Url);
            var content = await DownloadUrlAsync(info, token);

            if (info.UrlLoadStatusCode == UrlLoadStatusCode.Success)
            {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                try
                {
                    torrentInfo = _parser.ParseTorrentPage(stream);
                    topic.Title = torrentInfo.Title;
                    topic.Description = torrentInfo.Description;
                    topic.RawContent = torrentInfo.RawContent;
                    topic.Hash = torrentInfo.MagnetLink;
                    topic.ParseStatusCode = ParserStatusCode.Success;

                    topic.UrlLoadState = UrlLoadState.Completed;
                    topic.LoadStatusCode = UrlLoadStatusCode.Success;
                }
                catch (ParserException ex)
                {
                    topic.LoadStatusCode = UrlLoadStatusCode.Success;
                    topic.UrlLoadState = UrlLoadState.Loaded;
                    topic.ParseStatusCode = ParserStatusCode.Fail;
                    topic.ProcessingMessage = ex.Message;
                }
                catch (Exception ex)
                {
                    topic.LoadStatusCode = UrlLoadStatusCode.Success;
                    topic.UrlLoadState = UrlLoadState.Loaded;
                    topic.ParseStatusCode = ParserStatusCode.Fail;
                    topic.ProcessingMessage = ex.Message;
                }
            }
            else
            {
                topic.LoadStatusCode = UrlLoadStatusCode.Failure;
                topic.UrlLoadState = UrlLoadState.Loaded;
                topic.ParseStatusCode = ParserStatusCode.Unknown;
                topic.ProcessingMessage = string.Empty;
            }

            await _service.AddOrUpdateTopicAsync(topic, token);
        }

        private Task<TopicDto> GetWaitingTopicAsync(CancellationToken token)
        {
            return _service.GetWaitingTopicAsync(token);
        }

        private Task<TopicDto> GetWaitingTopicAsync2(BsonDocument filter, CancellationToken token)
        {
            return _service.GetWaitingTopicAsync2(filter, token);
        }

        private async Task<BranchDto> GetWaitingBranchAsync(CancellationToken token)
        {
            var branches = await _service.GetBranchesAsync(token);
            return branches.FirstOrDefault(b => b.LoadState == UrlLoadState.Unknown);
        }

        private async Task ProcessBranchAsync(BranchDto branch, CancellationToken cancellationToken)
        {
            if (branch == null)
            {
                throw new ArgumentNullException(nameof(branch));
            }

            Console.WriteLine($"Processing branch: {branch.Url}");

            branch.LoadState = UrlLoadState.Loading;
            await _service.AddOrUpdateBranchAsync(branch, cancellationToken);

            var start = 0;
            while (true)
            {
                var pageIndex = (start / 50) + 1;
                Console.WriteLine($"Page {pageIndex}");
                var url = $"{branch.Url.Replace("&start=0", "")}&start={start}";
                var info = new UrlInfo(url);
                var content = await DownloadUrlAsync(info, cancellationToken);
                if (info.UrlLoadStatusCode != UrlLoadStatusCode.Success)
                {
                    branch.LoadStatusCode = info.UrlLoadStatusCode;
                    branch.LoadState = UrlLoadState.Completed;
                    await _service.AddOrUpdateBranchAsync(branch, cancellationToken);
                    return;
                }

                // resource loaded
                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                var listingPage = _parser.ParseBranch(memoryStream);

                var nlRequired = false;

                foreach (var b in listingPage.Branches)
                {
                    var originalId = RutrackerParser.GetIdFromUrl(b.Key);
                    if (!await _service.ContainsBranchAsync(originalId, cancellationToken))
                    {
                        var newBranch = new BranchDto
                        {
                            Id = Guid.NewGuid(),
                            OriginalId = originalId,
                            ParentId = branch.Id,
                            LoadStatusCode = UrlLoadStatusCode.Unknown,
                            Url = b.Key,
                            LoadState = UrlLoadState.Unknown,
                            Title = b.Value,
                        };
                        await _service.AddOrUpdateBranchAsync(newBranch, cancellationToken);

                        Console.WriteLine($"Inserted new branch: {b.Key}");
                    }
                }

                foreach (var page in listingPage.Pages)
                {
                    var topicDto = new TopicDto
                    {
                        Id = Guid.NewGuid(),
                        OriginalId = page.OriginalId,
                        BranchId = branch.Id,
                        RawContent = string.Empty,
                        UrlLoadState = UrlLoadState.Unknown,
                        LoadStatusCode = UrlLoadStatusCode.Unknown,
                        Url = page.Url,
                        Title = page.Title,
                        Leechers = page.Leechers,
                        Seeders = page.Seeders,
                        Size = page.Size,
                        DownloadNumber = page.DownloadNumber,
                        Replies = page.Replies,
                        Description = string.Empty,
                        ParseStatusCode = page.ParserStatusCode,
                        Hash = string.Empty,
                        ProcessingMessage = page.ProcessingMessage,
                    };
                    if (await _service.AddTopicIfNotExistsAsync(topicDto, cancellationToken))
                    {
                        Console.Write("+");
                        nlRequired = true;
                    }
                }

                if (nlRequired)
                {
                    Console.WriteLine("");
                }

                if (listingPage.CurrentPage == listingPage.TotalPages)
                {
                    break;
                }

                start = listingPage.CurrentPage * 50;

                if (start >= 25000)
                {
                    break;
                }
            }

            branch.LoadStatusCode = UrlLoadStatusCode.Success;
            branch.LoadState = UrlLoadState.Loaded;
            await _service.AddOrUpdateBranchAsync(branch, cancellationToken);
        }

        static IMongoDatabase CreateMongoDatabase(TrackerSvcSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            return client.GetDatabase(settings.DatabaseName);
        }

        async Task<string> DownloadUrlAsync(UrlInfo url, CancellationToken token)
        {
            try
            {
                var cache = FileCache.TryGetCache(url.Url);
                if (!string.IsNullOrEmpty(cache))
                {
                    url.UrlLoadState = UrlLoadState.Loaded;
                    url.UrlLoadStatusCode = UrlLoadStatusCode.Success;
                    return cache;
                }

                var client = new HttpClient();
                var response = await client.GetAsync(url.Url);
                var content = await response.Content.ReadAsByteArrayAsync();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var win1251 = Encoding.GetEncoding("windows-1251");
                var responseString = win1251.GetString(content, 0, content.Length);
                url.UrlLoadState = UrlLoadState.Loaded;
                url.UrlLoadStatusCode = UrlLoadStatusCode.Success;

                if (responseString != null)
                {
                    FileCache.PutCache(url.Url, responseString);
                }

                return responseString;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
