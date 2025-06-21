// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Xml;

namespace Microsoft.SyndicationFeed.Atom;

/// <summary>
/// Represents an Atom feed writer that writes Atom feeds in XML format. Implements the <see cref="XmlFeedWriter"/>
/// </summary>
/// <seealso cref="XmlFeedWriter"/>
public class AtomFeedWriter : XmlFeedWriter
{
    private readonly IEnumerable<ISyndicationAttribute> _attributes;
    private readonly XmlWriter _writer;

    private bool _feedStarted;

    /// <inheritdoc/>
    public AtomFeedWriter(XmlWriter writer, IEnumerable<ISyndicationAttribute>? attributes = null)
        : this(writer, attributes, null)
    {
    }

    /// <inheritdoc/>
    public AtomFeedWriter(XmlWriter writer, IEnumerable<ISyndicationAttribute>? attributes, ISyndicationFeedFormatter? formatter) :
        this(writer, formatter, EnsureXmlNs(attributes ?? []))
    {
    }

    private AtomFeedWriter(XmlWriter writer, ISyndicationFeedFormatter? formatter, IEnumerable<ISyndicationAttribute> attributes) :
        base(writer, formatter ?? new AtomFormatter(attributes, writer.Settings))
    {
        _writer = writer;
        _attributes = attributes;
    }

    /// <inheritdoc/>
    public virtual Task WriteGenerator(string value, string uri, string version)
    {
        ArgumentNullException.ThrowIfNull(value);

        var generator = new SyndicationContent(AtomElementNames.Generator, value);

        if (!string.IsNullOrEmpty(uri))
        {
            generator.AddAttribute(new SyndicationAttribute("uri", uri));
        }

        if (!string.IsNullOrEmpty(version))
        {
            generator.AddAttribute(new SyndicationAttribute("version", version));
        }

        return Write(generator);
    }

    /// <inheritdoc/>
    public virtual Task WriteId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return WriteValue(AtomElementNames.Id, value);
    }

    /// <inheritdoc/>
    public override Task WriteRaw(string content)
    {
        if (!_feedStarted)
        {
            StartFeed();
        }

        return XmlUtils.WriteRawAsync(_writer, content);
    }

    /// <inheritdoc/>
    public virtual Task WriteRights(string value) => WriteText(AtomElementNames.Rights, value, null);

    /// <inheritdoc/>
    public virtual Task WriteSubtitle(string value) => WriteText(AtomElementNames.Subtitle, value, null);

    /// <inheritdoc/>
    public virtual Task WriteText(string name, string value, string? type)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(value);

        var content = new SyndicationContent(name, value);

        if (!string.IsNullOrEmpty(type))
        {
            content.AddAttribute(new SyndicationAttribute(AtomConstants.Type, type));
        }

        return Write(content);
    }

    /// <inheritdoc/>
    public virtual Task WriteTitle(string value) => WriteText(AtomElementNames.Title, value, null);

    /// <inheritdoc/>
    public virtual Task WriteUpdated(DateTimeOffset dt)
    {
        return dt == default
            ? throw new ArgumentException("DateTimeOffset cannot be default.", nameof(dt))
            : WriteValue(AtomElementNames.Updated, dt);
    }

    private static IEnumerable<ISyndicationAttribute> EnsureXmlNs(IEnumerable<ISyndicationAttribute> attributes)
    {
        ISyndicationAttribute? xmlnsAttr = attributes.FirstOrDefault(a => a.Name.StartsWith("xmlns") && a.Value == AtomConstants.Atom10Namespace);

        // Insert Atom namespace if it doesn't already exist
        if (xmlnsAttr is null)
        {
            var list = new List<ISyndicationAttribute>(attributes);
            list.Insert(0, new SyndicationAttribute("xmlns", AtomConstants.Atom10Namespace));

            attributes = list;
        }

        return attributes;
    }

    private void StartFeed()
    {
        ISyndicationAttribute? xmlns = _attributes.FirstOrDefault(a => a.Name == "xmlns");

        // Write <feed>
        if (xmlns is null)
        {
            _writer.WriteStartElement(AtomElementNames.Feed);
        }
        else
        {
            _writer.WriteStartElement(AtomElementNames.Feed, xmlns.Value);
        }

        // Add attributes
        foreach (ISyndicationAttribute a in _attributes)
        {
            if (a != xmlns)
            {
                _writer.WriteSyndicationAttribute(a);
            }
        }

        _feedStarted = true;
    }
}