// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed.Utils;
using System.Xml;

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Abstract base class for writing syndication feeds in XML format. Implements the <see cref="ISyndicationFeedWriter"/>
/// </summary>
/// <seealso cref="ISyndicationFeedWriter"/>
public abstract class XmlFeedWriter(XmlWriter writer, ISyndicationFeedFormatter formatter) : ISyndicationFeedWriter
{
    private readonly XmlWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));

    /// <inheritdoc/>
    public ISyndicationFeedFormatter Formatter { get; private set; } = formatter ?? throw new ArgumentNullException(nameof(formatter));

    /// <inheritdoc/>
    public Task Flush() => XmlUtils.FlushAsync(_writer);

    /// <inheritdoc/>
    public virtual Task Write(ISyndicationContent content) => WriteRaw(Formatter.Format(content ?? throw new ArgumentNullException(nameof(content))));

    /// <inheritdoc/>
    public virtual Task Write(ISyndicationCategory category) => WriteRaw(Formatter.Format(category ?? throw new ArgumentNullException(nameof(category))));

    /// <inheritdoc/>
    public virtual Task Write(ISyndicationImage image) => WriteRaw(Formatter.Format(image ?? throw new ArgumentNullException(nameof(image))));

    /// <inheritdoc/>
    public virtual Task Write(ISyndicationItem item) => WriteRaw(Formatter.Format(item ?? throw new ArgumentNullException(nameof(item))));

    /// <inheritdoc/>
    public virtual Task Write(ISyndicationPerson person) => WriteRaw(Formatter.Format(person ?? throw new ArgumentNullException(nameof(person))));

    /// <inheritdoc/>
    public virtual Task Write(ISyndicationLink link) => WriteRaw(Formatter.Format(link ?? throw new ArgumentNullException(nameof(link))));

    /// <inheritdoc/>
    public virtual Task WriteRaw(string content) => XmlUtils.WriteRawAsync(_writer, content);

    /// <inheritdoc/>
    public virtual Task WriteValue<T>(string name, T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        string valueString = Formatter.FormatValue(value);

        return valueString is null
            ? throw new FormatException(nameof(value))
            : WriteRaw(Formatter.Format(new SyndicationContent(name, valueString)));
    }
}