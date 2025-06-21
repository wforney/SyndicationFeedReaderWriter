// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Xml;

namespace Microsoft.SyndicationFeed.Rss;

/// <summary>
/// Represents a reader for RSS feeds, specifically RSS 2.0. Implements the <see cref="XmlFeedReader"/>
/// </summary>
/// <seealso cref="XmlFeedReader"/>
/// <remarks>Initializes a new instance of the <see cref="RssFeedReader"/> class.</remarks>
/// <param name="reader">The reader.</param>
/// <param name="parser">The parser.</param>
/// <seealso cref="ISyndicationFeedReader"/>
public class RssFeedReader(XmlReader reader, ISyndicationFeedParser? parser = null) : XmlFeedReader(reader, parser ?? new RssParser())
{
    private bool _knownFeed;

    /// <inheritdoc/>
    public override async Task<bool> Read()
    {
        if (!_knownFeed)
        {
            await InitRead();
            _knownFeed = true;
        }

        return await base.Read();
    }

    /// <inheritdoc/>
    protected override SyndicationElementType MapElementType(string elementName)
    {
        return Reader.NamespaceURI != RssConstants.Rss20Namespace
            ? SyndicationElementType.Content
            : elementName switch
            {
                RssElementNames.Item => SyndicationElementType.Item,
                RssElementNames.Link => SyndicationElementType.Link,
                RssElementNames.Category => SyndicationElementType.Category,
                RssElementNames.Author or RssElementNames.ManagingEditor => SyndicationElementType.Person,
                RssElementNames.Image => SyndicationElementType.Image,
                _ => SyndicationElementType.Content,
            };
    }

    private async Task InitRead()
    {
        // Check <rss>
        bool knownFeed = Reader.IsStartElement(RssElementNames.Rss, RssConstants.Rss20Namespace) &&
                         Reader.GetAttribute(RssElementNames.Version)?.Equals(RssConstants.Version) == true;

        if (knownFeed)
        {
            // Read<rss>
            _ = await XmlUtils.ReadAsync(Reader);

            // Check <channel>
            knownFeed = Reader.IsStartElement(RssElementNames.Channel, RssConstants.Rss20Namespace);
        }

        if (!knownFeed)
        {
            throw new XmlException("Unknown Rss Feed");
        }

        // Read <channel>
        _ = await XmlUtils.ReadAsync(Reader);
    }
}