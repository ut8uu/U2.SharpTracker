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

public interface IStorage
{
    Task AddBranchAsync(BranchDto branch, CancellationToken cancellationToken);
    Task<BranchDto> TryGetBranchAsync(int originalBranchId, CancellationToken cancellationToken);
    Task<BranchDto> TryGetBranchAsync(Guid branchId, CancellationToken cancellationToken);
    Task UpdateBranchAsync(BranchDto branch, CancellationToken cancellationToken);
    Task DeleteBranchAsync(Guid id, CancellationToken cancellationToken);

    IAsyncEnumerable<BranchDto> GetBranchesAsync(Guid parentId, CancellationToken cancellationToken);

    Task AddUrlAsync(UrlDto url, CancellationToken cancellationToken);
    Task<UrlDto> TryGetUrlAsync(Guid id, CancellationToken cancellationToken);
    Task<UrlDto> TryGetUrlAsync(string url, CancellationToken cancellationToken);
    Task UpdateUrlAsync(UrlDto url, CancellationToken cancellationToken);
    Task DeleteUrlAsync(Guid id, CancellationToken cancellationToken);

    IAsyncEnumerable<UrlDto> GetUrlsAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<UrlDto> GetUrlsAsync(Guid branchId, CancellationToken cancellationToken);

    Task<bool> HasUrl(string url, CancellationToken cancellationToken);
    Task<bool> HasBranch(int branch, CancellationToken cancellationToken);
}
