// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace Microsoft.SyndicationFeed.Tests.Rss;

public class RssReader
{
    [Fact]
    public async Task ReadSequential()
    {
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
        var reader = new RssFeedReader(xmlReader);

        await reader.Read();

        ISyndicationContent content = await reader.ReadContent();
        content = await reader.ReadContent();
        content = await reader.ReadContent();
    }

    [Fact]
    public async Task ReadItemAsContent()
    {
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
        var reader = new RssFeedReader(xmlReader);

        while (await reader.Read())
        {
            if (reader.ElementType == SyndicationElementType.Item)
            {
                // Read as content
                ISyndicationContent content = await reader.ReadContent();

                var fields = content.Fields.ToArray();
                Assert.True(fields.Length >= 6);

                Assert.Equal("title", fields[0].Name);
                Assert.False(string.IsNullOrEmpty(fields[0].Value));

                Assert.Equal("description", fields[1].Name);
                Assert.False(string.IsNullOrEmpty(fields[1].Value));

                Assert.Equal("link", fields[2].Name);
                Assert.False(string.IsNullOrEmpty(fields[2].Value));

                Assert.Equal("guid", fields[3].Name);
                Assert.Single(fields[3].Attributes);
                Assert.False(string.IsNullOrEmpty(fields[3].Value));

                Assert.Equal("creator", fields[4].Name);
                Assert.Equal("http://purl.org/dc/elements/1.1/", fields[4].Namespace);
                Assert.False(string.IsNullOrEmpty(fields[4].Value));

                Assert.Equal("pubDate", fields[5].Name);
                Assert.False(string.IsNullOrEmpty(fields[5].Value));
            }
        }
    }

    [Fact]
    public async Task ReadCategory()
    {
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
        var reader = new RssFeedReader(xmlReader);

        while (await reader.Read())
        {
            if (reader.ElementType == SyndicationElementType.Category)
            {
                ISyndicationCategory category = await reader.ReadCategory();

                Assert.Equal("Newspapers", category.Name);
                Assert.Equal("http://example.com/news", category.Scheme);
            }
        }
    }

    [Fact]
    public async Task ReadItemCategory()
    {
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
        var reader = new RssFeedReader(xmlReader);

        while (await reader.Read())
        {
            if (reader.ElementType == SyndicationElementType.Item)
            {
                ISyndicationItem item = await reader.ReadItem();

                foreach (var c in item.Categories)
                {
                    Assert.Equal("Newspapers", c.Name);
                    Assert.True(c.Scheme == null || c.Scheme == "http://example.com/news/item");
                }
            }
        }
    }

    [Fact]
    public async Task CountItems()
    {
        int itemCount = 0;

        using (var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true })) {
            var reader = new RssFeedReader(xmlReader);

            while (await reader.Read()) {
                if (reader.ElementType == SyndicationElementType.Item) {
                    itemCount++;
                }
            }
        }

        Assert.Equal(10, itemCount);
    }

    [Fact]
    public async Task ReadWhile()
    {
        using var xmlReader = XmlReader.Create(@"..\..\..\TestFeeds\rss20.xml", new XmlReaderSettings() { Async = true });
        var reader = new RssFeedReader(xmlReader);

        while (await reader.Read())
        {
            object _ = reader.ElementType switch
            {
                SyndicationElementType.Link => await reader.ReadLink(),
                SyndicationElementType.Item => await reader.ReadItem(),
                SyndicationElementType.Person => await reader.ReadPerson(),
                SyndicationElementType.Image => await reader.ReadImage(),
                _ => await reader.ReadContent(),
            };
        }
    }

    [Fact]
    public static async Task ReadFeedElements()
    {
        var reader = XmlReader.Create(@"..\..\..\TestFeeds\rss20-2items.xml", new XmlReaderSettings() { Async = true });
        await TestReadFeedElements(reader);
    }

    internal static async Task TestReadFeedElements(XmlReader outerXmlReader)
    {
        using var xmlReader = outerXmlReader;
        var reader = new RssFeedReader(xmlReader);
        int items = 0;
        while (await reader.Read())
        {
            switch (reader.ElementType)
            {
                case SyndicationElementType.Person:
                    ISyndicationPerson person = await reader.ReadPerson();
                    Assert.Equal("John Smith", person.Email);
                    break;

                case SyndicationElementType.Link:
                    ISyndicationLink link = await reader.ReadLink();
                    Assert.Equal(123, link.Length);
                    Assert.Equal("testType", link.MediaType);
                    Assert.Equal("http://example.com/", link.Uri.OriginalString);
                    break;

                case SyndicationElementType.Image:
                    ISyndicationImage image = await reader.ReadImage();
                    Assert.Equal("Microsoft News", image.Title);
                    Assert.Equal("Test description", image.Description);
                    Assert.Equal("http://2.bp.blogspot.com/-NA5Jb-64eUg/URx8CSdcj_I/AAAAAAAAAUo/eCx0irI0rq0/s1600/bg_Microsoft_logo3-20120824073001907469-620x349.jpg", image.Url.OriginalString);
                    break;

                case SyndicationElementType.Item:
                    items++;
                    ISyndicationItem item = await reader.ReadItem();

                    if (items == 1)
                    {
                        Assert.Equal("Lorem ipsum 2017-07-06T20:25:00+00:00", item.Title);
                        Assert.Equal("Exercitation sit dolore mollit et est eiusmod veniam aute officia veniam ipsum.", item.Description);
                        Assert.Equal(3, item.Links.Count());
                    }
                    else if (items == 2)
                    {
                        Assert.Equal("Lorem ipsum 2017-07-06T20:24:00+00:00", item.Title);
                        Assert.Equal("Do ipsum dolore veniam minim est cillum aliqua ea.", item.Description);
                        Assert.Equal(3, item.Links.Count());
                    }

                    break;

                default:
                    break;
            }
        }
    }


    public static async Task<List<ISyndicationContent>> RssReadFeedContent(XmlReader xmlReader)
    {
        var list = new List<ISyndicationContent>();

        using (XmlReader xReader = xmlReader)
        {
            var reader = new RssFeedReader(xReader);

            while(await reader.Read())
            {
                list.Add(await reader.ReadContent());
            }
        }

        return list;
    }
}
