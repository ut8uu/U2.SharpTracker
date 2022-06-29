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
using System.Globalization;
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
    private const string PageRecordTopicTitleXpath = "//a[@class='torTopic bold tt-text']";
    const string BranchRecordXpath = "//table[@class='forumline forum']//tr/td[2]//a";
    const string IndexPath = "//div[@id='pagination']/p[1]/b[1]";
    const string TotalPagesPath = "//div[@id='pagination']/p[1]/b[2]";

    //private const string TorrentPageTitle = "//div[@class='post_body']//";
    private const string TorrentPageTitle = "//a[@id='topic-title']";
    private const string TorrentPageDescription = "//td[@class='message td2']//div[@class='post_wrap']//div[@class='post_body']";
    private const string TorrentPageMagnet = "//a[@class='med magnet-link']";
    private const string TorrentPageMagnet2 = "//a[@class='magnet-link']";

    public ListingPage ParseBranch(Stream stream)
    {
        var xdoc = new HtmlDocument();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
        var pages = new List<TorrentPageInfo>();

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
            var x = new HtmlDocument();
            x.LoadHtml(element.OuterHtml);
            try
            {
                var id = element.Attributes["data-topic_id"];
                var url = $"{RuTrackerUrl}/forum/viewtopic.php?t={id.Value}";
                var title = FindNode(x, PageRecordTopicTitleXpath, "Title not found").InnerText;
                title = HttpUtility.HtmlDecode(title);

                if (pages.All(x => x.Url != url))
                {
                    var downloadNumber = GetNodeValue(x, "//td[4]/p[2]/b", "0");
                    var downloaded = downloadNumber.Replace(",", "");
                    var leechers = GetNodeValue(x, "//td[3]/div/div/span[3]", "0");
                    var seeders = GetNodeValue(x, "//td[3]/div/div/span[1]", "0");
                    var replies = GetNodeValue(x, "//td[4]/p[1]/span", "0");
                    var size = GetNodeValue(x, "//td[3]/div/div[2]/a", "0");
                    size = size.Replace("&nbsp;GB", "e+9");
                    size = size.Replace("&nbsp;MB", "e+6");
                    size = size.Replace("&nbsp;KB", "e+3");
                    size = size.Replace("&nbsp;B", "");
                    size = size.Replace(".", ",");
                    var sizeNumeric = double.Parse(size, CultureInfo.InvariantCulture);

                    var tpi = new TorrentPageInfo
                    {
                        Url = url,
                        Title = title,
                        Description = string.Empty,
                        DownloadNumber = int.Parse(downloaded),
                        Leechers = int.Parse(leechers),
                        Seeders = int.Parse(seeders),
                        Replies = int.Parse(replies),
                        Size = Convert.ToInt64(sizeNumeric),
                        OriginalId = GetIdFromUrl(url),
                    };
                    pages.Add(tpi);
                }
            }
            catch (Exception ex)
            {
                var s = ex.Message;
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

    public static int GetIdFromUrl(string url)
    {
        if (!url.Contains('='))
        {
            throw new ParserException($"Bad URL '{url}'. '=' is expected.");
        }
        var chunks = url.Split('=', StringSplitOptions.RemoveEmptyEntries);
        if (int.TryParse(chunks.Last(), out var id))
        {
            return id;
        }

        throw new ParserException($"Bad url '{url}'. Can't extract identifier.");
    }

    private string GetNodeValue(HtmlDocument x, string xpath, string defaultValue)
    {
        try
        {
            var node = FindNode(x, xpath, "");
            return node.InnerText;
        }
        catch
        {
            return defaultValue;
        }
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
        xdoc.Load(stream);

        var titleNode = FindNode(xdoc, TorrentPageTitle, "Failed to extract title");

        var desctiprionNode = FindNode(xdoc, TorrentPageDescription, "Failed to extract description");

        var magnetNode = FindNode(xdoc, TorrentPageMagnet, "Failed to extract magnet link", false);
        if (magnetNode == null)
        {
            magnetNode = FindNode(xdoc, TorrentPageMagnet2, "Failed to extract magnet link");
        }

        result.Title = titleNode.InnerText;
        result.Description = desctiprionNode.InnerHtml;
        result.MagnetLink = RegularExpressionHelper.MatchAndGetFirst("xt=urn:btih:([A-F0-9]+)&",
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
    private static HtmlNode FindNode(HtmlDocument doc, string xpath,
        string exceptionMessage, bool throwException = true)
    {
        var node = doc.DocumentNode.SelectSingleNode(xpath);
        if (node != null)
        {
            return node;
        }

        if (throwException)
        {
            throw new ParserException(exceptionMessage);
        }
        return null;
    }

    private static HtmlNode FindNode(HtmlNode input, string xpath, string exceptionMessage)
    {
        var node = input.SelectSingleNode(xpath);
        if (node == null)
        {
            throw new ParserException(exceptionMessage);
        }

        return node;
    }
}