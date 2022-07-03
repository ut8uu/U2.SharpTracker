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

using MongoDB.Bson;
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
        return _storage.GetUrlsAsync(branchId, cancellationToken);
    }

    public Task<TopicDto> GetTopicAsync(Guid id, CancellationToken cancellationToken)
    {
        return _storage.TryGetUrlAsync(id, cancellationToken);
    }

    public Task<TopicDto> GetTopicAsync(string url, CancellationToken cancellationToken)
    {
        return _storage.TryGetUrlAsync(url, cancellationToken);
    }

    public Task<TopicDto> GetWaitingTopicAsync2(BsonDocument filter, CancellationToken cancellationToken)
    {
        return _storage.GetTopicAsync(filter, cancellationToken);
    }

    public Task<TopicDto> GetWaitingTopicAsync(CancellationToken cancellationToken)
    {
        var topic = _storage.GetUrlsAsync(cancellationToken)
            .Where(x => x.UrlLoadState == UrlLoadState.Unknown)
            .FirstOrDefaultAsync(cancellationToken);
        return topic.AsTask();
    }

    public async Task AddOrUpdateTopicAsync(TopicDto topicDto, CancellationToken cancellationToken)
    {
        if (await _storage.HasUrl(topicDto.Url, cancellationToken))
        {
            await _storage.UpdateUrlAsync(topicDto, cancellationToken);
        }
        else
        {
            await _storage.AddUrlAsync(topicDto, cancellationToken);
        }
    }

    public async Task<bool> AddTopicIfNotExistsAsync(TopicDto topicDto, CancellationToken cancellationToken)
    {
        if (!await _storage.HasUrl(topicDto.OriginalId, cancellationToken))
        {
            await _storage.AddUrlAsync(topicDto, cancellationToken);
            return true;
        }
        return false;
    }

    public Task DeleteTopicAsync(Guid id, CancellationToken cancellationToken)
    {
        return _storage.DeleteUrlAsync(id, cancellationToken);
    }

    public Task<bool> ContainsTopicAsync(string url, CancellationToken cancellationToken)
    {
        return _storage.HasUrl(url, cancellationToken);
    }

    public Task<bool> ContainsBranchAsync(string url, CancellationToken cancellationToken)
    {
        return _storage.HasBranch(url, cancellationToken);
    }

    public Task<bool> ContainsBranchAsync(int originalId, CancellationToken cancellationToken)
    {
        return _storage.HasBranch(originalId, cancellationToken);
    }

    private async Task<bool> BranchExists(BranchDto branch, CancellationToken cancellationToken)
    {
        return await GetBranchAsync(branch.Id, cancellationToken) != null;
    }

    public async Task AddOrUpdateBranchAsync(BranchDto branch, CancellationToken cancellationToken)
    {
        if (!await BranchExists(branch, cancellationToken))
        {
            await _storage.AddBranchAsync(branch, cancellationToken);
        }
        else
        {
            await _storage.UpdateBranchAsync(branch, cancellationToken);
        }
    }

    public Task<TopicDto> GetTopic(Guid id, CancellationToken cancellationToken)
    {
        return _storage.TryGetUrlAsync(id, cancellationToken);
    }

    public Task<TopicDto> GetTopic(string id, CancellationToken cancellationToken)
    {
        return _storage.TryGetUrlAsync(id, cancellationToken);
    }

    public async Task<List<BranchDto>> GetBranchesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var collection = _storage.GetBranchesAsync(cancellationToken).ToEnumerable();
            var list = new List<BranchDto>(collection);
            return list;
        }
        catch (Exception ex)
        {
            throw;
        }
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
        return _storage.TryGetBranchAsync(id, cancellationToken);
    }

    public Task<BranchDto> GetBranchAsync(Guid id, CancellationToken cancellationToken)
    {
        return _storage.TryGetBranchAsync(id, cancellationToken);
    }

    public Task DeleteBranchAsync(Guid id, CancellationToken cancellationToken)
    {
        return _storage.DeleteBranchAsync(id, cancellationToken);
    }

    public async Task ResetLoadingBranchesAsync(CancellationToken token)
    {
        var branches = _storage.GetBranchesAsync(token)
            .Where(x => x.LoadState == UrlLoadState.Loading);
        await foreach (var b in branches)
        {
            b.LoadState = UrlLoadState.Unknown;
            await _storage.UpdateBranchAsync(b, token);
        }
    }
}