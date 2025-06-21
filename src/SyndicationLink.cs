// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a syndication link, which is a URI with additional metadata such as title, media
/// type, relationship type, length, and last updated time. This class cannot be inherited.
/// Implements the <see cref="ISyndicationLink"/>
/// </summary>
/// <param name="url">The URL.</param>
/// <param name="relationshipType">Type of the relationship.</param>
/// <seealso cref="ISyndicationLink"/>
public sealed class SyndicationLink(Uri url, string? relationshipType = null) : ISyndicationLink
{
    /// <inheritdoc/>
    public DateTimeOffset LastUpdated { get; set; }

    /// <inheritdoc/>
    public long Length { get; set; }

    /// <inheritdoc/>
    public string? MediaType { get; set; }

    /// <inheritdoc/>
    public string? RelationshipType { get; } = relationshipType;

    /// <inheritdoc/>
    public string? Title { get; set; }

    /// <inheritdoc/>
    public Uri Uri { get; private set; } = url ?? throw new ArgumentNullException(nameof(url));
}