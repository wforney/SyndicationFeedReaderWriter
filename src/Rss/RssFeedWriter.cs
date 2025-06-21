// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Globalization;
using System.Xml;

namespace Microsoft.SyndicationFeed.Rss;

/// <summary>
/// Represents a writer for RSS feeds. Implements the <see cref="XmlFeedWriter"/>
/// </summary>
/// <seealso cref="XmlFeedWriter"/>
/// <remarks>Initializes a new instance of the <see cref="RssFeedWriter"/> class.</remarks>
/// <param name="writer">The writer.</param>
/// <param name="attributes">The attributes.</param>
/// <param name="formatter">The formatter.</param>
public class RssFeedWriter(
    XmlWriter writer,
    IEnumerable<ISyndicationAttribute>? attributes,
    ISyndicationFeedFormatter? formatter)
    : XmlFeedWriter(
        writer,
        formatter ?? new RssFormatter(attributes, writer.Settings))
{
    private bool _feedStarted;

    /// <summary>
    /// Initializes a new instance of the <see cref="RssFeedWriter"/> class.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="attributes">The attributes.</param>
    public RssFeedWriter(XmlWriter writer, IEnumerable<ISyndicationAttribute>? attributes = null)
        : this(writer, attributes, null)
    {
    }

    /// <inheritdoc/>
    public virtual Task WriteCloud(Uri uri, string registerProcedure, string protocol)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (!uri.IsAbsoluteUri)
        {
            throw new ArgumentException("Absolute uri required");
        }

        if (string.IsNullOrEmpty(registerProcedure))
        {
            throw new ArgumentNullException(nameof(registerProcedure));
        }

        var cloud = new SyndicationContent(RssElementNames.Cloud);

        cloud.AddAttribute(new SyndicationAttribute("domain", uri.GetComponents(UriComponents.Host, UriFormat.UriEscaped)));
        cloud.AddAttribute(new SyndicationAttribute("port", uri.GetComponents(UriComponents.StrongPort, UriFormat.UriEscaped)));
        cloud.AddAttribute(new SyndicationAttribute("path", uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped)));
        cloud.AddAttribute(new SyndicationAttribute("registerProcedure", registerProcedure));
        cloud.AddAttribute(new SyndicationAttribute("protocol", protocol ?? "xml-rpc"));

        return Write(cloud);
    }

    /// <inheritdoc/>
    public virtual Task WriteCopyright(string value) => value is null ? throw new ArgumentNullException(nameof(value)) : WriteValue(RssElementNames.Copyright, value);

    /// <inheritdoc/>
    public virtual Task WriteDescription(string value) => value is null ? throw new ArgumentNullException(nameof(value)) : WriteValue(RssElementNames.Description, value);

    /// <inheritdoc/>
    public virtual Task WriteDocs() => WriteValue(RssElementNames.Docs, RssConstants.SpecificationLink);

    /// <inheritdoc/>
    public virtual Task WriteGenerator(string value) => value is null ? throw new ArgumentNullException(nameof(value)) : WriteValue(RssElementNames.Generator, value);

    /// <inheritdoc/>
    public virtual Task WriteLanguage(CultureInfo culture) => culture is null ? throw new ArgumentNullException(nameof(culture)) : WriteValue(RssElementNames.Language, culture.Name);

    /// <inheritdoc/>
    public virtual Task WriteLastBuildDate(DateTimeOffset dt) => dt == default ? throw new ArgumentNullException(nameof(dt)) : WriteValue(RssElementNames.LastBuildDate, dt);

    /// <inheritdoc/>
    public virtual Task WritePubDate(DateTimeOffset dt) => dt == default ? throw new ArgumentNullException(nameof(dt)) : WriteValue(RssElementNames.PubDate, dt);

    /// <inheritdoc/>
    public override Task WriteRaw(string content)
    {
        if (!_feedStarted)
        {
            StartFeed();
        }

        return XmlUtils.WriteRawAsync(Writer, content);
    }

    /// <inheritdoc/>
    public virtual Task WriteSkipDays(IEnumerable<DayOfWeek> days)
    {
        ArgumentNullException.ThrowIfNull(days);

        var skipDays = new SyndicationContent(RssElementNames.SkipDays);

        foreach (DayOfWeek d in days)
        {
            skipDays.AddField(new SyndicationContent("day", Formatter.FormatValue(d)));
        }

        return Write(skipDays);
    }

    /// <inheritdoc/>
    public virtual Task WriteSkipHours(IEnumerable<byte> hours)
    {
        ArgumentNullException.ThrowIfNull(hours);

        var skipHours = new SyndicationContent(RssElementNames.SkipHours);

        foreach (byte h in hours)
        {
            if (h is < 0 or > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hours), "Hour value must be between 0 and 23");
            }

            skipHours.AddField(new SyndicationContent("hour", Formatter.FormatValue(h)));
        }

        return Write(skipHours);
    }

    /// <inheritdoc/>
    public virtual Task WriteTimeToLive(TimeSpan ttl)
    {
        return ttl == default
            ? throw new ArgumentNullException(nameof(ttl))
            : WriteValue(RssElementNames.TimeToLive, (long)Math.Max(1, Math.Ceiling(ttl.TotalMinutes)));
    }

    /// <inheritdoc/>
    public virtual Task WriteTitle(string value) => value is null ? throw new ArgumentNullException(nameof(value)) : WriteValue(RssElementNames.Title, value);

    private void StartFeed()
    {
        // Write <rss version="2.0">
        Writer.WriteStartElement(RssElementNames.Rss);

        // Write attributes if exist
        if (attributes is not null)
        {
            foreach (ISyndicationAttribute a in attributes)
            {
                Writer.WriteSyndicationAttribute(a);
            }
        }

        Writer.WriteAttributeString(RssElementNames.Version, RssConstants.Version);
        Writer.WriteStartElement(RssElementNames.Channel);
        _feedStarted = true;
    }
}