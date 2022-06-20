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

namespace U2.SharpTracker.Core.Tests.RuTracker
{
    public class ParserTests
    {
        [Fact]
        public void FailsOnBadContent()
        {
            var parser = new RTBranchParser();
            var stream = new MemoryStream(new byte[]{0x01, 0x02});
            Assert.Throws<ParserException>(() => parser.Parse(stream));
        }

        [Fact]
        public void CanParseBranchPage()
        {
            var parser = new RTBranchParser();
            var stream = new MemoryStream(TestResource.rt_f1);
            var result = parser.Parse(stream);
            Assert.NotNull(result);

            Assert.Equal(15, result.Branches.Count());
            Assert.Equal(50, result.Pages.Count());
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.TotalPages);
        }
    }
}
