// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

/// <summary>
/// Create a SyndicationItem and add a custom field.
/// </summary>
internal class RssWriteItemWithCustomElement
{
    public static async Task WriteCustomItem()
    {
        const string ExampleNs = "http://contoso.com/syndication/feed/examples";
        StringWriterWithEncoding sw = new(Encoding.UTF8);

        using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true, Indent = true }))
        {
            List<SyndicationAttribute> attributes =
                [
                    new SyndicationAttribute("xmlns:example", ExampleNs)
                ];

            RssFormatter formatter = new(attributes, xmlWriter.Settings);
            RssFeedWriter writer = new(xmlWriter, attributes, formatter);

            // Create item
            SyndicationItem item = new()
            {
                Title = "Rss Writer Available",
                Description = "The new RSS Writer is now available as a NuGet package!",
                Id = "https://www.nuget.org/packages/Microsoft.SyndicationFeed",
                Published = DateTimeOffset.UtcNow
            };

            item.AddCategory(new SyndicationCategory("Technology"));
            item.AddContributor(new SyndicationPerson("test", "test@mail.com"));

            //
            // Format the item as SyndicationContent
            SyndicationContent content = new(formatter.CreateContent(item));

            // Add custom fields/attributes
            content.AddField(new SyndicationContent("customElement", ExampleNs, "Custom Value"));

            // Write
            await writer.Write(content);

            // Done
            xmlWriter.Flush();
        }

        Console.WriteLine(sw.ToString());
    }

    private class StringWriterWithEncoding(Encoding encoding) : StringWriter
    {
        public override Encoding Encoding => encoding;
    }
}
