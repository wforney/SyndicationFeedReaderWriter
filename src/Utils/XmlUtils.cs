// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Xml;

namespace Microsoft.SyndicationFeed.Utils;

/// <summary>
/// Provides utility methods for working with XML in syndication feeds.
/// </summary>
internal static class XmlUtils
{
    /// <summary>
    /// The XML namespace for XML 1.0.
    /// </summary>
    public const string XmlNs = "http://www.w3.org/XML/1998/namespace";

    /// <summary>
    /// Creates the XML reader.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>XmlReader.</returns>
    public static XmlReader CreateXmlReader(string value)
    {
        return XmlReader.Create(
            new StringReader(value),
            new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreComments = true,
                IgnoreWhitespace = true
            });
    }

    /// <summary>
    /// Creates the XML writer.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <param name="attributes">The attributes.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns>XmlWriter.</returns>
    public static XmlWriter CreateXmlWriter(XmlWriterSettings settings, IEnumerable<ISyndicationAttribute>? attributes, StringBuilder buffer)
    {
        settings.Async = false;
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;

        var writer = XmlWriter.Create(buffer, settings);

        // Apply attributes
        if (attributes is not null && attributes.Any())
        {
            // Create element wrapper
            ISyndicationAttribute? xmlns = attributes.FirstOrDefault(a => a.Name == "xmlns");

            if (xmlns is null)
            {
                writer.WriteStartElement("w");
            }
            else
            {
                writer.WriteStartElement("w", xmlns.Value);
            }

            // Write attributes
            foreach (ISyndicationAttribute a in attributes)
            {
                if (a != xmlns)
                {
                    writer.WriteSyndicationAttribute(a);
                }
            }

            writer.WriteStartElement("y");
            writer.WriteEndElement();

            writer.Flush();
            _ = buffer.Clear();
        }

        return writer;
    }

    /// <summary>
    /// Flushes the asynchronous.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <returns>Task.</returns>
    public static Task FlushAsync(XmlWriter writer) => writer.Settings?.Async ?? false ? writer.FlushAsync() : Task.Run(writer.Flush);

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="xmlNode">The XML node.</param>
    /// <returns>System.String.</returns>
    public static string GetValue(string xmlNode)
    {
        using var reader = XmlReader.Create(new StringReader(xmlNode));
        _ = reader.MoveToContent();
        return reader.ReadElementContentAsString();
    }

    /// <summary>
    /// Determines whether [is XHTML media type] [the specified value].
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if [is XHTML media type] [the specified value]; otherwise, <c>false</c>.</returns>
    public static bool IsXhtmlMediaType(string? value) => value == "xhtml";

    /// <summary>
    /// Determines whether [is XML media type] [the specified value].
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if [is XML media type] [the specified value]; otherwise, <c>false</c>.</returns>
    public static bool IsXmlMediaType(string? value) => value is not null && (value == "xml" || value.EndsWith("/xml") || value.EndsWith("+xml"));

    /// <summary>
    /// Determines whether the specified name is XMLNS.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="ns">The namespace.</param>
    /// <returns><c>true</c> if the specified name is XMLNS; otherwise, <c>false</c>.</returns>
    public static bool IsXmlns(string name, string ns) => name == "xmlns" || ns == "http://www.w3.org/2000/xmlns/";

    /// <summary>
    /// Determines whether [is XML schema type] [the specified name].
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="ns">The namespace.</param>
    /// <returns><c>true</c> if [is XML schema type] [the specified name]; otherwise, <c>false</c>.</returns>
    public static bool IsXmlSchemaType(string name, string ns) => name == "type" && ns == "http://www.w3.org/2001/XMLSchema-instance";

    /// <summary>
    /// Needs the XML escape.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if the value needs XML escape, <c>false</c> otherwise.</returns>
    public static bool NeedXmlEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (int i = 0; i < value.Length; ++i)
        {
            char ch = value[i];

            if (ch == '<' || ch == '>' || ch == '&' || char.IsSurrogate(ch))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Reads the next node from the stream asynchronously.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>Task&lt;System.Boolean&gt;.</returns>
    public static Task<bool> ReadAsync(XmlReader reader) => reader.Settings?.Async ?? false ? reader.ReadAsync() : Task.Run(reader.Read);

    /// <summary>
    /// Reads the outer XML asynchronously.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>Task&lt;System.String&gt;.</returns>
    public static Task<string> ReadOuterXmlAsync(XmlReader reader) => reader.Settings?.Async ?? false ? reader.ReadOuterXmlAsync() : Task.Run(reader.ReadOuterXml);

    /// <summary>
    /// Skips the children of the current node.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>Task.</returns>
    public static Task SkipAsync(XmlReader reader) => reader.Settings?.Async ?? false ? reader.SkipAsync() : Task.Run(reader.Skip);

    /// <summary>
    /// Splits the name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="prefix">The prefix.</param>
    /// <param name="localName">Name of the local.</param>
    public static void SplitName(string name, out string? prefix, out string localName)
    {
        int i = name.IndexOf(':');
        if (i > 0)
        {
            prefix = name[..i];
            localName = name[(i + 1)..];
        }
        else
        {
            prefix = null;
            localName = name;
        }
    }

    /// <summary>
    /// Writes the raw XML content asynchronously.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="content">The content.</param>
    /// <returns>Task.</returns>
    public static Task WriteRawAsync(XmlWriter writer, string content) => writer.Settings?.Async ?? false ? writer.WriteRawAsync(content) : Task.Run(() => writer.WriteRaw(content));
}