// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Text;
using System.Xml;

namespace Microsoft.SyndicationFeed.Rss;

/// <summary>
/// Represents a formatter for RSS feeds. Implements the <see cref="ISyndicationFeedFormatter"/>
/// </summary>
/// <seealso cref="ISyndicationFeedFormatter"/>
public class RssFormatter : ISyndicationFeedFormatter
{
    private readonly StringBuilder _buffer;
    private readonly XmlWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RssFormatter"/> class.
    /// </summary>
    /// <param name="knownAttributes">The known attributes.</param>
    /// <param name="settings">The settings.</param>
    public RssFormatter(IEnumerable<ISyndicationAttribute>? knownAttributes = null, XmlWriterSettings? settings = null)
    {
        _buffer = new StringBuilder();
        _writer = XmlUtils.CreateXmlWriter(settings?.Clone() ?? new XmlWriterSettings(), knownAttributes, _buffer);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use CDATA sections for content values.
    /// </summary>
    /// <value><c>true</c> if we should use CDATA sections for content values; otherwise, <c>false</c>.</value>
    public bool UseCDATA { get; set; }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(link.Uri);

        return link.RelationshipType switch
        {
            RssElementNames.Enclosure => CreateEnclosureContent(link),
            RssElementNames.Comments => CreateCommentsContent(link),
            RssElementNames.Source => CreateSourceContent(link),
            _ => CreateLinkContent(link),
        };
    }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (string.IsNullOrEmpty(category.Name))
        {
            throw new FormatException("Invalid category name");
        }

        var content = new SyndicationContent(RssElementNames.Category, category.Name);

        if (category.Scheme is not null)
        {
            content.AddAttribute(new SyndicationAttribute(RssConstants.Domain, category.Scheme));
        }

        return content;
    }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationPerson person)
    {
        ArgumentNullException.ThrowIfNull(person);

        // RSS requires Email
        if (string.IsNullOrEmpty(person.Email))
        {
            throw new ArgumentNullException(nameof(person), "Invalid person Email");
        }

        // Real name recommended with RSS e-mail addresses Ex:
        // <author>email@address.com (John Doe)</author>
        string value = string.IsNullOrEmpty(person.Name) ? person.Email : $"{person.Email} ({person.Name})";

        return new SyndicationContent(person.RelationshipType ?? RssElementNames.Author, value);
    }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        // Required URL - Title - Link
        if (string.IsNullOrEmpty(image.Title))
        {
            throw new ArgumentNullException(nameof(image), "Image requires a title");
        }

        if (image.Link is null)
        {
            throw new ArgumentNullException(nameof(image), "Image requires a link");
        }

        if (image.Url is null)
        {
            throw new ArgumentNullException(nameof(image), "Image requires an url");
        }

        var content = new SyndicationContent(RssElementNames.Image);

        // Write required contents of image
        content.AddField(new SyndicationContent(RssElementNames.Url, FormatValue(image.Url)));
        content.AddField(new SyndicationContent(RssElementNames.Title, image.Title));
        content.AddField(CreateContent(image.Link));

        // Write optional elements
        if (!string.IsNullOrEmpty(image.Description))
        {
            content.AddField(new SyndicationContent(RssElementNames.Description, image.Description));
        }

        return content;
    }

    /// <inheritdoc/>
    public virtual ISyndicationContent CreateContent(ISyndicationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Spec requires to have at least one title or description
        if (string.IsNullOrEmpty(item.Title) && string.IsNullOrEmpty(item.Description))
        {
            throw new ArgumentNullException(nameof(item), "RSS Item requires a title or a description");
        }

        // Write <item> tag
        var content = new SyndicationContent(RssElementNames.Item);

        // Title
        if (!string.IsNullOrEmpty(item.Title))
        {
            content.AddField(new SyndicationContent(RssElementNames.Title, item.Title));
        }

        // Links
        ISyndicationLink? guidLink = null;

        if (item.Links is not null)
        {
            foreach (ISyndicationLink link in item.Links)
            {
                if (link.RelationshipType == RssElementNames.Guid)
                {
                    guidLink = link;
                }

                content.AddField(CreateContent(link));
            }
        }

        // Description
        if (!string.IsNullOrEmpty(item.Description))
        {
            content.AddField(new SyndicationContent(RssElementNames.Description, item.Description));
        }

        // Authors (persons)
        if (item.Contributors is not null)
        {
            foreach (ISyndicationPerson person in item.Contributors)
            {
                content.AddField(CreateContent(person));
            }
        }

        // Category
        if (item.Categories is not null)
        {
            foreach (ISyndicationCategory category in item.Categories)
            {
                content.AddField(CreateContent(category));
            }
        }

        // Guid (id)
        if (guidLink is null && !string.IsNullOrEmpty(item.Id))
        {
            var guid = new SyndicationContent(RssElementNames.Guid, item.Id);

            guid.AddAttribute(new SyndicationAttribute(RssConstants.IsPermaLink, "false"));

            content.AddField(guid);
        }

        // PubDate
        if (item.Published != DateTimeOffset.MinValue)
        {
            content.AddField(new SyndicationContent(RssElementNames.PubDate, FormatValue(item.Published)));
        }

        return content;
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
    public string Format(ISyndicationCategory category)
    {
        ISyndicationContent content = CreateContent(category);

        return Format(content);
    }

    /// <inheritdoc/>
    public string Format(ISyndicationImage image)
    {
        ISyndicationContent content = CreateContent(image);

        return Format(content);
    }

    /// <inheritdoc/>
    public string Format(ISyndicationPerson person)
    {
        ISyndicationContent content = CreateContent(person);

        return Format(content);
    }

    /// <inheritdoc/>
    public string Format(ISyndicationItem item)
    {
        ISyndicationContent content = CreateContent(item);

        return Format(content);
    }

    /// <inheritdoc/>
    public string Format(ISyndicationLink link)
    {
        ISyndicationContent content = CreateContent(link);

        return Format(content);
    }

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
            return DateTimeUtils.ToRfc1123String((DateTimeOffset)(object)value);
        }

        // DateTime
        return type == typeof(DateTime) ? DateTimeUtils.ToRfc1123String(new DateTimeOffset((DateTime)(object)value)) : value.ToString();
    }

    private SyndicationContent CreateCommentsContent(ISyndicationLink link)
    {
        ArgumentNullException.ThrowIfNull(link?.RelationshipType);
        return new SyndicationContent(link.RelationshipType)
        {
            Value = FormatValue(link.Uri)
        };
    }

    private SyndicationContent CreateEnclosureContent(ISyndicationLink link)
    {
        var content = new SyndicationContent(RssElementNames.Enclosure);

        // Url
        content.AddAttribute(new SyndicationAttribute(RssElementNames.Url, FormatValue(link.Uri)));

        // Length
        if (link.Length == 0)
        {
            throw new ArgumentException("Enclosure requires length attribute");
        }

        content.AddAttribute(new SyndicationAttribute(RssConstants.Length, FormatValue(link.Length)));

        // MediaType
        if (string.IsNullOrEmpty(link.MediaType))
        {
            throw new ArgumentNullException(nameof(link), "Enclosure requires a MediaType");
        }

        content.AddAttribute(new SyndicationAttribute(RssConstants.Type, link.MediaType));
        return content;
    }

    private SyndicationContent CreateLinkContent(ISyndicationLink link)
    {
        SyndicationContent content;

        if (string.IsNullOrEmpty(link.RelationshipType) ||
            link.RelationshipType == RssLinkTypes.Alternate)
        {
            // Regular <link>
            content = new SyndicationContent(RssElementNames.Link);
        }
        else
        {
            // Custom
            content = new SyndicationContent(link.RelationshipType);
        }

        // title
        if (!string.IsNullOrEmpty(link.Title))
        {
            content.Value = link.Title;
        }

        // url
        string? url = FormatValue(link.Uri);

        if (content.Value is not null)
        {
            content.AddAttribute(new SyndicationAttribute(RssElementNames.Url, url));
        }
        else
        {
            content.Value = url;
        }

        // Type
        if (!string.IsNullOrEmpty(link.MediaType))
        {
            content.AddAttribute(new SyndicationAttribute(RssConstants.Type, link.MediaType));
        }

        // Lenght
        if (link.Length != 0)
        {
            content.AddAttribute(new SyndicationAttribute(RssConstants.Length, FormatValue(link.Length)));
        }

        return content;
    }

    private SyndicationContent CreateSourceContent(ISyndicationLink link)
    {
        ArgumentNullException.ThrowIfNull(link?.RelationshipType);
        var content = new SyndicationContent(link.RelationshipType);

        // Url
        string? url = FormatValue(link.Uri);
        if (link.Title != url)
        {
            content.AddAttribute(new SyndicationAttribute(RssElementNames.Url, url));
        }

        // Title
        if (!string.IsNullOrEmpty(link.Title))
        {
            content.Value = link.Title;
        }

        return content;
    }

    private void WriteSyndicationContent(ISyndicationContent content)
    {
        // Write Start
        _writer.WriteStartSyndicationContent(content, null);

        // Write attributes
        if (content.Attributes is not null)
        {
            foreach (ISyndicationAttribute a in content.Attributes)
            {
                _writer.WriteSyndicationAttribute(a);
            }
        }

        // Write value
        if (content.Value is not null)
        {
            _writer.WriteString(content.Value, UseCDATA);
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
