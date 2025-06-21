// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed.Atom;

/// <summary>
/// Represents an Atom entry in a syndication feed.
/// </summary>
public interface IAtomEntry : ISyndicationItem
{
    /// <summary>
    /// Gets the content type of the Atom entry.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Gets the rights information of the Atom entry, if available.
    /// </summary>
    string? Rights { get; }

    /// <summary>
    /// Gets the summary of the Atom entry.
    /// </summary>
    string? Summary { get; }
}