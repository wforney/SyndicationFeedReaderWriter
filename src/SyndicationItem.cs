// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a syndication item, which is a single entry in a syndication feed. Implements the
/// <see cref="ISyndicationItem"/>
/// </summary>
/// <seealso cref="ISyndicationItem"/>
public class SyndicationItem : ISyndicationItem
{
    private ICollection<ISyndicationCategory> _categories = [];
    private ICollection<ISyndicationPerson> _contributors = [];
    private ICollection<ISyndicationLink> _links = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationItem"/> class.
    /// </summary>
    public SyndicationItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationItem"/> class.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <exception cref="ArgumentNullException">item</exception>
    public SyndicationItem(ISyndicationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Id = item.Id;
        Title = item.Title;
        Description = item.Description;
        LastUpdated = item.LastUpdated;
        Published = item.Published;

        // Copy collections only if needed
        _categories = item.Categories as ICollection<ISyndicationCategory> ?? [.. item.Categories];
        _contributors = item.Contributors as ICollection<ISyndicationPerson> ?? [.. item.Contributors];
        _links = item.Links as ICollection<ISyndicationLink> ?? [.. item.Links];
    }

    /// <inheritdoc/>
    public IEnumerable<ISyndicationCategory> Categories => _categories;

    /// <inheritdoc/>
    public IEnumerable<ISyndicationPerson> Contributors => _contributors;

    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <inheritdoc/>
    public string? Id { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset LastUpdated { get; set; }

    /// <inheritdoc/>
    public IEnumerable<ISyndicationLink> Links => _links;

    /// <inheritdoc/>
    public DateTimeOffset Published { get; set; }

    /// <inheritdoc/>
    public string? Title { get; set; }

    /// <inheritdoc/>
    public void AddCategory(ISyndicationCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (_categories.IsReadOnly)
        {
            _categories = [.. _categories];
        }

        _categories.Add(category);
    }

    /// <inheritdoc/>
    public void AddContributor(ISyndicationPerson person)
    {
        ArgumentNullException.ThrowIfNull(person);

        if (_contributors.IsReadOnly)
        {
            _contributors = [.. _contributors];
        }

        _contributors.Add(person);
    }

    /// <inheritdoc/>
    public void AddLink(ISyndicationLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        if (_links.IsReadOnly)
        {
            _links = [.. _links];
        }

        _links.Add(link);
    }
}