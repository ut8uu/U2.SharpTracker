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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace U2.SharpTracker.Core;

public sealed class RTBranchParser : IParser
{
    const string PageRecordXpath = "//table[@class='vf-table vf-tor forumline forum']//tr//td[2]//a[1]";
    const string BranchRecordXpath = "//table[@class='forumline forum']//tr/td[2]//a";
    const string IndexPath = "//div[@id='pagination']/p[1]/b[1]";
    const string TotalPagesPath = "//div[@id='pagination']/p[1]/b[2]";

    public ListingPage Parse(Stream stream)
    {
        var xdoc = new HtmlDocument();
        xdoc.Load(stream);

        var indexNode = xdoc.DocumentNode.SelectSingleNode(IndexPath);
        if (indexNode == null)
        {
            throw new ParserException("Error extracting page index.");
        }
        var index = int.Parse(indexNode.InnerText);

        var totalPagesNode = xdoc.DocumentNode.SelectSingleNode(TotalPagesPath);
        if (indexNode == null)
        {
            throw new ParserException("Error extracting total pages number.");
        }
        var totalPages = int.Parse(totalPagesNode.InnerText);

        var branches = new List<string>();
        var pages = new List<string>();

        var elements = xdoc.DocumentNode.SelectNodes(BranchRecordXpath);
        foreach (var element in elements)
        {
            var href = element.Attributes["href"];
            if (!href.Value.Contains("viewforum", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }
            var url = $"https://rutracker.org/{href.Value}";
            branches.Add(url);
        }

        elements = xdoc.DocumentNode.SelectNodes(PageRecordXpath);
        foreach (var element in elements)
        {
            var href = element.Attributes["href"];
            if (!href.Value.Contains("viewtopic", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }
            var url = $"https://rutracker.org/{href.Value}";
            var uri = new Uri(url);
            var queryChunks = HttpUtility.ParseQueryString(uri.Query);
            if (!queryChunks.AllKeys.Contains("t"))
            {
                continue;
            }
            var t = queryChunks["t"];
            url = $"https://rutracker.org{uri.AbsolutePath}?t={t}";
            if (!pages.Contains(url))
            {
                pages.Add(url);
            }
        }

        var result = new ListingPage
        {
            Branches = branches,
            Pages = pages,
            PageIndex = index,
            TotalPages = totalPages,
        };

        return result;
    }
}