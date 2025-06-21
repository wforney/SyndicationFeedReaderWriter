// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml;

namespace Microsoft.SyndicationFeed.Utils;

internal static class XmlExtentions
{
    /// <summary>
    /// Reads the syndication attribute.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>System.Nullable&lt;ISyndicationAttribute&gt;.</returns>
    /// <exception cref="System.InvalidOperationException">Invalid Xml Attribute</exception>
    public static ISyndicationAttribute? ReadSyndicationAttribute(this XmlReader reader)
    {
        if (reader.NodeType != XmlNodeType.Attribute)
        {
            throw new InvalidOperationException("Invalid Xml Attribute");
        }

        string ns = reader.NamespaceURI;
        string name = reader.LocalName;

        return XmlUtils.IsXmlns(name, ns) || XmlUtils.IsXmlSchemaType(name, ns) ? null : (ISyndicationAttribute)new SyndicationAttribute(name, ns, reader.Value);
    }

    /// <summary>
    /// Writes the start content of the syndication.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="content">The content.</param>
    /// <param name="defaultNs">The default ns.</param>
    public static void WriteStartSyndicationContent(this XmlWriter writer, ISyndicationContent content, string? defaultNs)
    {
        string? ns = content.Namespace ?? defaultNs;

        if (ns is not null)
        {
            XmlUtils.SplitName(content.Name, out string? prefix, out string localName);

            prefix = writer.LookupPrefix(ns) ?? prefix;

            if (prefix is not null)
            {
                writer.WriteStartElement(prefix, localName, ns);
            }
            else
            {
                writer.WriteStartElement(localName, ns);
            }
        }
        else
        {
            writer.WriteStartElement(content.Name);
        }
    }

    /// <summary>
    /// Writes the string.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="useCDATA">if set to <c>true</c> [use cdata].</param>
    public static void WriteString(this XmlWriter writer, string value, bool useCDATA)
    {
        if (useCDATA && XmlUtils.NeedXmlEscape(value))
        {
            writer.WriteCData(value);
        }
        else
        {
            writer.WriteString(value);
        }
    }

    /// <summary>
    /// Writes the syndication attribute.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="attr">The attribute.</param>
    public static void WriteSyndicationAttribute(this XmlWriter writer, ISyndicationAttribute attr)
    {
        XmlUtils.SplitName(attr.Name, out string? prefix, out string localName);

        writer.WriteAttribute(prefix, attr.Name, localName, attr.Namespace, attr.Value);
    }

    /// <summary>
    /// Writes the XML fragment.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="fragment">The fragment.</param>
    /// <param name="defaultNs">The default ns.</param>
    public static void WriteXmlFragment(this XmlWriter writer, string fragment, string defaultNs)
    {
        using XmlReader reader = XmlUtils.CreateXmlReader(fragment);
        _ = reader.MoveToContent();

        while (!reader.EOF)
        {
            string? ns = string.IsNullOrEmpty(reader.NamespaceURI) ? defaultNs : reader.NamespaceURI;

            // Start Element
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (ns is null)
                {
                    writer.WriteStartElement(reader.LocalName);
                }
                else
                {
                    writer.WriteStartElement(reader.LocalName, ns);
                }

                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (!XmlUtils.IsXmlns(reader.Name, reader.Value))
                        {
                            writer.WriteAttribute(reader.Prefix, reader.Name, reader.LocalName, ns, reader.Value);
                        }
                    }

                    _ = reader.MoveToContent();
                }

                if (reader.IsEmptyElement)
                {
                    writer.WriteEndElement();
                }

                _ = reader.Read();
                continue;
            }

            // End Element
            if (reader.NodeType == XmlNodeType.EndElement)
            {
                writer.WriteEndElement();
                _ = reader.Read();
                continue;
            }

            // Copy Content
            writer.WriteNode(reader, false);
        }
    }

    private static void WriteAttribute(this XmlWriter writer, string? prefix, string name, string localName, string? ns, string? value)
    {
        prefix ??= writer.LookupPrefix(ns ?? string.Empty);

        if (prefix == string.Empty)
        {
            writer.WriteStartAttribute(name);
        }
        else if (prefix is not null)
        {
            writer.WriteStartAttribute(prefix, localName, ns);
        }
        else
        {
            writer.WriteStartAttribute(localName, ns);
        }

        writer.WriteString(value);
        writer.WriteEndAttribute();
    }
}