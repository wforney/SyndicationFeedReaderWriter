// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents an attribute in a syndication feed.
/// </summary>
public interface ISyndicationAttribute
{
    /// <summary>
    /// Gets the name of the attribute.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the namespace of the attribute.
    /// </summary>
    string? Namespace { get; }

    /// <summary>
    /// Gets the value of the attribute.
    /// </summary>
    string? Value { get; }
}