// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Microsoft.SyndicationFeed.Rss;

/// <summary>
/// Represents a parser for RSS feeds, specifically RSS 2.0 format. Implements the <see cref="ISyndicationFeedParser"/>
/// </summary>
/// <seealso cref="ISyndicationFeedParser"/>
public class RssParser : ISyndicationFeedParser
{
    /// <inheritdoc/>
    public virtual ISyndicationCategory CreateCategory(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return content.Value is null
            ? throw new FormatException("Invalid Rss category name")
            : (ISyndicationCategory)new SyndicationCategory(content.Value)
            {
                Scheme = content.Attributes.GetRss(RssConstants.Domain)
            };
    }

    /// <inheritdoc/>
    public virtual ISyndicationImage CreateImage(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        string? title = null;
        string? description = null;
        Uri? url = null;
        ISyndicationLink? link = null;

        foreach (ISyndicationContent field in content.Fields)
        {
            if (field.Namespace != RssConstants.Rss20Namespace)
            {
                continue;
            }

            switch (field.Name)
            {
                // Title
                case RssElementNames.Title:
                    title = field.Value;
                    break;

                // Url
                case RssElementNames.Url:
                    if (!TryParseValue(field.Value, out url))
                    {
                        throw new FormatException($"Invalid image url '{field.Value}'");
                    }
                    break;

                // Link
                case RssElementNames.Link:
                    link = CreateLink(field);
                    break;

                // Description
                case RssElementNames.Description:
                    description = field.Value;
                    break;

                default:
                    break;
            }
        }

        return url is null
            ? throw new FormatException("Image url not found")
            : (ISyndicationImage)new SyndicationImage(url, RssElementNames.Image)
            {
                Title = title,
                Description = description,
                Link = link
            };
    }

    /// <inheritdoc/>
    public virtual ISyndicationItem CreateItem(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var item = new SyndicationItem();

        foreach (ISyndicationContent field in content.Fields)
        {
            if (field.Namespace != RssConstants.Rss20Namespace)
            {
                continue;
            }

            switch (field.Name)
            {
                // Title
                case RssElementNames.Title:
                    item.Title = field.Value;
                    break;

                // Link
                case RssElementNames.Link:
                    item.AddLink(CreateLink(field));
                    break;

                // Description
                case RssElementNames.Description:
                    item.Description = field.Value;
                    break;

                // Author
                case RssElementNames.Author:
                    item.AddContributor(CreatePerson(field));
                    break;

                // Category
                case RssElementNames.Category:
                    item.AddCategory(CreateCategory(field));
                    break;

                // Links
                case RssElementNames.Comments:
                case RssElementNames.Enclosure:
                case RssElementNames.Source:
                    item.AddLink(CreateLink(field));
                    break;

                // Guid
                case RssElementNames.Guid:
                    item.Id = field.Value;

                    // isPermaLink
                    string? isPermaLinkAttr = field.Attributes.GetRss(RssConstants.IsPermaLink);

                    if ((isPermaLinkAttr is null || (TryParseValue(isPermaLinkAttr, out bool isPermalink) && isPermalink)) &&
                        TryParseValue(field.Value, out Uri? permaLink))
                    {
                        item.AddLink(new SyndicationLink(permaLink, RssLinkTypes.Guid));
                    }

                    break;

                // PubDate
                case RssElementNames.PubDate:
                    if (TryParseValue(field.Value, out DateTimeOffset dt))
                    {
                        item.Published = dt;
                    }
                    break;

                default:
                    break;
            }
        }

        return item;
    }

    /// <inheritdoc/>
    public virtual ISyndicationLink CreateLink(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        // Title
        string? title = content.Value;
        string? url = content.Attributes.GetRss("url");

        // Url
        Uri? uri;
        if (url is not null)
        {
            if (!TryParseValue(url, out uri))
            {
                throw new FormatException("Invalid url attribute");
            }
        }
        else
        {
            if (!TryParseValue(content.Value, out uri))
            {
                throw new FormatException("Invalid url");
            }

            title = null;
        }

        // Length
        _ = TryParseValue(content.Attributes.GetRss("length"), out long length);

        // Type
        string? type = content.Attributes.GetRss("type");

        // rel
        string rel = (content.Name == RssElementNames.Link) ? RssLinkTypes.Alternate : content.Name;

        return new SyndicationLink(uri, rel)
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
        ArgumentNullException.ThrowIfNull(content.Value);

        // Handle real name parsing Ex:
        // <author>abc@def.com (John Doe)</author>

        string email = content.Value;
        string? name = null;

        int nameStart = content.Value.IndexOf('(');

        if (nameStart != -1)
        {
            int end = content.Value.IndexOf(')');

            if (end == -1 || end - nameStart - 1 < 0)
            {
                throw new FormatException("Invalid Rss person");
            }

            email = content.Value[..nameStart].Trim();

            name = content.Value.Substring(nameStart + 1, end - nameStart - 1);
        }

        return new SyndicationPerson(name!, email, content.Name);
    }

    /// <inheritdoc/>
    public ISyndicationCategory ParseCategory(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return content.Name != RssElementNames.Category ||
            content.Namespace != RssConstants.Rss20Namespace
            ? throw new FormatException("Invalid Rss category")
            : CreateCategory(content);
    }

    /// <inheritdoc/>
    public ISyndicationContent ParseContent(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        using XmlReader reader = XmlUtils.CreateXmlReader(value);
        _ = reader.MoveToContent();

        return ReadSyndicationContent(reader);
    }

    /// <inheritdoc/>
    public ISyndicationImage ParseImage(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return content.Name != RssElementNames.Image ||
            content.Namespace != RssConstants.Rss20Namespace
            ? throw new FormatException("Invalid Rss Image")
            : CreateImage(content);
    }

    /// <inheritdoc/>
    public ISyndicationItem ParseItem(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return content.Name != RssElementNames.Item ||
            content.Namespace != RssConstants.Rss20Namespace
            ? throw new FormatException("Invalid Rss item")
            : CreateItem(content);
    }

    /// <inheritdoc/>
    public ISyndicationLink ParseLink(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return content.Name != RssElementNames.Link ||
            content.Namespace != RssConstants.Rss20Namespace
            ? throw new FormatException("Invalid Rss link")
            : CreateLink(content);
    }

    /// <inheritdoc/>
    public ISyndicationPerson ParsePerson(string value)
    {
        ISyndicationContent content = ParseContent(value);

        return (content.Name != RssElementNames.Author &&
             content.Name != RssElementNames.ManagingEditor) ||
            content.Namespace != RssConstants.Rss20Namespace
            ? throw new FormatException("Invalid Rss Person")
            : CreatePerson(content);
    }

    /// <inheritdoc/>
    public virtual bool TryParseValue<T>(string? value, [NotNullWhen(true)] out T? result) => Converter.TryParseValue(value, out result);

    private static SyndicationContent ReadSyndicationContent(XmlReader reader)
    {
        var content = new SyndicationContent(reader.LocalName, reader.NamespaceURI, null);

        // Attributes
        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                ISyndicationAttribute? attr = reader.ReadSyndicationAttribute();

                if (attr is not null)
                {
                    content.AddAttribute(attr);
                }
            }

            _ = reader.MoveToContent();
        }

        // Content
        if (!reader.IsEmptyElement)
        {
            reader.ReadStartElement();

            // Value
            if (reader.HasValue)
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
        else
        {
            reader.Skip();
        }

        return content;
    }
}

internal static class RssAttributeExtentions
{
    /// <summary>
    /// Gets the RSS.
    /// </summary>
    /// <param name="attributes">The attributes.</param>
    /// <param name="name">The name.</param>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    public static string? GetRss(this IEnumerable<ISyndicationAttribute> attributes, string name)
    {
        return attributes.FirstOrDefault(a => a.Name == name && (a.Namespace == RssConstants.Rss20Namespace || a.Namespace is null))?.Value;
    }
}