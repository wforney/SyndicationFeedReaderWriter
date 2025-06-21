// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Text;
using System.Xml;

namespace Microsoft.SyndicationFeed.Atom;

/// <summary>
/// Represents a formatter for Atom syndication feeds. Implements the <see cref="ISyndicationFeedFormatter"/>
/// </summary>
/// <seealso cref="ISyndicationFeedFormatter"/>
public class AtomFormatter : ISyndicationFeedFormatter
{
    private readonly StringBuilder _buffer;
    private readonly XmlWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomFormatter"/> class.
    /// </summary>
    /// <param name="knownAttributes">The known attributes.</param>
    /// <param name="settings">The settings.</param>
    public AtomFormatter(IEnumerable<ISyndicationAttribute>? knownAttributes = null, XmlWriterSettings? settings = null)
    {
        _buffer = new StringBuilder();
        _writer = XmlUtils.CreateXmlWriter(
            settings?.Clone() ?? new XmlWriterSettings(),
            EnsureAtomNs(knownAttributes ?? []),
            _buffer);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use CDATA.
    /// </summary>
    /// <value><c>true</c> if we should use CDATA; otherwise, <c>false</c>.</value>
    public bool UseCDATA { get; set; }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(link.Uri);

        return link.RelationshipType switch
        {
            AtomLinkTypes.Content => CreateFromContentLink(link),
            AtomLinkTypes.Source => CreateFromSourceLink(link),
            _ => CreateFromLink(link),
        };
    }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentException.ThrowIfNullOrEmpty(category.Name);

        var result = new SyndicationContent(AtomElementNames.Category);

        // term
        result.AddAttribute(new SyndicationAttribute(AtomConstants.Term, category.Name));

        // scheme
        if (!string.IsNullOrEmpty(category.Scheme))
        {
            result.AddAttribute(new SyndicationAttribute(AtomConstants.Scheme, category.Scheme));
        }

        // label
        if (!string.IsNullOrEmpty(category.Label))
        {
            result.AddAttribute(new SyndicationAttribute(AtomConstants.Label, category.Label));
        }

        return result;
    }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationPerson person)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentException.ThrowIfNullOrEmpty(person.Name);

        string contributorType = person.RelationshipType ?? AtomContributorTypes.Author;

        if (contributorType is not AtomContributorTypes.Author and not AtomContributorTypes.Contributor)
        {
            throw new ArgumentException("RelationshipType");
        }

        var result = new SyndicationContent(contributorType);

        // name
        result.AddField(new SyndicationContent(AtomElementNames.Name, person.Name));

        // email
        if (!string.IsNullOrEmpty(person.Email))
        {
            result.AddField(new SyndicationContent(AtomElementNames.Email, person.Email));
        }

        // uri
        if (person.Uri is not null)
        {
            result.AddField(new SyndicationContent(AtomElementNames.Uri, FormatValue(person.Uri)));
        }

        return result;
    }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(image.Url);
        return new SyndicationContent(
            string.IsNullOrEmpty(image.RelationshipType)
                ? AtomImageTypes.Icon
                : image.RelationshipType,
            FormatValue(image.Url));
    }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrEmpty(item.Id);
        ArgumentException.ThrowIfNullOrEmpty(item.Title);

        if (item.LastUpdated == default)
        {
            throw new ArgumentException("LastUpdated");
        }

        var result = new SyndicationContent(AtomElementNames.Entry);

        // id
        result.AddField(new SyndicationContent(AtomElementNames.Id, item.Id));

        // title
        result.AddField(new SyndicationContent(AtomElementNames.Title, item.Title));

        // updated
        result.AddField(new SyndicationContent(AtomElementNames.Updated, FormatValue(item.LastUpdated)));

        // published
        if (item.Published != default)
        {
            result.AddField(new SyndicationContent(AtomElementNames.Published, FormatValue(item.Published)));
        }

        // link
        bool hasContentLink = false;
        bool hasAlternateLink = false;

        if (item.Links is not null)
        {
            foreach (ISyndicationLink link in item.Links)
            {
                if (link.RelationshipType == AtomLinkTypes.Content)
                {
                    if (hasContentLink)
                    {
                        throw new ArgumentException("Multiple content links are not allowed", nameof(item));
                    }

                    hasContentLink = true;
                }
                else if (link.RelationshipType is null or AtomLinkTypes.Alternate)
                {
                    hasAlternateLink = true;
                }

                result.AddField(CreateContent(link));
            }
        }

        // author/contributor
        bool hasAuthor = false;

        if (item.Contributors is not null)
        {
            foreach (ISyndicationPerson c in item.Contributors)
            {
                if (c.RelationshipType is null or AtomContributorTypes.Author)
                {
                    hasAuthor = true;
                }

                result.AddField(CreateContent(c));
            }
        }

        if (!hasAuthor)
        {
            throw new ArgumentException("Author is required");
        }

        // category
        if (item.Categories is not null)
        {
            foreach (ISyndicationCategory category in item.Categories)
            {
                result.AddField(CreateContent(category));
            }
        }

        var entry = item as IAtomEntry;

        // content
        if (!string.IsNullOrEmpty(item.Description))
        {
            if (hasContentLink)
            {
                throw new ArgumentException("Description and content link are not allowed simultaneously");
            }

            var content = new SyndicationContent(AtomElementNames.Content, item.Description);

            // type
            if (entry is not null &&
                !(string.IsNullOrEmpty(entry.ContentType) || entry.ContentType.Equals(AtomConstants.PlainTextContentType, StringComparison.OrdinalIgnoreCase)))
            {
                content.AddAttribute(new SyndicationAttribute(AtomConstants.Type, entry.ContentType));
            }

            result.AddField(content);
        }
        else
        {
            if (!(hasContentLink || hasAlternateLink))
            {
                throw new ArgumentException("Description or alternate link is required");
            }
        }

        if (entry is not null)
        {
            // summary
            if (!string.IsNullOrEmpty(entry.Summary))
            {
                result.AddField(new SyndicationContent(AtomElementNames.Summary, entry.Summary));
            }

            // rights
            if (!string.IsNullOrEmpty(entry.Rights))
            {
                result.AddField(new SyndicationContent(AtomElementNames.Rights, entry.Rights));
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public string Format(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            WriteSyndicationContent(content);

            _writer.Flush();

            return _buffer.ToString();
        }
        finally
        {
            _ = _buffer.Clear();
        }
    }

    /// <inheritdoc/>
    public string Format(ISyndicationCategory category) => Format(CreateContent(category));

    /// <inheritdoc/>
    public string Format(ISyndicationImage image) => Format(CreateContent(image));

    /// <inheritdoc/>
    public string Format(ISyndicationPerson person) => Format(CreateContent(person));

    /// <inheritdoc/>
    public string Format(ISyndicationItem item) => Format(CreateContent(item));

    /// <inheritdoc/>
    public string Format(IAtomEntry entry) => Format(CreateContent(entry));

    /// <inheritdoc/>
    public string Format(ISyndicationLink link) => Format(CreateContent(link));

    /// <inheritdoc/>
    public virtual string? FormatValue<T>(T value)
    {
        if (value is null)
        {
            return null;
        }

        Type type = typeof(T);

        // DateTimeOffset
        if (type == typeof(DateTimeOffset))
        {
            return DateTimeUtils.ToRfc3339String((DateTimeOffset)(object)value);
        }

        // DateTime
        return type == typeof(DateTime) ? DateTimeUtils.ToRfc3339String(new DateTimeOffset((DateTime)(object)value)) : value.ToString();
    }

    private static IEnumerable<ISyndicationAttribute> EnsureAtomNs(IEnumerable<ISyndicationAttribute> attributes)
    {
        // Insert Atom namespace if it doesn't already exist
        if (!attributes.Any(a => a.Name.StartsWith("xmlns") && a.Value == AtomConstants.Atom10Namespace))
        {
            var list = new List<ISyndicationAttribute>(attributes);
            list.Insert(0, new SyndicationAttribute("xmlns", AtomConstants.Atom10Namespace));

            attributes = list;
        }

        return attributes;
    }

    private SyndicationContent CreateFromContentLink(ISyndicationLink link)
    {
        // content
        var result = new SyndicationContent(AtomElementNames.Content);

        // src
        result.AddAttribute(new SyndicationAttribute(AtomConstants.Source, FormatValue(link.Uri)));

        // type
        if (!string.IsNullOrEmpty(link.MediaType))
        {
            result.AddAttribute(new SyndicationAttribute(AtomConstants.Type, link.MediaType));
        }

        return result;
    }

    private SyndicationContent CreateFromLink(ISyndicationLink link)
    {
        // link
        var result = new SyndicationContent(AtomElementNames.Link);

        // title
        if (!string.IsNullOrEmpty(link.Title))
        {
            result.AddAttribute(new SyndicationAttribute(AtomElementNames.Title, link.Title));
        }

        // href
        result.AddAttribute(new SyndicationAttribute(AtomConstants.Href, FormatValue(link.Uri)));

        // rel
        if (!string.IsNullOrEmpty(link.RelationshipType))
        {
            result.AddAttribute(new SyndicationAttribute(AtomConstants.Rel, link.RelationshipType));
        }

        // type
        if (!string.IsNullOrEmpty(link.MediaType))
        {
            result.AddAttribute(new SyndicationAttribute(AtomConstants.Type, link.MediaType));
        }

        // length
        if (link.Length > 0)
        {
            result.AddAttribute(new SyndicationAttribute(AtomConstants.Length, FormatValue(link.Length)));
        }

        return result;
    }

    private SyndicationContent CreateFromSourceLink(ISyndicationLink link)
    {
        // source
        var result = new SyndicationContent(AtomElementNames.Source);

        // title
        if (!string.IsNullOrEmpty(link.Title))
        {
            result.AddField(new SyndicationContent(AtomElementNames.Title, link.Title));
        }

        // link
        result.AddField(CreateFromLink(new SyndicationLink(link.Uri)
        {
            MediaType = link.MediaType,
            Length = link.Length
        }));

        // updated
        if (link.LastUpdated != default)
        {
            result.AddField(new SyndicationContent(AtomElementNames.Updated, FormatValue(link.LastUpdated)));
        }

        return result;
    }

    private void WriteSyndicationContent(ISyndicationContent content)
    {
        string? type = null;

        // Write Start
        _writer.WriteStartSyndicationContent(content, AtomConstants.Atom10Namespace);

        // Write attributes
        if (content.Attributes is not null)
        {
            foreach (ISyndicationAttribute a in content.Attributes)
            {
                if (type is null && a.Name == AtomConstants.Type)
                {
                    type = a.Value;
                }

                _writer.WriteSyndicationAttribute(a);
            }
        }

        // Write value
        if (content.Value is not null)
        {
            // Xhtml
            if (XmlUtils.IsXhtmlMediaType(type) && content.IsAtom())
            {
                _writer.WriteStartElement("div", AtomConstants.XhtmlNamespace);
                _writer.WriteXmlFragment(content.Value, AtomConstants.XhtmlNamespace);
                _writer.WriteEndElement();
            }
            // Xml (applies to <content>)
            else if (XmlUtils.IsXmlMediaType(type) && content.IsAtom(AtomElementNames.Content))
            {
                _writer.WriteXmlFragment(content.Value, string.Empty);
            }
            // Text/Html
            else
            {
                _writer.WriteString(content.Value, UseCDATA);
            }
        }
        // Write Fields
        else
        {
            if (content.Fields is not null)
            {
                foreach (ISyndicationContent field in content.Fields)
                {
                    WriteSyndicationContent(field);
                }
            }
        }

        // Write End
        _writer.WriteEndElement();
    }
}