// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a syndication link, which is a hyperlink associated with a syndication feed.
/// </summary>
public interface ISyndicationLink
{
    /// <summary>
    /// Gets the date and time when the syndication link was last updated.
    /// </summary>
    DateTimeOffset LastUpdated { get; }

    /// <summary>
    /// Gets the length of the syndication link content in bytes.
    /// </summary>
    long Length { get; }

    /// <summary>
    /// Gets the media type of the syndication link.
    /// </summary>
    string MediaType { get; }

    /// <summary>
    /// Gets the relationship type of the syndication link.
    /// </summary>
    string RelationshipType { get; }

    /// <summary>
    /// Gets the title of the syndication link.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the URI of the syndication link.
    /// </summary>
    Uri Uri { get; }
}