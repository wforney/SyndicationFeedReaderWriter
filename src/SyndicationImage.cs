// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a syndication image, which is an image associated with a syndication feed or entry.
/// This class cannot be inherited. Implements the <see cref="ISyndicationImage"/>
/// </summary>
/// <param name="url">The URL.</param>
/// <param name="relationshipType">Type of the relationship.</param>
/// <seealso cref="ISyndicationImage"/>
public sealed class SyndicationImage(Uri url, string? relationshipType = null) : ISyndicationImage
{
    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <inheritdoc/>
    public ISyndicationLink? Link { get; set; }

    /// <inheritdoc/>
    public string? RelationshipType { get; set; } = relationshipType;

    /// <inheritdoc/>
    public string? Title { get; set; }

    /// <inheritdoc/>
    public Uri Url { get; private set; } = url ?? throw new ArgumentNullException(nameof(url));
}