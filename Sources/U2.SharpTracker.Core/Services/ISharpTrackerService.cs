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

public interface ISharpTrackerService
{
    public IAsyncEnumerable<TopicDto> GetTopicsAsync(Guid branchId, CancellationToken cancellationToken);
    public Task<TopicDto> GetTopicAsync(Guid id, CancellationToken cancellationToken);
    public Task<TopicDto> GetTopicAsync(string id, CancellationToken cancellationToken);
    public Task<TopicDto> GetWaitingTopicAsync(CancellationToken cancellationToken);

    public Task AddOrUpdateTopicAsync(TopicDto topicDto, CancellationToken cancellationToken);
    public Task DeleteTopicAsync(Guid id, CancellationToken cancellationToken);
    public Task<bool> ContainsTopicAsync(string url, CancellationToken cancellationToken);

    Task ResetLoadingBranchesAsync(CancellationToken token);
    public Task<bool> ContainsBranchAsync(string url, CancellationToken cancellationToken);
    public Task AddOrUpdateBranchAsync(BranchDto branch, CancellationToken cancellationToken);
    public Task<List<BranchDto>> GetBranchesAsync(CancellationToken cancellationToken);
    public Task<List<BranchDto>> GetBranchesAsync(Guid parentId, CancellationToken cancellationToken);
    public Task<BranchDto> GetBranchAsync(string id, CancellationToken cancellationToken);
    public Task<BranchDto> GetBranchAsync(Guid id, CancellationToken cancellationToken);
    public Task DeleteBranchAsync(Guid id, CancellationToken cancellationToken);

}