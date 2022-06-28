/* 
 * This file is part of the U2.SharpTracker distribution
 * (https://github.com/ut8uu/U2.SharpTracker).
 * 
 * Copyright (c) 2022 Sergey Usmanov.
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using U2.SharpTracker.Core.Storage;
using U2.SharpTracker.Core.Trackers.RuTracker;

namespace U2.SharpTracker.Core;

public sealed class RTPerBranchStrategy : IDownloadStrategy
{
    private string _branchId = "0";
    private bool _started;
    private const string _rutrackerUrl = "https://rutracker.org";
    private readonly IParser _parser = new RutrackerParser();
    private Task _runnerTask;
    private CancellationTokenSource _cancellationTokenSource;

    private IMongoDatabase _database;
    private IStorage _storage;

    public RTPerBranchStrategy(IMongoDatabase database)
    {
        _database = database;
        _storage = new TrackerStorage(_database);
    }

    public event UserInputRequiredEventHandler UserInputRequired;
    public event InternetResourceContentRequiredEventHandler InternetResourceContentRequired;
    public event ProgressReportedEventHandler ProgressReported;
    public event WorkFinishedEventHandler WorkFinished;
    public event TorrentPageLoadedEventHandler TorrentPageLoaded;
    public event StrategyReadyEventHandler StrategyReady;

    public IStorage Storage { get; set; }
    public bool Ready { get; private set; }
    public List<string> Pages { get; } = new();

    public async Task StartAsync()
    {
        if (_started)
        {
            return;
        }

        Ready = false;
        _started = true;

        _cancellationTokenSource = new CancellationTokenSource();
        
        var eventArgs = new UserInputRequiredEventArgs
        {
            MessageToUser = "Enter identifier of the branch to download.",
            UserInput = null,
        };
        OnUserInputRequired(eventArgs);
        if (eventArgs.Canceled)
        {
            _started = false;
            OnWorkFinished(WorkFinishStatusCode.Canceled);
            return;
        }

        if (!await CollectBranchPages())
        {
            _started = false;
            OnWorkFinished(WorkFinishStatusCode.Incomplete);
            return;
        }

        Ready = true;
        OnStrategyReady();
    }

    /// <summary>
    /// Collects all pages of this branch
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CollectBranchPages()
    {
        Pages.Clear();
        var token = _cancellationTokenSource.Token;

        var branchDto = await _storage.TryGetBranchAsync(_branchId, token);
        if (branchDto == null)
        {
            branchDto = new BranchDto
            {
                OriginalId = _branchId,
                Id = Guid.NewGuid(),
                Title = string.Empty,
                ParentId = Guid.Empty,
                Url = RuTrackerHelper.GenerateBranchUrl(_branchId, 0),
            };
            await _storage.AddBranchAsync(branchDto, token);
        }

        var start = 0;
        while (true)
        {
            var url = RuTrackerHelper.GenerateBranchUrl(_branchId, start);
            var info = new UrlInfo(url);
            OnInternetResourceContentRequired(info, out var content);
            if (info.UrlLoadStatusCode != UrlLoadStatusCode.Success)
            {
                return false;
            }

            // resource loaded
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var listingPage = _parser.ParseBranch(memoryStream);
            foreach (var page in listingPage.Pages)
            {
                if (await _storage.HasUrl(page.Url, _cancellationTokenSource.Token))
                {
                    continue;
                }

                var urlDto = new TopicDto
                {
                    Id = Guid.NewGuid(),
                    BranchId = branchDto.Id,
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
                await _storage.AddUrlAsync(urlDto, token);
            }

            if (listingPage.CurrentPage == listingPage.TotalPages)
            {
                break;
            }

            start = listingPage.CurrentPage * 50;
        }
        return true;
    }

    /// <summary>
    /// Collects all topic pages on all branch pages
    /// </summary>
    /// <returns></returns>
    private bool CollectTopicPages()
    {
        while (Pages.Count > 0)
        {
            var url = Pages.First();
            var info = new UrlInfo(url);
            OnInternetResourceContentRequired(info, out var content);

            TorrentPageInfo torrentInfo = null;

            if (info.UrlLoadStatusCode == UrlLoadStatusCode.Success)
            {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

                try
                {
                    torrentInfo = _parser.ParseTorrentPage(stream);
                    torrentInfo.Url = info.Url;
                }
                catch (ParserException ex)
                {
                    torrentInfo = new TorrentPageInfo
                    {
                        Url = info.Url,
                        StatusCode = info.UrlLoadStatusCode,
                        ParserStatusCode = ParserStatusCode.Fail,
                        ProcessingMessage = ex.Message,
                    };
                }
                catch (Exception ex)
                {
                    #warning TODO Make smarter solution about logging this
                    torrentInfo = new TorrentPageInfo
                    {
                        Url = info.Url,
                        StatusCode = info.UrlLoadStatusCode,
                        ParserStatusCode = ParserStatusCode.Fail,
                        ProcessingMessage = ex.Message,
                    };
                }

            }
            else
            {
                torrentInfo = new TorrentPageInfo
                {
                    ParserStatusCode = ParserStatusCode.Fail,
                    Url = info.Url,
                };
            }
            OnTorrentPageLoaded(torrentInfo);
            Pages.Remove(url);
        }
        return true;
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }

        _started = false;
        _cancellationTokenSource.Cancel();
    }

    public async Task<TopicDto> TryGetNextUrlAsync()
    {
        if (!Ready)
        {
            throw new StrategyNotReadyException();
        }

        var urls = _storage.GetUrlsAsync(_cancellationTokenSource.Token);
        var nextUrl = await urls.FirstOrDefaultAsync(x => x.UrlLoadState == UrlLoadState.Unknown);
        if (nextUrl == null)
        {
            throw new NoMoreUrlsToDownloadException();
        }

        return nextUrl;
    }

    private void OnUserInputRequired(UserInputRequiredEventArgs e)
    {
        UserInputRequired?.Invoke(this, e);
    }

    private void OnInternetResourceContentRequired(UrlInfo info, out string content)
    {
        content = string.Empty;

        var eventArgs = new InternetResourceContentRequiredEventArgs
        {
            UrlInfo = info,
        };
        InternetResourceContentRequired?.Invoke(this, eventArgs);
        content = eventArgs.ResourceContent;
    }

    private void OnProgressReported(int progress, string message)
    {
        var args = new ProgressReportedEventArgs
        {
            Cancel = false,
            Progress = progress,
            Text = message,
        };
        ProgressReported?.Invoke(this, args);
    }

    private void OnWorkFinished(WorkFinishStatusCode statusCode)
    {
        var args = new WorkFinishedEventArgs
        {
            StatusCode = statusCode,
        };
        WorkFinished?.Invoke(this, args);
    }

    private void OnTorrentPageLoaded(TorrentPageInfo pageInfo)
    {
        TorrentPageLoaded?.Invoke(this, new TorrentPageLoadedEventArgs
        {
            PageInfo = pageInfo,
        });
    }

    private void OnStrategyReady()
    {
        StrategyReady?.Invoke(this);
    }
}
