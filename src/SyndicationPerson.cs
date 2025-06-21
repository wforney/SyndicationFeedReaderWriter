// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a person associated with a syndication feed, such as an author or contributor. This
/// class cannot be inherited. Implements the <see cref="ISyndicationPerson"/>
/// </summary>
/// <seealso cref="ISyndicationPerson"/>
public sealed class SyndicationPerson : ISyndicationPerson
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationPerson"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="email">The email.</param>
    /// <param name="relationshipType">Type of the relationship.</param>
    /// <exception cref="ArgumentNullException">Valid name or email is required</exception>
    public SyndicationPerson(string name, string email, string? relationshipType = null)
    {
        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email))
        {
            throw new ArgumentNullException(nameof(name), "Valid name or email is required");
        }

        Name = name;
        Email = email;
        RelationshipType = relationshipType;
    }

    /// <inheritdoc/>
    public string? Email { get; private set; }

    /// <inheritdoc/>
    public string? Name { get; private set; }

    /// <inheritdoc/>
    public string? RelationshipType { get; set; }

    /// <inheritdoc/>
    public string? Uri { get; set; }
}