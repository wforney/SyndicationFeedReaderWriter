// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed
{
    /// <summary>
    /// Represents the types of elements that can appear in a syndication feed.
    /// </summary>
    public enum SyndicationElementType
    {
        /// <summary>
        /// No element type specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents an item element, such as an entry or article.
        /// </summary>
        Item = 1,

        /// <summary>
        /// Represents a person element, such as an author or contributor.
        /// </summary>
        Person = 2,

        /// <summary>
        /// Represents a link element.
        /// </summary>
        Link = 3,

        /// <summary>
        /// Represents a content element.
        /// </summary>
        Content = 4,

        /// <summary>
        /// Represents a category element.
        /// </summary>
        Category = 5,

        /// <summary>
        /// Represents an image element.
        /// </summary>
        Image = 6
    }
}