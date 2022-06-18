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

public delegate void UrlRequiredEventHandler(object sender, UrlRequiredEventArgs eventArgs);
public delegate void PageDownloadFinishedEventHandler(object sender, PageLoadResult result);

public sealed class TrackerLoaderThread
{
    private readonly LoaderThreadOptions _options;
    private UrlInfo _urlInfo;

    public TrackerLoaderThread(LoaderThreadOptions options)
    {
        _options = options;
    }

    public string Title { get; set; }
    public UrlInfo CurrentUrlInfo { get; set; }

    public event PageDownloadFinishedEventHandler PageDownloadFinished;
    public event UrlRequiredEventHandler UrlRequired;

    private void OnPageDownloadFinished(PageLoadResult result)
    {
        PageDownloadFinished?.Invoke(this, result);
    }

    public void LoadPage(string url)
    {
        if (_urlInfo != null)
        {
            if (_urlInfo.UrlLoadState != UrlLoadState.Completed)
            {

            }
        }
    }

    private void OnUrlRequired(UrlRequiredEventArgs eventargs)
    {
        UrlRequired?.Invoke(this, eventargs);
    }
}
