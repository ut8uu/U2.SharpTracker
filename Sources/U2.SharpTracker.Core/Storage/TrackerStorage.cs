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
using MongoDB.Driver;

namespace U2.SharpTracker.Core.Storage
{
    public sealed class TrackerStorage : IStorage
    {
        private const string BranchesCollectionName = "Branches";
        private const string UrlsCollectionName = "Urls";

        private readonly IMongoDatabase _database;
        readonly IMongoCollection<BranchDto> _branchesCollection;
        readonly IMongoCollection<TopicDto> _urlsCollection;

        public TrackerStorage(IMongoDatabase database)
        {
            _database = database;
            _branchesCollection = database.GetCollection<BranchDto>(BranchesCollectionName);
            _urlsCollection = database.GetCollection<TopicDto>(UrlsCollectionName);
        }

        public Task AddBranchAsync(BranchDto branch, CancellationToken cancellationToken)
        {
            return _branchesCollection.InsertOneAsync(branch, new InsertOneOptions(), cancellationToken);
        }

        public Task<BranchDto> TryGetBranchAsync(int originalBranchId, CancellationToken cancellationToken)
        {
            return _branchesCollection.Find(b => b.OriginalId == originalBranchId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<BranchDto> TryGetBranchAsync(Guid branchId, CancellationToken cancellationToken)
        {
            return _branchesCollection.Find(b => b.Id == branchId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task UpdateBranchAsync(BranchDto branch, CancellationToken cancellationToken)
        {
            return _branchesCollection.ReplaceOneAsync(p => p.Id == branch.Id, branch, new ReplaceOptions { }, cancellationToken);
        }

        public Task DeleteBranchAsync(Guid id, CancellationToken cancellationToken)
        {
            return _branchesCollection.DeleteOneAsync(b => b.Id == id, cancellationToken);
        }

        public async IAsyncEnumerable<BranchDto> GetBranchesAsync(CancellationToken cancellationToken)
        {
            var cursor = await _branchesCollection.FindAsync(b => !string.IsNullOrEmpty(b.Url), options: null, cancellationToken);
            await foreach (var branch in cursor.ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return branch;
            }
        }

        public async IAsyncEnumerable<BranchDto> GetBranchesAsync(Guid parentId, CancellationToken cancellationToken)
        {
            var cursor = await _branchesCollection.FindAsync(b => b.ParentId == parentId, options: null, cancellationToken);
            await foreach (var branch in cursor.ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return branch;
            }
        }

        public Task AddUrlAsync(TopicDto url, CancellationToken cancellationToken)
        {
            return _urlsCollection.InsertOneAsync(url, new InsertOneOptions(), cancellationToken);
        }

        public Task<TopicDto> TryGetUrlAsync(Guid id, CancellationToken cancellationToken)
        {
            return _urlsCollection.Find(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<TopicDto> TryGetUrlAsync(string url, CancellationToken cancellationToken)
        {
            return _urlsCollection.Find(x => x.Url == url)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task UpdateUrlAsync(TopicDto url, CancellationToken cancellationToken)
        {
            return _urlsCollection.ReplaceOneAsync(x => x.Id == url.Id, url, new ReplaceOptions { }, cancellationToken);
        }

        public Task DeleteUrlAsync(Guid id, CancellationToken cancellationToken)
        {
            return _urlsCollection.DeleteOneAsync(x => x.Id == id, cancellationToken);
        }

        public async IAsyncEnumerable<TopicDto> GetUrlsAsync(CancellationToken cancellationToken)
        {
            var cursor = await _urlsCollection.FindAsync(x => !string.IsNullOrEmpty(x.Url), options: null, cancellationToken);
            await foreach (var url in cursor.ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return url;
            }
        }

        public async IAsyncEnumerable<TopicDto> GetUrlsAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var cursor = await _urlsCollection.FindAsync(b => b.BranchId == branchId, options: null, cancellationToken);
            await foreach (var url in cursor.ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return url;
            }
        }

        public Task<bool> HasUrl(string url, CancellationToken cancellationToken)
        {
            var foundUrl = _urlsCollection.Find(x => x.Url == url)
                .FirstOrDefault(cancellationToken);
            return Task.FromResult(foundUrl != null);
        }

        public Task<bool> HasUrl(int originalId, CancellationToken cancellationToken)
        {
            var foundUrl = _urlsCollection.Find(x => x.OriginalId == originalId)
                .FirstOrDefault(cancellationToken);
            return Task.FromResult(foundUrl != null);
        }

        public Task<bool> HasBranch(string url, CancellationToken cancellationToken)
        {
            var branchesCount = _branchesCollection.Find(x => x.Url == url)
                .CountDocumentsAsync(cancellationToken);
            return Task.FromResult(branchesCount.Result > 0);
        }
    }
}
