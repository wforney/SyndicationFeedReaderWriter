// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.SyndicationFeed.Rss;

/// <summary>
/// Provides constants for the RSS 2.0 specification.
/// </summary>
internal static class RssConstants
{
    /// <summary>
    /// The domain attribute for RSS elements.
    /// </summary>
    public const string Domain = "domain";

    /// <summary>
    /// The isPermaLink attribute for RSS elements.
    /// </summary>
    public const string IsPermaLink = "isPermaLink";

    /// <summary>
    /// The length attribute for RSS elements.
    /// </summary>
    public const string Length = "length";

    /// <summary>
    /// The namespace for RSS 2.0 (empty string as RSS 2.0 does not use a namespace).
    /// </summary>
    public const string Rss20Namespace = "";

    /// <summary>
    /// The specification link for RSS 2.0.
    /// </summary>
    public const string SpecificationLink = "http://blogs.law.harvard.edu/tech/rss";

    /// <summary>
    /// The type attribute for RSS elements.
    /// </summary>
    public const string Type = "type";

    /// <summary>
    /// The version value for RSS 2.0.
    /// </summary>
    public const string Version = "2.0";
}