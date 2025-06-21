// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed;

/// <summary>
/// Represents a reader for syndication feeds, providing methods to read various elements of the feed.
/// </summary>
public interface ISyndicationFeedReader
{
    /// <summary>
    /// Gets the name of the current syndication element.
    /// </summary>
    string? ElementName { get; }

    /// <summary>
    /// Gets the type of the current syndication element.
    /// </summary>
    SyndicationElementType ElementType { get; }

    /// <summary>
    /// Reads the next element in the syndication feed.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean
    /// indicating whether the read was successful.
    /// </returns>
    Task<bool> Read();

    /// <summary>
    /// Reads the current syndication category.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the syndication category.
    /// </returns>
    Task<ISyndicationCategory> ReadCategory();

    /// <summary>
    /// Reads the current syndication content.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the syndication content.
    /// </returns>
    Task<ISyndicationContent> ReadContent();

    /// <summary>
    /// Reads the current element as a string.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the string
    /// representation of the element.
    /// </returns>
    Task<string> ReadElementAsString();

    /// <summary>
    /// Reads the current syndication image.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the syndication image.
    /// </returns>
    Task<ISyndicationImage> ReadImage();

    /// <summary>
    /// Reads the current syndication item.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the syndication item.
    /// </returns>
    Task<ISyndicationItem> ReadItem();

    /// <summary>
    /// Reads the current syndication link.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the syndication link.
    /// </returns>
    Task<ISyndicationLink> ReadLink();

    /// <summary>
    /// Reads the current syndication person.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the syndication person.
    /// </returns>
    Task<ISyndicationPerson> ReadPerson();

    /// <summary>
    /// Reads the value of the current element as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the value should be converted.</typeparam>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the value of the element.
    /// </returns>
    Task<T> ReadValue<T>();

    /// <summary>
    /// Skips the current element in the syndication feed.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Skip();
}