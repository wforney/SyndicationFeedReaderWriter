// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a syndication attribute. This class cannot be inherited. Implements the <see cref="ISyndicationAttribute"/>
/// </summary>
/// <seealso cref="ISyndicationAttribute"/>
public sealed class SyndicationAttribute : ISyndicationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationAttribute"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public SyndicationAttribute(string name, string? value) : this(name, null, value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationAttribute"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="ns">The namespace.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    public SyndicationAttribute(string name, string? ns, string? value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
        Namespace = ns;
    }

    /// <inheritdoc/>
    public string Name { get; private set; } = default!;

    /// <inheritdoc/>
    public string? Namespace { get; private set; }

    /// <inheritdoc/>
    public string? Value { get; private set; }
}