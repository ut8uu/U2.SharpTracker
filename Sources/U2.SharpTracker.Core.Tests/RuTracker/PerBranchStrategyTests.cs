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
using Xunit;

namespace U2.SharpTracker.Core.Tests;

public class PerBranchStrategyTests
{
    [Fact]
    public async Task EntireSuccessfulFlowTest()
    {
        int branchId = 0;
        var strategy = new RTPerBranchStrategy();
        // strategy asks for a BranchId to parse
        strategy.UserInputRequired += (sender, args) =>
        {
            args.UserInput = "1";
            branchId = 1;
        };
        strategy.Start();

        var waitTimespan = TimeSpan.FromMilliseconds(100);

        var task = Task.Factory.StartNew(() =>
        {
            while (branchId == 0)
            {
                Thread.Sleep(waitTimespan);
            }
        });
        Assert.True(task.Wait(TimeSpan.FromMilliseconds(5000)));
        Assert.Equal(1, branchId);



        strategy.Stop();
    }
}
