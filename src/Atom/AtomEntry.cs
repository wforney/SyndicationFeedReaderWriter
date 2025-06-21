// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed.Atom;

/// <summary>
/// Represents an Atom entry, which is a single entry in an Atom feed.
/// </summary>
public class AtomEntry : SyndicationItem, IAtomEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomEntry"/> class.
    /// </summary>
    public AtomEntry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomEntry"/> class by copying values from
    /// another <see cref="IAtomEntry"/> instance.
    /// </summary>
    /// <param name="item">The <see cref="IAtomEntry"/> instance to copy values from.</param>
    public AtomEntry(IAtomEntry item)
        : base(item)
    {
        ContentType = item.ContentType;
        Summary = item.Summary;
        Rights = item.Rights;
    }

    /// <summary>
    /// Gets or sets the content type of the Atom entry.
    /// </summary>
    public string ContentType { get; set; } = default!;

    /// <summary>
    /// Gets or sets the rights associated with the Atom entry.
    /// </summary>
    public string? Rights { get; set; }

    /// <summary>
    /// Gets or sets the summary of the Atom entry.
    /// </summary>
    public string? Summary { get; set; }
}