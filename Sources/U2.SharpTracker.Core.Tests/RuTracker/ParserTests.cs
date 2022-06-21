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
            var parser = new RutrackerParser();
            var stream = new MemoryStream(new byte[]{0x01, 0x02});
            Assert.Throws<ParserException>(() => parser.ParseBranch(stream));
        }

        [Fact]
        public void CanParseBranchPage()
        {
            var parser = new RutrackerParser();
            var stream = new MemoryStream(TestResource.rt_f1);
            var result = parser.ParseBranch(stream);
            Assert.NotNull(result);

            Assert.Equal(15, result.Branches.Count());
            Assert.Equal(50, result.Pages.Count());
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.TotalPages);
        }

        [Theory]
        [MemberData(nameof(ParseTopicTestData))]
        public void CanParseTopicPage(TopicTestData testData)
        {
            var parser = new RutrackerParser();
            using var stream = new MemoryStream(TestResource.rt_t1);
            var pageInfo = parser.ParseTorrentPage(stream);
            Assert.Equal(testData.Title, pageInfo.Title);
        }

        #region Test data

        public static IEnumerable<object[]> ParseTopicTestData
        {
            get
            {
                return new List<object[]>
                {
                    new object[]
                    {
                        new TopicTestData()
                        {
                            Title = "Только для взрослых. Выпуск первый (Ефим Гамбург) [1971, СССР, рисованный мультфильм, короткометражка, 35mm Film Scan 1080p]",
                            ExpectedParserException = false,
                            ExpectedWords = new List<string>
                            {
                                "1971", "СССР", "Продолжительность", "1440x1080",
                            },
                            MagnetLink = "AF907474F5308A1EEE324FEA4D08671DD1096052",
                        },
                    },
                };
            }
        }

        public class TopicTestData
        {
            public string RawContent { get; set; }
            public string Title { get; set; }
            public string MagnetLink { get; set; }
            public IEnumerable<string> ExpectedWords { get; set; }
            public bool ExpectedParserException { get; set; }
        }

        #endregion
    }
}
