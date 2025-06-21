// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

public sealed class SyndicationCategory(string name) : ISyndicationCategory
{
    /// <inheritdoc/>
    public string? Label { get; set; }

    /// <inheritdoc/>
    public string Name { get; private set; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <inheritdoc/>
    public string? Scheme { get; set; }
}