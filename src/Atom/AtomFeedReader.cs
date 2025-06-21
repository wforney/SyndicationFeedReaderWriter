// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Xml;

namespace Microsoft.SyndicationFeed.Atom;

/// <summary>
/// Represents a reader for Atom feeds. Implements the <see cref="XmlFeedReader"/>
/// </summary>
/// <seealso cref="XmlFeedReader"/>
/// <remarks>Initializes a new instance of the <see cref="AtomFeedReader"/> class.</remarks>
/// <param name="reader">The reader.</param>
/// <param name="parser">The parser.</param>
public class AtomFeedReader(XmlReader reader, ISyndicationFeedParser? parser = null) : XmlFeedReader(reader, parser ?? new AtomParser())
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
    public virtual async Task<IAtomEntry> ReadEntry() => await base.ReadItem() is not IAtomEntry item ? throw new FormatException("Invalid Atom entry") : item;

    /// <inheritdoc/>
    protected override SyndicationElementType MapElementType(string elementName)
    {
        return Reader.NamespaceURI != AtomConstants.Atom10Namespace
            ? SyndicationElementType.Content
            : elementName switch
            {
                AtomElementNames.Entry => SyndicationElementType.Item,
                AtomElementNames.Link => SyndicationElementType.Link,
                AtomElementNames.Category => SyndicationElementType.Category,
                AtomElementNames.Logo or AtomElementNames.Icon => SyndicationElementType.Image,
                AtomContributorTypes.Author or AtomContributorTypes.Contributor => SyndicationElementType.Person,
                _ => SyndicationElementType.Content,
            };
    }

    private async Task InitRead()
    {
        // Check <feed>
        if (Reader.IsStartElement(AtomElementNames.Feed, AtomConstants.Atom10Namespace))
        {
            // Read <feed>
            _ = await XmlUtils.ReadAsync(Reader);
        }
        else
        {
            throw new XmlException("Unknown Atom Feed");
        }
    }
}