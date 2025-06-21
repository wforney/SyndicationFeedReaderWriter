// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Microsoft.SyndicationFeed.Atom;

/// <summary>
/// AtomParser is a parser for Atom feeds, implementing the <see cref="ISyndicationFeedParser"/> interface.
/// </summary>
/// <seealso cref="ISyndicationFeedParser"/>
public class AtomParser : ISyndicationFeedParser
{
    /// <inheritdoc/>
    public virtual ISyndicationCategory CreateCategory(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        string term = content.Attributes.GetAtom(AtomConstants.Term)
            ?? throw new FormatException("Invalid Atom category, requires Term attribute");

        return new SyndicationCategory(term)
        {
            Scheme = content.Attributes.GetAtom(AtomConstants.Scheme),
            Label = content.Attributes.GetAtom(AtomConstants.Label)
        };
    }

    /// <inheritdoc/>
    public virtual IAtomEntry CreateEntry(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var item = new AtomEntry();

        foreach (ISyndicationContent field in content.Fields)
        {
            // content does not contain atom's namespace. So if we receibe a different namespace we
            // will ignore it.
            if (field.Namespace != AtomConstants.Atom10Namespace)
            {
                continue;
            }

            switch (field.Name)
            {
                // Category
                case AtomElementNames.Category:
                    item.AddCategory(CreateCategory(field));
                    break;

                // Content
                case AtomElementNames.Content:

                    item.ContentType = field.Attributes.GetAtom(AtomConstants.Type) ?? AtomConstants.PlainTextContentType;

                    if (field.Attributes.GetAtom(AtomConstants.Source) != null)
                    {
                        item.AddLink(CreateLink(field));
                    }
                    else
                    {
                        item.Description = field.Value;
                    }

                    break;

                // Author/Contributor
                case AtomContributorTypes.Author:
                case AtomContributorTypes.Contributor:
                    item.AddContributor(CreatePerson(field));
                    break;

                // Id
                case AtomElementNames.Id:
                    item.Id = field.Value;
                    break;

                // Link
                case AtomElementNames.Link:
                    item.AddLink(CreateLink(field));
                    break;

                // Published
                case AtomElementNames.Published:
                    if (TryParseValue(field.Value, out DateTimeOffset published))
                    {
                        item.Published = published;
                    }

                    break;

                // Rights
                case AtomElementNames.Rights:
                    item.Rights = field.Value;
                    break;

                // Source
                case AtomElementNames.Source:
                    item.AddLink(CreateSource(field));
                    break;
                // Summary
                case AtomElementNames.Summary:
                    item.Summary = field.Value;
                    break;

                // Title
                case AtomElementNames.Title:
                    item.Title = field.Value;
                    break;

                // Updated
                case AtomElementNames.Updated:
                    if (TryParseValue(field.Value, out DateTimeOffset updated))
                    {
                        item.LastUpdated = updated;
                    }

                    break;

                // Unrecognized tags
                default:
                    break;
            }
        }

        return item;
    }

    /// <inheritdoc/>
    public virtual ISyndicationImage CreateImage(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return !TryParseValue(content.Value, out Uri? uri)
            ? throw new FormatException("Invalid Atom image url")
            : (ISyndicationImage)new SyndicationImage(uri, content.Name);
    }

    /// <inheritdoc/>
    public virtual ISyndicationLink CreateLink(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        // title
        string? title = content.Attributes.GetAtom(AtomElementNames.Title);

        // type
        string? type = content.Attributes.GetAtom(AtomConstants.Type);

        // length
        _ = TryParseValue(content.Attributes.GetAtom(AtomConstants.Length), out long length);

        // rel
        string rel = content.Attributes.GetAtom(AtomConstants.Rel) ?? ((content.Name == AtomElementNames.Link) ? AtomLinkTypes.Alternate : content.Name);

        // href
        _ = TryParseValue(content.Attributes.GetAtom(AtomConstants.Href), out Uri? uri);

        // src
        if (uri is null)
        {
            _ = TryParseValue(content.Attributes.GetAtom(AtomConstants.Source), out uri);
        }

        return uri is null
            ? throw new FormatException("Invalid uri")
            : (ISyndicationLink)new SyndicationLink(uri, rel)
            {
                Title = title,
                Length = length,
                MediaType = type
            };
    }

    /// <inheritdoc/>
    public virtual ISyndicationPerson CreatePerson(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        string? name = null;
        string? email = null;
        string? uri = null;

        foreach (ISyndicationContent field in content.Fields)
        {
            // content does not contain atom's namespace. So if we receibe a different namespace we
            // will ignore it.
            if (field.Namespace != AtomConstants.Atom10Namespace)
            {
                continue;
            }

            switch (field.Name)
            {
                // Name
                case AtomElementNames.Name:
                    name = field.Value;
                    break;

                // Email
                case AtomElementNames.Email:
                    email = field.Value;
                    break;

                // Uri
                case AtomElementNames.Uri:
                    uri = field.Value;
                    break;
                // Unrecognized field
                default:
                    break;
            }
        }

        return new SyndicationPerson(name!, email!, content.Name)
        {
            Uri = uri!
        };
    }

    /// <inheritdoc/>
    public virtual ISyndicationLink CreateSource(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        Uri? url = null;
        string? title = null;
        DateTimeOffset lastUpdated = DateTimeOffset.MinValue;

        foreach (ISyndicationContent field in content.Fields)
        {
            // content does not contain atom's namespace. So if we receibe a different namespace we
            // will ignore it.
            if (field.Namespace != AtomConstants.Atom10Namespace)
            {
                continue;
            }

            switch (field.Name)
            {
                // Id
                case AtomElementNames.Id:

                    if (url is null)
                    {
                        _ = TryParseValue(field.Value, out url);
                    }

                    break;

                // Title
                case AtomElementNames.Title:
                    title = field.Value;
                    break;

                // Updated
                case AtomElementNames.Updated:
                    _ = TryParseValue(field.Value, out lastUpdated);
                    break;

                // Link
                case AtomElementNames.Link:
                    url ??= CreateLink(field).Uri;
                    break;

                // Unrecognized
                default:
                    break;
            }
        }

        ArgumentNullException.ThrowIfNull(url, "Invalid source link");

        return new SyndicationLink(url, AtomLinkTypes.Source)
        {
            Title = title,
            LastUpdated = lastUpdated
        };
    }

    /// <inheritdoc/>
    public ISyndicationCategory ParseCategory(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return content.Name != AtomElementNames.Category ? throw new FormatException("Invalid Atom Category") : CreateCategory(content);
    }

    /// <inheritdoc/>
    public ISyndicationContent ParseContent(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        using XmlReader reader = CreateXmlReader(value);
        _ = reader.MoveToContent();

        return ReadSyndicationContent(reader);
    }

    /// <inheritdoc/>
    public IAtomEntry ParseEntry(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return content.Name != AtomElementNames.Entry ? throw new FormatException("Invalid Atom feed") : CreateEntry(content);
    }

    /// <inheritdoc/>
    public ISyndicationImage ParseImage(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return content.Name is not AtomElementNames.Logo and not AtomElementNames.Icon
            ? throw new FormatException("Invalid Atom Image")
            : CreateImage(content);
    }

    /// <inheritdoc/>
    public ISyndicationItem ParseItem(string value) => ParseEntry(value);

    /// <inheritdoc/>
    public ISyndicationLink ParseLink(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return content.Name != AtomElementNames.Link ? throw new FormatException("Invalid Atom Link") : CreateLink(content);
    }

    /// <inheritdoc/>
    public ISyndicationPerson ParsePerson(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return content.Name is not AtomContributorTypes.Author and not AtomContributorTypes.Contributor
            ? throw new FormatException("Invalid Atom person")
            : CreatePerson(content);
    }

    /// <inheritdoc/>
    public virtual bool TryParseValue<T>(string? value, [NotNullWhen(true)] out T? result) => Converter.TryParseValue(value, out result);

    private static XmlReader CreateXmlReader(string value) => XmlUtils.CreateXmlReader(value);

    private static SyndicationContent ReadSyndicationContent(XmlReader reader)
    {
        string? type = null;

        var content = new SyndicationContent(reader.LocalName, reader.NamespaceURI, null);

        // Attributes
        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                ISyndicationAttribute attr = reader.ReadSyndicationAttribute();

                if (attr is not null)
                {
                    if (type is null && attr.IsAtom(AtomConstants.Type))
                    {
                        type = attr.Value;
                    }

                    content.AddAttribute(attr);
                }
            }

            _ = reader.MoveToContent();
        }

        // Content
        if (!reader.IsEmptyElement)
        {
            // Xml (applies to <content>)
            if (XmlUtils.IsXmlMediaType(type) && content.IsAtom(AtomElementNames.Content))
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    throw new FormatException($"Invalid Xml element");
                }

                content.Value = reader.ReadInnerXml();
            }
            else
            {
                reader.ReadStartElement();

                // Xhtml
                if (XmlUtils.IsXhtmlMediaType(type) && content.IsAtom())
                {
                    if (reader.NamespaceURI != AtomConstants.XhtmlNamespace)
                    {
                        throw new FormatException($"Invalid Xhtml namespace");
                    }

                    content.Value = reader.ReadInnerXml();
                }
                // Text/Html
                else if (reader.HasValue)
                {
                    content.Value = reader.ReadContentAsString();
                }
                // Children
                else
                {
                    while (reader.IsStartElement())
                    {
                        content.AddField(ReadSyndicationContent(reader));
                    }
                }

                reader.ReadEndElement(); // end
            }
        }
        else
        {
            reader.Skip();
        }

        return content;
    }
}