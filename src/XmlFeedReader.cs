// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Xml;

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a base class for reading XML syndication feeds. Implements the <see cref="ISyndicationFeedReader"/>
/// </summary>
/// <param name="reader">The reader.</param>
/// <param name="parser">The parser.</param>
/// <seealso cref="ISyndicationFeedReader"/>
public abstract class XmlFeedReader(XmlReader reader, ISyndicationFeedParser parser) : ISyndicationFeedReader
{
    private bool _currentSet;

    /// <inheritdoc/>
    public string? ElementName { get; private set; }

    /// <inheritdoc/>
    public SyndicationElementType ElementType { get; private set; } = SyndicationElementType.None;

    /// <inheritdoc/>
    public ISyndicationFeedParser Parser { get; private set; } = parser ?? throw new ArgumentNullException(nameof(parser));

    /// <summary>
    /// Gets the reader.
    /// </summary>
    /// <value>The reader.</value>
    protected XmlReader Reader { get; } = reader ?? throw new ArgumentNullException(nameof(reader));

    /// <inheritdoc/>
    public virtual async Task<bool> Read()
    {
        if (_currentSet)
        {
            // The reader is already advanced, return status
            _currentSet = false;
            return !Reader.EOF;
        }
        else
        {
            if (ElementType != SyndicationElementType.None)
            {
                await Skip();
                return !Reader.EOF;
            }
            else
            {
                // Advance the reader
                return await MoveNext(false);
            }
        }
    }

    /// <inheritdoc/>
    public virtual async Task<ISyndicationCategory> ReadCategory()
    {
        if (ElementType == SyndicationElementType.None)
        {
            _ = await Read();
        }

        return ElementType != SyndicationElementType.Category
            ? throw new InvalidOperationException("Unknown Category")
            : Parser.ParseCategory(await ReadElementAsString());
    }

    /// <inheritdoc/>
    public virtual async Task<ISyndicationContent> ReadContent()
    {
        if (ElementType == SyndicationElementType.None)
        {
            _ = await Read();
        }

        // Any element can be read as ISyndicationContent
        return ElementType == SyndicationElementType.None
            ? throw new InvalidOperationException("Unknown Content")
            : Parser.ParseContent(await ReadElementAsString());
    }

    /// <inheritdoc/>
    public virtual async Task<string> ReadElementAsString()
    {
        string result = await XmlUtils.ReadOuterXmlAsync(Reader);

        _ = await MoveNext();

        return result;
    }

    /// <inheritdoc/>
    public virtual async Task<ISyndicationImage> ReadImage()
    {
        if (ElementType == SyndicationElementType.None)
        {
            _ = await Read();
        }

        return ElementType != SyndicationElementType.Image
            ? throw new InvalidOperationException("Unknown Image")
            : Parser.ParseImage(await ReadElementAsString());
    }

    /// <inheritdoc/>
    public virtual async Task<ISyndicationItem> ReadItem()
    {
        if (ElementType == SyndicationElementType.None)
        {
            _ = await Read();
        }

        return ElementType != SyndicationElementType.Item
            ? throw new InvalidOperationException("Unknown Item")
            : Parser.ParseItem(await ReadElementAsString());
    }

    /// <inheritdoc/>
    public virtual async Task<ISyndicationLink> ReadLink()
    {
        if (ElementType == SyndicationElementType.None)
        {
            _ = await Read();
        }

        return ElementType != SyndicationElementType.Link
            ? throw new InvalidOperationException("Unknown Link")
            : Parser.ParseLink(await ReadElementAsString());
    }

    /// <inheritdoc/>
    public virtual async Task<ISyndicationPerson> ReadPerson()
    {
        if (ElementType == SyndicationElementType.None)
        {
            _ = await Read();
        }

        return ElementType != SyndicationElementType.Person
            ? throw new InvalidOperationException("Unknown Person")
            : Parser.ParsePerson(await ReadElementAsString());
    }

    /// <inheritdoc/>
    public virtual async Task<T> ReadValue<T>()
    {
        ISyndicationContent content = await ReadContent();

        return Parser.TryParseValue(content.Value, out T? value) ? value : throw new FormatException();
    }

    /// <inheritdoc/>
    public virtual async Task Skip()
    {
        await XmlUtils.SkipAsync(Reader);
        _ = await MoveNext(false);
    }

    /// <summary>
    /// Maps the type of the element.
    /// </summary>
    /// <param name="elementName">Name of the element.</param>
    /// <returns>SyndicationElementType.</returns>
    protected abstract SyndicationElementType MapElementType(string elementName);

    private async Task<bool> MoveNext(bool setCurrent = true)
    {
        do
        {
            if (Reader.NodeType == XmlNodeType.Element)
            {
                ElementType = MapElementType(Reader.LocalName);
                ElementName = Reader.LocalName;

                _currentSet = setCurrent;

                return true;
            }
        }
        while (await XmlUtils.ReadAsync(Reader));

        // Reset
        ElementType = SyndicationElementType.None;
        ElementName = null;
        _currentSet = false;

        return false;
    }
}
