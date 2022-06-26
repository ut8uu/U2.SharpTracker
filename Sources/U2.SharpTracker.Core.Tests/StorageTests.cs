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
using DeepEqual.Syntax;
using MongoDB.Bson;
using MongoDB.Driver;
using U2.SharpTracker.Core.Storage;
using Xunit;

namespace U2.SharpTracker.Core.Tests;

public class StorageTests : IDisposable
{
    private readonly Guid _emptyGuid = Guid.Empty;

    protected IMongoDatabase Database { get; private set; }
    private readonly MongoDBRunner _runner;

    public StorageTests()
    {
        var dbDirectory = Path.Combine(Path.GetDirectoryName(typeof(MongoDBRunner).Assembly.Location), "MongoDB");
        _runner = new MongoDBRunner(dbDirectory);
        _runner.Start();
        Database = _runner.CreateDatabase(GetType().Name);
    }

    public void Dispose()
    {
        _runner.DropDatabase(GetType().Name);
        Database = null;
        _runner.Stop();
    }

    private static BranchDto CreateBranch(Guid parentId)
    {
        return new BranchDto
        {
            Id = Guid.NewGuid(),
            Title = $"test branch {new Random(DateTime.UtcNow.Millisecond).NextInt64()}",
            ParentId = parentId,
            Url = "test url",
        };
    }

    private static TopicDto CreateUrl(Guid branchId)
    {
        return new TopicDto
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            Content = "content",
            ObjectState = UrlLoadState.Added,
            LoadStatusCode = UrlLoadStatusCode.Unknown,
            Url = "url",
        };
    }

    private async Task AddBranchAsync(IStorage storage, BranchDto branch)
    {
        await storage.AddBranchAsync(branch, CancellationToken.None);
        
        var addedBranch = await storage.TryGetBranchAsync(branch.Id, CancellationToken.None);
        Assert.NotNull(addedBranch);
        addedBranch.ShouldDeepEqual(branch);
    }

    private async Task AddUrlAsync(IStorage storage, TopicDto url)
    {
        await storage.AddUrlAsync(url, CancellationToken.None);

        var addedUrl = await storage.TryGetUrlAsync(url.Id, CancellationToken.None);
        Assert.NotNull(addedUrl);
        addedUrl.ShouldDeepEqual(url);
    }

    [Fact]
    public async Task TestBranchCrud()
    {
        var storage = new TrackerStorage(Database);

        var branch = CreateBranch(_emptyGuid);
        await AddBranchAsync(storage, branch);

        var addedBranch = await storage.TryGetBranchAsync(branch.Id, CancellationToken.None);
        Assert.NotNull(addedBranch);
        addedBranch.ShouldDeepEqual(branch);

        addedBranch.Title = "updated name";
        await storage.UpdateBranchAsync(addedBranch, CancellationToken.None);

        var updatedBranch = await storage.TryGetBranchAsync(branch.Id, CancellationToken.None);
        Assert.NotNull(updatedBranch);
        updatedBranch.ShouldDeepEqual(addedBranch);

        await storage.DeleteBranchAsync(branch.Id, CancellationToken.None);
        var deletedBranch = await storage.TryGetBranchAsync(branch.Id, CancellationToken.None);
        Assert.Null(deletedBranch);
    }

    [Fact]
    public async Task TestLoadingBranches()
    {
        var storage = new TrackerStorage(Database);
        var parent = CreateBranch(_emptyGuid);
        await AddBranchAsync(storage, parent);
        var child = CreateBranch(parent.Id);
        await AddBranchAsync(storage, child);

        var rootBranches = storage.GetBranchesAsync(_emptyGuid, CancellationToken.None);
        Assert.Single(rootBranches.ToEnumerable());

        var childBranches = storage.GetBranchesAsync(parent.Id, CancellationToken.None);
        Assert.Single(childBranches.ToEnumerable());
    }

    [Fact]
    public async Task TestUrlCrud()
    {
        var storage = new TrackerStorage(Database);

        var branch = CreateBranch(_emptyGuid);
        var url = CreateUrl(branch.Id);
        await storage.AddUrlAsync(url, CancellationToken.None);
        var addedUrl = await storage.TryGetUrlAsync(url.Id, CancellationToken.None);
        Assert.NotNull(addedUrl);
        addedUrl = await storage.TryGetUrlAsync(url.Url, CancellationToken.None);
        Assert.NotNull(addedUrl);
        
        url.ObjectState = UrlLoadState.Completed;
        url.Url = "updated url";
        await storage.UpdateUrlAsync(url, CancellationToken.None);
        url.ShouldDeepEqual(await storage.TryGetUrlAsync(url.Id, CancellationToken.None));

        await storage.DeleteUrlAsync(url.Id, CancellationToken.None);
        Assert.Null(await storage.TryGetUrlAsync(url.Id, CancellationToken.None));
    }
}
