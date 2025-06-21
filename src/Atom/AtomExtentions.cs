// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed.Atom;

/// <summary>
/// Provides extension methods for <see cref="ISyndicationAttribute"/> to determine if it is an Atom attribute.
/// </summary>
internal static class AtomAttributeExtentions
{
    /// <summary>
    /// Gets the atom.
    /// </summary>
    /// <param name="attributes">The attributes.</param>
    /// <param name="name">The name.</param>
    /// <returns>System.Nullable&lt;System.String&gt;.</returns>
    public static string? GetAtom(this IEnumerable<ISyndicationAttribute> attributes, string name) => attributes.FirstOrDefault(a => a.IsAtom(name))?.Value;

    /// <summary>
    /// Determines whether the specified name is atom.
    /// </summary>
    /// <param name="attr">The attribute.</param>
    /// <param name="name">The name.</param>
    /// <returns><c>true</c> if the specified name is atom; otherwise, <c>false</c>.</returns>
    public static bool IsAtom(this ISyndicationAttribute attr, string name) => attr.Name == name && (attr.Namespace == null || attr.Namespace == string.Empty || attr.Namespace == AtomConstants.Atom10Namespace);
}

/// <summary>
/// Provides extension methods for <see cref="ISyndicationContent"/> to determine if it is an Atom content.
/// </summary>
internal static class AtomContentExtentions
{
    /// <summary>
    /// Determines whether the specified name is atom.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="name">The name.</param>
    /// <returns><c>true</c> if the specified name is atom; otherwise, <c>false</c>.</returns>
    public static bool IsAtom(this ISyndicationContent content, string name) => content.Name == name && (content.Namespace == null || content.Namespace == AtomConstants.Atom10Namespace);

    /// <summary>
    /// Determines whether the specified content is atom.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns><c>true</c> if the specified content is atom; otherwise, <c>false</c>.</returns>
    public static bool IsAtom(this ISyndicationContent content) => content.Namespace is null or AtomConstants.Atom10Namespace;
}