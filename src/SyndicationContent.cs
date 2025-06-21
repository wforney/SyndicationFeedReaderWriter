// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this
// file to you under the MIT license. See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a content element in a syndication feed. Implements the <see cref="ISyndicationContent"/>
/// </summary>
/// <seealso cref="ISyndicationContent"/>
public class SyndicationContent : ISyndicationContent
{
    private ICollection<ISyndicationAttribute> _attributes;
    private ICollection<ISyndicationContent> _children;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationContent"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public SyndicationContent(string name, string? value = null)
        : this(name, null, value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationContent"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="ns">The namespace.</param>
    /// <param name="value">The value.</param>
    public SyndicationContent(string name, string? ns, string? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
        Value = value;
        Namespace = ns;

        _attributes = [];
        _children = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationContent"/> class.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <exception cref="ArgumentNullException">content</exception>
    public SyndicationContent(ISyndicationContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        Name = content.Name;
        Namespace = content.Namespace;
        Value = content.Value;

        // Copy collections only if needed
        _attributes = content.Attributes as ICollection<ISyndicationAttribute> ?? [.. content.Attributes];
        _children = content.Fields as ICollection<ISyndicationContent> ?? [.. content.Fields];
    }

    /// <inheritdoc/>
    public IEnumerable<ISyndicationAttribute> Attributes => _attributes;

    /// <inheritdoc/>
    public IEnumerable<ISyndicationContent> Fields => _children;

    /// <inheritdoc/>
    public string Name { get; private set; }

    /// <inheritdoc/>
    public string? Namespace { get; private set; }

    /// <inheritdoc/>
    public string? Value { get; set; }

    /// <summary>
    /// Adds the attribute.
    /// </summary>
    /// <param name="attribute">The attribute.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddAttribute(ISyndicationAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        if (_attributes.IsReadOnly)
        {
            _attributes = [.. _attributes];
        }

        _attributes.Add(attribute);
    }

    /// <summary>
    /// Adds the field.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddField(ISyndicationContent field)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (_children.IsReadOnly)
        {
            _children = [.. _children];
        }

        _children.Add(field);
    }
}