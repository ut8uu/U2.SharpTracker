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
using Mongo2Go;
using MongoDB.Driver;

namespace U2.SharpTracker.Core;

public sealed class MongoDBRunner
{
    private readonly string _dbDirectory;
    MongoDbRunner _dbRunner;
    IMongoClient _dbClient;

    public MongoDBRunner(string dbDirectory)
    {
        _dbDirectory = dbDirectory;
    }

    public void Start()
    {
        _dbRunner = MongoDbRunner.Start(_dbDirectory, singleNodeReplSet: true);
        _dbClient = new MongoClient(_dbRunner.ConnectionString);
    }

    public void Stop()
    {
        _dbClient = null;
        _dbRunner?.Dispose();
        _dbRunner = null;
    }

    public void DropDatabase(string name)
    {
        _dbClient.DropDatabase(name);
    }

    public IMongoDatabase CreateDatabase(string name)
    {
        return _dbClient.GetDatabase(name);
    }

    public string ConnectionString => _dbRunner.ConnectionString;
}
