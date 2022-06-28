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

public sealed class TorrentPageLoadedEventArgs
{
    public TorrentPageInfo PageInfo { get; set; }
}

public sealed class TorrentPageInfo
{
    public string Url { get; set; }
    public UrlLoadStatusCode StatusCode { get; set; }
    public ParserStatusCode ParserStatusCode { get; set; }
    public string ProcessingMessage { get; set; }
    public string RawContent { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string MagnetLink { get; set; }
    public long Size { get; set; }
    public int Seeders { get; set; }
    public int Leechers { get; set; }
    public int DownloadNumber { get; set; }
    public int Replies { get; set; }

}