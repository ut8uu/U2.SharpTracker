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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using U2.SharpTracker.Core.Storage;
using Xunit;

namespace U2.SharpTracker.Core.Tests;

public class PerBranchStrategyTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly MongoDBRunner _runner;

    public PerBranchStrategyTests()
    {
        var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        _testDirectory = Path.Combine(currentPath, "Tests", "PerBranchStrategyTests");

        _runner = new MongoDBRunner(_testDirectory);
        _runner.Start();
    }

    public void Dispose()
    {
        _runner.Stop();
    }

    [Fact]
    public async Task EntireSuccessfulFlowTest()
    {
        var branchId = 0;
        var branchRequested = false;
        var numberOfRequestedPages = 0;
        var numberOfParsedPages = 0;
        var reportedProgress = new List<Tuple<int, string>>();

        var database = _runner.CreateDatabase("EntireSuccessfulFlowTest");
        var strategy = new RTPerBranchStrategy(database);
        // strategy asks for a BranchId to parse
        strategy.ProgressReported += (sender, args) =>
        {
            reportedProgress.Add(new Tuple<int, string>(args.Progress, args.Text));
        };
        strategy.UserInputRequired += (sender, args) =>
        {
            args.UserInput = "1";
            branchId = 1;
        };
        strategy.InternetResourceContentRequired += (sender, args) =>
        {
            if (args.UrlInfo.Url.Contains("viewforum", StringComparison.InvariantCultureIgnoreCase))
            {
                // a request for the branch page
                args.ResourceContent = Encoding.UTF8.GetString(TestResource.rt_f1_short);
                args.UrlInfo.UrlLoadState = UrlLoadState.Completed;
                args.UrlInfo.UrlLoadStatusCode = UrlLoadStatusCode.Success;

                branchRequested = true;
            }
            else if (args.UrlInfo.Url.Contains("viewtopic", StringComparison.InvariantCultureIgnoreCase))
            {
                // a request for the topic page
                if (args.UrlInfo.Url.Contains("t=1"))
                {
                    args.ResourceContent = Encoding.UTF8.GetString(TestResource.rt_t1);
                    numberOfRequestedPages++;
                }
                else if (args.UrlInfo.Url.Contains("t=2"))
                {
                    args.ResourceContent = Encoding.UTF8.GetString(TestResource.rt_t2);
                    numberOfRequestedPages++;
                }
                else if (args.UrlInfo.Url.Contains("t=3"))
                {
                    args.ResourceContent = Encoding.UTF8.GetString(TestResource.rt_t3);
                    numberOfRequestedPages++;
                }
                args.UrlInfo.UrlLoadState = UrlLoadState.Completed;
                args.UrlInfo.UrlLoadStatusCode = UrlLoadStatusCode.Success;
            }
        };
        strategy.TorrentPageLoaded += (sender, args) =>
        {
            numberOfParsedPages++;
        };
        await strategy.StartAsync();

        Assert.Equal(1, branchId);
        Assert.True(branchRequested);

        var dbStorage = new TrackerStorage(database);
        var pages = dbStorage.GetUrlsAsync(CancellationToken.None);
        Assert.Equal(3, await pages.CountAsync());

        // now it's time to download pages
        while (true)
        {
            try
            {
                var url = await strategy.TryGetNextUrlAsync();
                if (url == null)
                {
                    break;
                }

                numberOfRequestedPages++;
                url.LoadStatusCode = UrlLoadStatusCode.Success;
                url.UrlLoadState = UrlLoadState.Loaded;
                await dbStorage.UpdateUrlAsync(url, CancellationToken.None);
            }
            catch (NoMoreUrlsToDownloadException)
            {
                break;
            }
        }

        strategy.Stop();

        pages = dbStorage.GetUrlsAsync(CancellationToken.None);
        Assert.Equal(3, await pages.CountAsync(x => x.LoadStatusCode == UrlLoadStatusCode.Success));

        _runner.DropDatabase("EntireSuccessfulFlowTest");
    }
}
