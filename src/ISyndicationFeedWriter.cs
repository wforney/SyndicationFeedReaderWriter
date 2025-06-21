// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Defines methods for writing syndication feed elements such as content, categories, images,
/// items, persons, and links.
/// </summary>
public interface ISyndicationFeedWriter
{
    /// <summary>
    /// Writes syndication content to the feed.
    /// </summary>
    /// <param name="content">The syndication content to write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Write(ISyndicationContent content);

    /// <summary>
    /// Writes a syndication category to the feed.
    /// </summary>
    /// <param name="category">The syndication category to write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Write(ISyndicationCategory category);

    /// <summary>
    /// Writes a syndication image to the feed.
    /// </summary>
    /// <param name="image">The syndication image to write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Write(ISyndicationImage image);

    /// <summary>
    /// Writes a syndication item to the feed.
    /// </summary>
    /// <param name="item">The syndication item to write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Write(ISyndicationItem item);

    /// <summary>
    /// Writes a syndication person to the feed.
    /// </summary>
    /// <param name="person">The syndication person to write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Write(ISyndicationPerson person);

    /// <summary>
    /// Writes a syndication link to the feed.
    /// </summary>
    /// <param name="link">The syndication link to write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Write(ISyndicationLink link);

    /// <summary>
    /// Writes raw content to the feed.
    /// </summary>
    /// <param name="content">The raw content to write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteRaw(string content);

    /// <summary>
    /// Writes a named value to the feed.
    /// </summary>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    /// <param name="name">The name of the value.</param>
    /// <param name="value">The value to write.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteValue<T>(string name, T value);
}