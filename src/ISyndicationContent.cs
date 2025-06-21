// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a content element in a syndication feed, which can have attributes, fields, and a value.
/// </summary>
public interface ISyndicationContent
{
    /// <summary>
    /// Gets the attributes.
    /// </summary>
    /// <value>The attributes.</value>
    IEnumerable<ISyndicationAttribute> Attributes { get; }

    /// <summary>
    /// Gets the fields.
    /// </summary>
    /// <value>The fields.</value>
    IEnumerable<ISyndicationContent> Fields { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    string Name { get; }

    /// <summary>
    /// Gets the namespace.
    /// </summary>
    /// <value>The namespace.</value>
    string? Namespace { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    string? Value { get; }
}