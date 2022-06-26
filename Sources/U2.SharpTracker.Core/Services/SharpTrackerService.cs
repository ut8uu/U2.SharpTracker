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

public sealed class SharpTrackerService : ISharpTrackerService
{
    private readonly IStorage _storage;
    private readonly ITrackerSvcSettings _svcSettings;

    public SharpTrackerService(IStorage storage)
    {
        _storage = storage;
    }

    public IAsyncEnumerable<TopicDto> GetTopicsAsync(Guid branchId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TopicDto> GetTopicAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TopicDto> GetTopicAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task AddBranchAsync(BranchDto branch, CancellationToken cancellationToken)
    {
        await _storage.AddBranchAsync(branch, cancellationToken);
    }

    public Task<TopicDto> GetTopic(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TopicDto> GetTopic(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<List<BranchDto>> GetBranchesAsync(Guid parentId, CancellationToken cancellationToken)
    {
        try
        {
            var collection = _storage.GetBranchesAsync(parentId, cancellationToken).ToEnumerable();
            var list = new List<BranchDto>(collection);
            return list;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public Task<BranchDto> GetBranchAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<BranchDto> GetBranchAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteBranchAsync(Guid id, CancellationToken cancellationToken)
    {
        await _storage.DeleteBranchAsync(id, cancellationToken);
    }
}