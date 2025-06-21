// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a person associated with a syndication feed, such as an author or contributor.
/// </summary>
public interface ISyndicationPerson
{
    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the name of the person.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the relationship type of the person (e.g., "author", "contributor").
    /// </summary>
    string? RelationshipType { get; }

    /// <summary>
    /// Gets the URI associated with the person.
    /// </summary>
    string? Uri { get; }
}