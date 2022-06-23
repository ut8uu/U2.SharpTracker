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

namespace U2.SharpTracker.Core;

public sealed class RTSequentialStrategy : IDownloadStrategy
{
    private int _index = 0;
    private static readonly object _getNextUrlLock = new();

    public IStorage Storage { get; set; }
    public bool Ready { get; private set; } = false;

    public event UserInputRequiredEventHandler UserInputRequired;
    public event InternetResourceContentRequiredEventHandler InternetResourceContentRequired;
    public event ProgressReportedEventHandler ProgressReported;
    public event WorkFinishedEventHandler WorkFinished;
    public event TorrentPageLoadedEventHandler TorrentPageLoaded;
    public event StrategyReadyEventHandler StrategyReady;

    public Task StartAsync()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public Task<UrlDto> TryGetNextUrlAsync()
    {
        lock (_getNextUrlLock)
        {
            _index++;
            var url = $"https://rutracker.org/forum/viewtopic.php?t={_index}";
            return Task.FromResult(new UrlDto
            {
                Url = url,
            });
        }
    }

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
