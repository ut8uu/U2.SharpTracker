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

using U2.SharpTracker.Core;

namespace U2.SharpTracker.Svc;

public sealed class BranchInfo
{
    public BranchInfo(BranchDto branchDto)
    {
        Id = branchDto.Id;
        ParentId = branchDto.ParentId;
        OriginalId = branchDto.OriginalId;
        Name = branchDto.Title;
        Url = branchDto.Url;
        ObjectState = branchDto.LoadState;
        LoadStatusCode = branchDto.LoadStatusCode;
        Topics = new List<TopicShortInfo>();
    }

    public Guid Id { get; set; }
    public Guid ParentId { get; set; }
    public string OriginalId { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public UrlLoadState ObjectState { get; set; }
    public UrlLoadStatusCode LoadStatusCode { get; set; }

    public List<TopicShortInfo> Topics { get; set; }
}