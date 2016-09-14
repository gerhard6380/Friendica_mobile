using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Documents;

namespace BackgroundTasks
{
    public sealed class clsHtmlToXml
    {
        HtmlDocument htmlDoc;
        string xmlString;
        List<string> formatEntities = new List<string>(new string[] { "em", "b", "u", "i", "strong", "#text", "span" });


        public clsHtmlToXml()
        {
        }


        public IAsyncOperation<string> TransformHtmlToXml(string html)
        {
            return this.TransformHtmlToXmlHelper(html).AsAsyncOperation();
        }


        private async Task<string> TransformHtmlToXmlHelper(string html)
        {
            xmlString = "";
            Init(html);
            await CreateXml();
            return xmlString;
        }


        private void Init(string html)
        {
            htmlDoc = new HtmlDocument();
            htmlDoc.OptionCheckSyntax = true;
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.OptionAutoCloseOnEnd = true;
            htmlDoc.LoadHtml(html);
        }


        private async Task CreateXml()
        {
            //xmlString = "";
            if (htmlDoc.DocumentNode.HasChildNodes)
            {
                string collectText = "";
                foreach (var child in htmlDoc.DocumentNode.ChildNodes)
                {
                    if (formatEntities.Contains(child.Name))
                    {
                        if (child.NextSibling != null && formatEntities.Contains(child.NextSibling.Name))
                        {
                            collectText += child.InnerText;
                            continue;
                        }
                        else if (child.Name == "span" && child.FirstChild != null && child.FirstChild.Name == "a")
                        {
                            xmlString += CreateTextElement("Link: " + child.FirstChild.Attributes["href"].Value);
                        }
                        else
                            collectText += child.InnerText;
                        xmlString += CreateTextElement(collectText);
                        collectText = "";
                    }
                    else if (child.Name == "br") { }
                    // code Block separat darstellen: 
                    else if (child.Name == "key")
                    {
                        if (child.NextSibling != null && formatEntities.Contains(child.NextSibling.Name))
                        {
                            collectText += child.InnerText;
                            continue;
                        }
                        else
                            collectText += child.InnerText;
                        xmlString += CreateTextElement("Code: " + collectText);
                        collectText = "";
                    }
                    // blockquote Block separat darstellen: 
                    else if (child.Name == "blockquote")
                    {
                        if (child.NextSibling != null && formatEntities.Contains(child.NextSibling.Name))
                        {
                            collectText += child.InnerText;
                            continue;
                        }
                        else
                            collectText += child.InnerText;
                        xmlString += CreateTextElement("Zitat: " + collectText);
                        collectText = "";
                    }
                    // list Block separat darstellen: 
                    else if (child.Name == "ul")
                    {
                        foreach (var childList in child.ChildNodes)
                        {
                            if (childList.Name == "li")
                                xmlString += CreateTextElement("* " + childList.InnerText);
                        }
                    }
                    else if (child.Name == "a" && child.HasChildNodes && child.FirstChild.Name == "img")
                    {
                        var source = child.FirstChild.Attributes["src"];
                        if (source.Value.Contains("data:image/jpeg;base64") || source.Value.Contains("data:image/png;base64"))
                        {
                            string fileSuffix = ".jpg";
                            if (source.Value.Contains("data:image/jpeg;base64"))
                                fileSuffix = ".jpg";
                            if (source.Value.Contains("data:image/png;base64"))
                                fileSuffix = ".png";
                            // Lade base64-Stream in Variable bytes und erzeuge daraus einen MemoryStream
                            source.Value = source.Value.Replace("data:image/jpeg;base64,", "");
                            source.Value = source.Value.Replace("data:image/png;base64,", "");
                            byte[] bytes = Convert.FromBase64String(source.Value);
                            string fileName = Path.GetRandomFileName() + fileSuffix;
                            string fileNamePath = "ms-appdata:///local/Toastimages/" + fileName;
                            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                            await localFolder.CreateFolderAsync("Toastimages", CreationCollisionOption.OpenIfExists);
                            var file = await localFolder.CreateFileAsync("Toastimages\\" + fileName, CreationCollisionOption.ReplaceExisting);
                            using (MemoryStream ms = new MemoryStream(bytes))
                                {
                                    using (FileStream fileStream = new FileStream(file.Path, FileMode.Create, FileAccess.Write))
                                    {
                                        // Write the data to the file, byte by byte.
                                        for (int i = 0; i < bytes.Length; i++)
                                        {
                                            fileStream.WriteByte(bytes[i]);
                                        }
                                    }
                                }
                            xmlString += CreateImageElement(fileNamePath);
                        }
                    }
                    // Links which are not nested in a span tag or having an image as child
                    else if (child.Name == "a" && child.FirstChild.Name == "#text")
                    {
                        if (child.Attributes["href"].Value.Contains("youtube.com/"))
                        {
                            var getYoutube = new GetOEmbedYoutube();
                            var oembed = await getYoutube.GetOEmbedYoutubeByUrl(child.Attributes["href"].Value);
                            if (oembed != null)
                            {
                                xmlString += CreateTextElement("Link - Youtube: " + oembed.Title.Replace("&", "&amp;"));
                                xmlString += CreateImageElement(oembed.ThumbnailUrl);
                            }
                        }
                        else
                            xmlString += CreateTextElement(child.InnerText);
                    }
                    // alternative for all other tags - display innertext
                    else
                        xmlString += CreateTextElement(child.InnerText);
                }
            }
        }


        private string CreateTextElement(string displayText)
        {
            var baseText = "<text>{0}</text>";
            return String.Format(baseText, ChangeIcons(displayText));
        }


        private string CreateImageElement(string imgSource)
        {
            var baseText = "<image placement =\"inline\" src=\"{0}\" />";
            return String.Format(baseText, imgSource);
        }


        internal string ChangeIcons(string content)
        {
            // Caution: order of replacements can be important, i.e. the 2nd devil smiley needs to be replaced before the happy smiley :-)
            //content = content.Replace("&uuml;", "ü");
            //content = content.Replace("&auml;", "ä");
            //content = content.Replace("&ouml;", "ö");
            //content = content.Replace("&Uuml;", "Ü");
            //content = content.Replace("&Auml;", "Ä");
            //content = content.Replace("&Ouml;", "Ö");
            //content = content.Replace("&szlig;", "ß");
            //content = content.Replace("rsquo;", "'");
            //content = content.Replace("&", "&amp;");
            content = System.Net.WebUtility.HtmlDecode(content);
            // symbol for like/dislike
            content = content.Replace(":like", System.Net.WebUtility.HtmlDecode("&#x1F44D;"));
            content = content.Replace(":dislike", System.Net.WebUtility.HtmlDecode("&#x1F44E;"));
            //:facepalm missing
            // symbol coffe
            content = content.Replace(":coffee", System.Net.WebUtility.HtmlDecode("&#x1F375;"));
            // symbol beer
            content = content.Replace(":beer", System.Net.WebUtility.HtmlDecode("&#x1F37A;"));
            content = content.Replace(":homebrew", System.Net.WebUtility.HtmlDecode("&#x1F37A;"));
            // smiley devil
            content = content.Replace(">:)", System.Net.WebUtility.HtmlDecode("&#x1F608;"));
            content = content.Replace("}:-)", System.Net.WebUtility.HtmlDecode("&#x1F608;"));
            // symbol heart
            content = content.Replace("<3", System.Net.WebUtility.HtmlDecode("&#x1F497;"));
            //symbol broken heart
            content = content.Replace("</3", System.Net.WebUtility.HtmlDecode("&#x1F494;"));
            content = content.Replace("<\\3", System.Net.WebUtility.HtmlDecode("&#x1F494;"));
            // smiley happy
            content = content.Replace(":-)", System.Net.WebUtility.HtmlDecode("&#x1F60A;"));
            content = content.Replace(":)", System.Net.WebUtility.HtmlDecode("&#x1F60A;"));
            // smiley sad
            content = content.Replace(":-(", System.Net.WebUtility.HtmlDecode("&#x1F621;"));
            content = content.Replace(":(", System.Net.WebUtility.HtmlDecode("&#x1F621;"));
            // smiley twinker
            content = content.Replace(";-)", System.Net.WebUtility.HtmlDecode("&#x1F609;"));
            // smiley with tongue
            content = content.Replace(":-P", System.Net.WebUtility.HtmlDecode("&#x1F61B;"));
            content = content.Replace(":-p", System.Net.WebUtility.HtmlDecode("&#x1F61B;"));
            // kiss in used for :-" and :-X/:-x in Friendica, however commonly used is :-* for kiss, and :-X means sealed lips
            // :-" and :-* lead to kiss symbol in App (while web browser doesn't display :-*), 
            // :-X and :-x displayed as sealed in App (while web browser displays kiss)
            // smiley kiss
            content = content.Replace(":-\"", System.Net.WebUtility.HtmlDecode("&#x1F618;"));
            content = content.Replace(":-*", System.Net.WebUtility.HtmlDecode("&#x1F618;"));
            // smiley sealed lips
            content = content.Replace(":-x", System.Net.WebUtility.HtmlDecode("&#x1F645;"));
            content = content.Replace(":-X", System.Net.WebUtility.HtmlDecode("&#x1F645;"));
            // smiley laughing out loud
            content = content.Replace(":-D", System.Net.WebUtility.HtmlDecode("&#x1F600;"));
            // smiley surprised
            content = content.Replace("8-|", System.Net.WebUtility.HtmlDecode("&#x1F632;"));
            content = content.Replace("8-O", System.Net.WebUtility.HtmlDecode("&#x1F632;"));
            content = content.Replace(":-O", System.Net.WebUtility.HtmlDecode("&#x1F632;"));
            // smiley dumbs up
            content = content.Replace("\\o/", System.Net.WebUtility.HtmlDecode("&#x1F64C;"));
            // smiley questioning
            content = content.Replace("O.o", System.Net.WebUtility.HtmlDecode("&#x1F615;"));
            content = content.Replace("o.O", System.Net.WebUtility.HtmlDecode("&#x1F615;"));
            content = content.Replace("O_o", System.Net.WebUtility.HtmlDecode("&#x1F615;"));
            content = content.Replace("o_O", System.Net.WebUtility.HtmlDecode("&#x1F615;"));
            // smiley cry
            content = content.Replace(":'(", System.Net.WebUtility.HtmlDecode("&#x1F62D;"));
            // Smiley Foot in mouth
            content = content.Replace(":-!", System.Net.WebUtility.HtmlDecode("&#x1F625;"));
            // smiley undecided
            content = content.Replace(":-/", System.Net.WebUtility.HtmlDecode("&#x1F629;"));
            // smiley embarrassed
            content = content.Replace(":-[", System.Net.WebUtility.HtmlDecode("&#x1F633;"));
            // smiley cool
            content = content.Replace("8-)", System.Net.WebUtility.HtmlDecode("&#x1F60E;"));
            // smiley spock
            content = content.Replace(">:-I", System.Net.WebUtility.HtmlDecode("&#x1F596;"));

            return content;
        }

    }  // END OF public sealed class clsHtmlToXml

}  // END OF namespace BackgroundTasks
