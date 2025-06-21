// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Defines methods for formatting syndication feed elements into string representations.
/// </summary>
public interface ISyndicationFeedFormatter
{
    /// <summary>
    /// Formats the specified syndication content into a string representation.
    /// </summary>
    /// <param name="content">The syndication content to format.</param>
    /// <returns>A string representation of the syndication content.</returns>
    string Format(ISyndicationContent content);

    /// <summary>
    /// Formats the specified syndication category into a string representation.
    /// </summary>
    /// <param name="category">The syndication category to format.</param>
    /// <returns>A string representation of the syndication category.</returns>
    string Format(ISyndicationCategory category);

    /// <summary>
    /// Formats the specified syndication image into a string representation.
    /// </summary>
    /// <param name="image">The syndication image to format.</param>
    /// <returns>A string representation of the syndication image.</returns>
    string Format(ISyndicationImage image);

    /// <summary>
    /// Formats the specified syndication item into a string representation.
    /// </summary>
    /// <param name="item">The syndication item to format.</param>
    /// <returns>A string representation of the syndication item.</returns>
    string Format(ISyndicationItem item);

    /// <summary>
    /// Formats the specified syndication person into a string representation.
    /// </summary>
    /// <param name="person">The syndication person to format.</param>
    /// <returns>A string representation of the syndication person.</returns>
    string Format(ISyndicationPerson person);

    /// <summary>
    /// Formats the specified syndication link into a string representation.
    /// </summary>
    /// <param name="link">The syndication link to format.</param>
    /// <returns>A string representation of the syndication link.</returns>
    string Format(ISyndicationLink link);

    /// <summary>
    /// Formats the specified value into a string representation.
    /// </summary>
    /// <typeparam name="T">The type of the value to format.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <returns>A string representation of the value.</returns>
    string? FormatValue<T>(T value);
}