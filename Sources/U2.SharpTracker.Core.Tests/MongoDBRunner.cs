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

namespace U2.SharpTracker.Core.Tests
{
    public sealed class MongoDBRunner
    {
        static MongoDbRunner _dbRunner;
        static IMongoClient _dbClient;

        public static void Start()
        {
            var dbDirectory = Path.Combine(Path.GetDirectoryName(typeof(MongoDBRunner).Assembly.Location), "MongoDB");
            _dbRunner = MongoDbRunner.Start(dbDirectory, singleNodeReplSet: true);
            _dbClient = new MongoClient(_dbRunner.ConnectionString);
        }

        public static void Stop()
        {
            _dbClient = null;
            _dbRunner?.Dispose();
            _dbRunner = null;
        }

        public static IMongoDatabase CreateDatabase(string name)
        {
            _dbClient.DropDatabase(name);
            return _dbClient.GetDatabase(name);
        }

        public static string ConnectionString => _dbRunner.ConnectionString;
    }

}
