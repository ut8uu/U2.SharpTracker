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

public delegate void UserInputRequiredEventHandler(object sender, UserInputRequiredEventArgs e);
public delegate void InternetResourceContentRequiredEventHandler(object sender, InternetResourceContentRequiredEventArgs e);
public delegate void ProgressReportedEventHandler(object sender, ProgressReportedEventArgs eventArgs);
public delegate void WorkFinishedEventHandler(object sender, WorkFinishedEventArgs eventArgs);
public delegate void TorrentPageLoadedEventHandler(object sender, TorrentPageLoadedEventArgs eventArgs);
public delegate void StrategyReadyEventHandler(object sender);

public interface IDownloadStrategy
{
    IStorage Storage { get; set; }

    /// <summary>
    /// Indicates whether the strategy is ready to server requests
    /// </summary>
    bool Ready { get; }

    /// <summary>
    /// Starts the strategy. 
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stops the strategy. It rejects further requests until started.
    /// </summary>
    void Stop();

    /// <summary>
    /// Returns the next URL for downloading.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NoMoreUrlsToDownloadException"></exception>
    Task<TopicDto> TryGetNextUrlAsync();

    event UserInputRequiredEventHandler UserInputRequired;
    event InternetResourceContentRequiredEventHandler InternetResourceContentRequired;
    event ProgressReportedEventHandler ProgressReported;
    event WorkFinishedEventHandler WorkFinished;
    event TorrentPageLoadedEventHandler TorrentPageLoaded;
    event StrategyReadyEventHandler StrategyReady;
}
