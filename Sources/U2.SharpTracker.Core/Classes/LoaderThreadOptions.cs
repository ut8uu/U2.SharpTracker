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

namespace U2.SharpTracker.Core;

public sealed class LoaderThreadOptions
{
    /// <summary>
    /// A pause before calling the next URL.
    /// </summary>
    public TimeSpan PauseBeforeNextCall { get; set; } = TimeSpan.FromMilliseconds(10000);

    /// <summary>
    /// A delay before making a retry to the failed URL.
    /// </summary>
    public TimeSpan DelayBeforeRetry { get; set; } = TimeSpan.FromMilliseconds(1000);

    /// <summary>
    /// A number of retries to the same URL
    /// before it considered failed.
    /// </summary>
    public int NumberOfRetries { get; set; } = 5;

    /// <summary>
    /// Specifies a maximal number of failed URLs
    /// before stopping the further activity.
    /// </summary>
    public int MaxNumberOfFailedUrlsInRow { get; set; } = 10;

    /// <summary>
    /// A number of lines of the log to keep in memory.
    /// </summary>
    public int LogSize { get; set; } = 200;
}