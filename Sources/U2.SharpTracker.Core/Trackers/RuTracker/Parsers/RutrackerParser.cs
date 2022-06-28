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

public sealed class RutrackerParser : IParser
{
    private const string RuTrackerUrl = "https://rutracker.org";
    //const string PageRecordXpath = "//table[@class='vf-table vf-tor forumline forum']//tr//td[2]//a[1]";
    const string PageRecordXpath = "//tr[@class='hl-tr']";
    const string BranchRecordXpath = "//table[@class='forumline forum']//tr/td[2]//a";
    const string IndexPath = "//div[@id='pagination']/p[1]/b[1]";
    const string TotalPagesPath = "//div[@id='pagination']/p[1]/b[2]";

    //private const string TorrentPageTitle = "//div[@class='post_body']//";
    private const string TorrentPageTitle = "//a[@id='topic-title']";
    private const string TorrentPageDescription = "//td[@class='message td2']//div[@class='post_wrap']//div[@class='post_body']";
    private const string TorrentPageMagnet = "//a[@class='med magnet-link']";

    public ListingPage ParseBranch(Stream stream)
    {
        var xdoc = new HtmlDocument();
        xdoc.Load(stream);

        var index = 1;
        var totalPages = 1;
        var indexNode = xdoc.DocumentNode.SelectSingleNode(IndexPath);
        if (indexNode != null)
        {
            index = int.Parse(indexNode.InnerText);

            var totalPagesNode = xdoc.DocumentNode.SelectSingleNode(TotalPagesPath);
            if (indexNode == null)
            {
                throw new ParserException("Error extracting total pages number.");
            }
            totalPages = int.Parse(totalPagesNode.InnerText);
        }

        var branches = new Dictionary<string, string>();
        var pages = new List<string>();

        var elements = xdoc.DocumentNode.SelectNodes(BranchRecordXpath);

        if (elements != null)
        {
            // can be empty
            foreach (var element in elements)
            {
                var href = element.Attributes["href"];
                if (!href.Value.Contains("viewforum", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var url = $"{RuTrackerUrl}/forum/{href.Value}";
                var title = element.InnerText;
                if (!branches.Keys.Contains(url))
                {
                    branches[url] = title;
                }
            }
        }

        elements = xdoc.DocumentNode.SelectNodes(PageRecordXpath);
        foreach (var element in elements)
        {
            var id = element.Attributes["data-topic_id"];
            var url = $"{RuTrackerUrl}/forum/viewtopic.php?t={id.Value}";
            if (!pages.Contains(url))
            {
                pages.Add(url);
            }
        }

        var result = new ListingPage
        {
            Branches = branches,
            Pages = pages,
            CurrentPage = index,
            TotalPages = totalPages,
        };

        return result;
    }

    /// <summary>
    /// Parses given stream as a content of the torrent page
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="ParserException"></exception>
    public TorrentPageInfo ParseTorrentPage(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var result = new TorrentPageInfo();

        var win1251 = Encoding.GetEncoding("windows-1251");

        var xdoc = new HtmlDocument();
        xdoc.Load(stream, win1251);

        var titleNode = FindNode(xdoc, TorrentPageTitle, 
            "Failed to extract title");

        var desctiprionNode = FindNode(xdoc, TorrentPageDescription,
            "Failed to extract description");

        var magnetNode = FindNode(xdoc, TorrentPageMagnet,
            "Failed to extract magnet link");

        result.Title = titleNode.InnerText;
        result.Description = desctiprionNode.InnerHtml;
        result.MagnetLink =
            RegularExpressionHelper.MatchAndGetFirst("xt=urn:btih:([A-F0-9]+)&",
                magnetNode.Attributes["href"].Value);

        return result;
    }

    /// <summary>
    /// Attempts to find out the node by its xPath.
    /// </summary>
    /// <param name="doc">An HtmlDocument to search in</param>
    /// <param name="xpath">An xpath to search for</param>
    /// <param name="exceptionMessage">An exception mesage to show if node hasn't been found.</param>
    /// <returns></returns>
    /// <exception cref="ParserException"></exception>
    private static HtmlNode FindNode(HtmlDocument doc, string xpath, string exceptionMessage)
    {
        var node = doc.DocumentNode.SelectSingleNode(xpath);
        if (node == null)
        {
            throw new ParserException(exceptionMessage);
        }

        return node;
    }
}