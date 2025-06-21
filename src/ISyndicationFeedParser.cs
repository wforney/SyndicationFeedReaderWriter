// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Defines methods for parsing syndication feed elements such as items, links, persons, categories,
/// images, and content.
/// </summary>
public interface ISyndicationFeedParser
{
    /// <summary>
    /// Parses a syndication category from the provided string value.
    /// </summary>
    /// <param name="value">The string representation of the syndication category.</param>
    /// <returns>An <see cref="ISyndicationCategory"/> instance.</returns>
    ISyndicationCategory ParseCategory(string value);

    /// <summary>
    /// Parses syndication content from the provided string value.
    /// </summary>
    /// <param name="value">The string representation of the syndication content.</param>
    /// <returns>An <see cref="ISyndicationContent"/> instance.</returns>
    ISyndicationContent ParseContent(string value);

    /// <summary>
    /// Parses a syndication image from the provided string value.
    /// </summary>
    /// <param name="value">The string representation of the syndication image.</param>
    /// <returns>An <see cref="ISyndicationImage"/> instance.</returns>
    ISyndicationImage ParseImage(string value);

    /// <summary>
    /// Parses a syndication item from the provided string value.
    /// </summary>
    /// <param name="value">The string representation of the syndication item.</param>
    /// <returns>An <see cref="ISyndicationItem"/> instance.</returns>
    ISyndicationItem ParseItem(string value);

    /// <summary>
    /// Parses a syndication link from the provided string value.
    /// </summary>
    /// <param name="value">The string representation of the syndication link.</param>
    /// <returns>An <see cref="ISyndicationLink"/> instance.</returns>
    ISyndicationLink ParseLink(string value);

    /// <summary>
    /// Parses a syndication person from the provided string value.
    /// </summary>
    /// <param name="value">The string representation of the syndication person.</param>
    /// <returns>An <see cref="ISyndicationPerson"/> instance.</returns>
    ISyndicationPerson ParsePerson(string value);

    /// <summary>
    /// Attempts to parse a value of type <typeparamref name="T"/> from the provided string value.
    /// </summary>
    /// <typeparam name="T">The type of the value to parse.</typeparam>
    /// <param name="value">The string representation of the value.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed value if the operation succeeded; otherwise,
    /// the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns><c>true</c> if the value was successfully parsed; otherwise, <c>false</c>.</returns>
    bool TryParseValue<T>(string? value, [NotNullWhen(true)] out T? result);
}