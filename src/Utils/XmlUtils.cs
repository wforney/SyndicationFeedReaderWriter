// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Xml;

namespace Microsoft.SyndicationFeed.Utils;

internal static class XmlUtils
{
    public const string XmlNs = "http://www.w3.org/XML/1998/namespace";

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

    public static XmlWriter CreateXmlWriter(XmlWriterSettings settings, IEnumerable<ISyndicationAttribute> attributes, StringBuilder buffer)
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

    public static Task FlushAsync(XmlWriter writer) => writer.Settings?.Async ?? false ? writer.FlushAsync() : Task.Run(writer.Flush);

    public static string GetValue(string xmlNode)
    {
        using var reader = XmlReader.Create(new StringReader(xmlNode));
        _ = reader.MoveToContent();
        return reader.ReadElementContentAsString();
    }

    public static bool IsXhtmlMediaType(string? value) => value == "xhtml";

    public static bool IsXmlMediaType(string? value) => value is not null && (value == "xml" || value.EndsWith("/xml") || value.EndsWith("+xml"));

    public static bool IsXmlns(string name, string ns) => name == "xmlns" || ns == "http://www.w3.org/2000/xmlns/";

    public static bool IsXmlSchemaType(string name, string ns) => name == "type" && ns == "http://www.w3.org/2001/XMLSchema-instance";

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

    public static Task<bool> ReadAsync(XmlReader reader) => reader.Settings?.Async ?? false ? reader.ReadAsync() : Task.Run(reader.Read);

    public static Task<string> ReadOuterXmlAsync(XmlReader reader) => reader.Settings?.Async ?? false ? reader.ReadOuterXmlAsync() : Task.Run(reader.ReadOuterXml);

    public static Task SkipAsync(XmlReader reader) => reader.Settings?.Async ?? false ? reader.SkipAsync() : Task.Run(reader.Skip);

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

    public static Task WriteRawAsync(XmlWriter writer, string content) => writer.Settings?.Async ?? false ? writer.WriteRawAsync(content) : Task.Run(() => writer.WriteRaw(content));
}