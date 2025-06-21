// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a syndication item, such as an entry or article in a feed.
/// </summary>
public interface ISyndicationItem
{
    /// <summary>
    /// Gets the categories associated with the syndication item.
    /// </summary>
    IEnumerable<ISyndicationCategory> Categories { get; }

    /// <summary>
    /// Gets the contributors to the syndication item.
    /// </summary>
    IEnumerable<ISyndicationPerson> Contributors { get; }

    /// <summary>
    /// Gets the description or summary of the syndication item.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the unique identifier of the syndication item.
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// Gets the date and time when the syndication item was last updated.
    /// </summary>
    DateTimeOffset LastUpdated { get; }

    /// <summary>
    /// Gets the links associated with the syndication item.
    /// </summary>
    IEnumerable<ISyndicationLink> Links { get; }

    /// <summary>
    /// Gets the date and time when the syndication item was published.
    /// </summary>
    DateTimeOffset Published { get; }

    /// <summary>
    /// Gets the title of the syndication item.
    /// </summary>
    string? Title { get; }
}