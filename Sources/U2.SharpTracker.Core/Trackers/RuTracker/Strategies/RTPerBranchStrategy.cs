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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using U2.SharpTracker.Core.Classes;

namespace U2.SharpTracker.Core;

public sealed class RTPerBranchStrategy : IDownloadStrategy
{
    private int _branchId = 0;
    private bool _started;
    private const string _rutrackerUrl = "https://rutracker.org";
    private readonly IParser _parser = new RTBranchParser();

    public bool Ready { get; }
    public List<string> Pages { get; } = new();

    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;

        var eventArgs = new UserInputRequiredEventArgs
        {
            MessageToUser = "Enter identifier of the branch to download.",
            UserInput = null,
        };
        OnUserInputRequired(eventArgs);
        if (eventArgs.Canceled)
        {
            _started = false;
            return;
        }

        if (!int.TryParse(eventArgs.UserInput, out _branchId))
        {
            _started = false;
            return;
        }

        if (!CollectBranchPages())
        {
            _started = false;
            return;
        }

        if (!CollectTopicPages())
        {
            _started = false;
            return;
        }
    }

    /// <summary>
    /// Collects all pages of this branch
    /// </summary>
    /// <returns></returns>
    private bool CollectBranchPages()
    {
        Pages.Clear();

        var start = 0;
        while (true)
        {
            var eventArgs = new InternetResourceContentRequiredEventArgs
            {
                UrlInfo = new UrlInfo
                {
                    Url = $"{_rutrackerUrl}/viewforum.php?f={_branchId}&start={start}",
                },
                ResourceContent = string.Empty,
            };
            OnInternetResourceContentRequired(eventArgs);
            if (string.IsNullOrEmpty(eventArgs.ResourceContent))
            {
                return false;
            }

            // resource loaded
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(eventArgs.ResourceContent));
            var listingPage = _parser.Parse(memoryStream);
            foreach (var page in listingPage.Pages)
            {
                if (!Pages.Contains(page, StringComparer.InvariantCultureIgnoreCase))
                {
                    Pages.Add(page);
                }
            }
            if (listingPage.CurrentPage == listingPage.TotalPages)
            {
                break;
            }

            start = (listingPage.CurrentPage - 1) * 50;
        }
        return true;
    }

    /// <summary>
    /// Collects all topic pages on all branch pages
    /// </summary>
    /// <returns></returns>
    private bool CollectTopicPages()
    {
        return false;
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }
    }

    public string GetNextUrl()
    {
        if (!Ready)
        {
            throw new StrategyNotReadyException();
        }

        throw new NoMoreUrlsToDownloadException();
    }

    public event UserInputRequiredEventHandler UserInputRequired;
    public event InternetResourceContentRequiredEventHandler InternetResourceContentRequired;
    public event ProgressReportedEventHandler ProgressReported;

    private void OnUserInputRequired(UserInputRequiredEventArgs e)
    {
        UserInputRequired?.Invoke(this, e);
    }

    private void OnInternetResourceContentRequired(InternetResourceContentRequiredEventArgs e)
    {
        InternetResourceContentRequired?.Invoke(this, e);
    }

    private void OnProgressReported(ProgressReportedEventArgs eventArgs)
    {
        ProgressReported?.Invoke(this, eventArgs);
    }
}
