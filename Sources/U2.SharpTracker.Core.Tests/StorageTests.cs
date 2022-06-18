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
using MongoDB.Driver;
using U2.SharpTracker.Core.Storage;
using Xunit;

namespace U2.SharpTracker.Core.Tests;

public class StorageTests : IDisposable
{
    protected IMongoDatabase Database { get; private set; }

    public StorageTests()
    {
        MongoDBRunner.Start();
        Database = MongoDBRunner.CreateDatabase(GetType().Name);
    }

    public void Dispose()
    {
        Database = null;
        MongoDBRunner.Stop();
    }

    [Fact]
    public async Task CanAddGetBranch()
    {
        var storage = new TrackerStorage(Database);

        var branch = new BranchDto
        {
            Id = Guid.NewGuid(),
            Name = "test branch",
            ParentId = Guid.Empty,
            Url = "test url",
        };

        await storage.AddBranchAsync(branch, CancellationToken.None);

        var addedBranch = await storage.TryGetBranchAsync(branch.Id, CancellationToken.None);
        Assert.NotNull(addedBranch);
        addedBranch.ShouldDeepEqual(branch);
    }
}
