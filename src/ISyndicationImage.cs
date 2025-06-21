// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents an image associated with a syndication feed.
/// </summary>
public interface ISyndicationImage
{
    /// <summary>
    /// Gets the description of the image.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the link associated with the image.
    /// </summary>
    ISyndicationLink? Link { get; }

    /// <summary>
    /// Gets the relationship type of the image.
    /// </summary>
    string? RelationshipType { get; }

    /// <summary>
    /// Gets the title of the image.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Gets the URL of the image.
    /// </summary>
    Uri Url { get; }
}