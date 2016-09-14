using Friendica_Mobile.HttpRequests;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Friendica_Mobile
{
    public class clsHtmlToRichTextBlock
    {
        HtmlDocument htmlDoc;
        Dictionary<string, IHtmlType> ruleDic = new Dictionary<string, IHtmlType>();

        public clsHtmlToRichTextBlock(string html)
        {
            Init(html);
        }

        private void Init(string html)
        {
            //Initialize HtmlAgilityPack
            htmlDoc = new HtmlDocument();
            htmlDoc.OptionCheckSyntax = true;
            //htmlDoc.OptionFixNestedTags = false;
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.OptionAutoCloseOnEnd = true;
            htmlDoc.LoadHtml(html);

            //add tag type
            AppendHtmlType(new HToXaml("h1"));
            AppendHtmlType(new HToXaml("h2"));
            AppendHtmlType(new HToXaml("h3"));
            AppendHtmlType(new HToXaml("h4"));
            AppendHtmlType(new HToXaml("h5"));
            AppendHtmlType(new HToXaml("h6"));
            AppendHtmlType(new HToXaml("h7"));
            AppendHtmlType(new BrToXaml());
            AppendHtmlType(new IToXaml("em"));
            AppendHtmlType(new UToXaml("u"));
            AppendHtmlType(new StrongToXaml("b"));
            AppendHtmlType(new StrongToXaml("strong"));
            AppendHtmlType(new StrikeToXaml("strike"));

            //add special type
            AppendHtmlType(new AToXaml());
            AppendHtmlType(new ImageToXaml());
            AppendHtmlType(new BlockQuoteToXaml());
            AppendHtmlType(new CodeToXaml("code"));
            AppendHtmlType(new CodeToXaml("key"));
            AppendHtmlType(new UlToXaml());

            //not suported html tags (list elements and tables)
            //AppendHtmlType(new OLToXaml());
            //AppendHtmlType(new DLToXaml());
            //AppendHtmlType(new TableToXaml());

            //add default type
            AppendHtmlType(new DefaultToXaml());
        }


        public void ApplyHtmlToRichTextBlock(RichTextBlock rtb)
        {
            Paragraph block = new Paragraph();
            foreach (var node in htmlDoc.DocumentNode.ChildNodes)
                ConvertHtmlNode(node, block);
            rtb.Blocks.Add(block);
        }


        public Paragraph ApplyHtmlToParagraph()
        {
            Paragraph block = new Paragraph();
            foreach (var node in htmlDoc.DocumentNode.ChildNodes)
                ConvertHtmlNode(node, block);
            return block;
        }


        public void AppendHtmlType(IHtmlType hType)
        {
            if (ruleDic.ContainsKey(hType.TagName))
                ruleDic[hType.TagName] = hType;
            else
                ruleDic.Add(hType.TagName, hType);
        }


        private void ConvertHtmlNode(HtmlNode htmlNode, Block block)
        {
            string htmlNodeName = htmlNode.Name.ToLower();

            if (new[] { "p", "div" }.Contains(htmlNodeName))
            {
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    if (string.IsNullOrEmpty(htmlNode.InnerHtml))
                        continue;
                    ConvertHtmlNode(childHtmlNode, block);
                }
                var br = new LineBreak();
                (block as Paragraph).Inlines.Add(br);
            }

            if (new[] { "span" }.Contains(htmlNodeName))
            {
                // zeige keine Linien wenn nur 1 child vorhanden ist
                if (htmlNode.ChildNodes.Count <= 1)
                {
                    foreach (var childHtmlNode in htmlNode.ChildNodes)
                    {
                        if (string.IsNullOrEmpty(htmlNode.InnerHtml))
                            continue;
                        ConvertHtmlNode(childHtmlNode, block);
                    }
                }
                else
                {
                    var line = new Run();
                    line.Text = "______________________________";
                    (block as Paragraph).Inlines.Add(line);
                    var br = new LineBreak();
                    (block as Paragraph).Inlines.Add(br);
                    foreach (var childHtmlNode in htmlNode.ChildNodes)
                    {
                        if (string.IsNullOrEmpty(htmlNode.InnerHtml))
                            continue;
                        ConvertHtmlNode(childHtmlNode, block);
                    }
                    var br2 = new LineBreak();
                    (block as Paragraph).Inlines.Add(br2);
                    var line2 = new Run();
                    line2.Text = "______________________________";
                    (block as Paragraph).Inlines.Add(line2);
                }
            }

            if (ruleDic.ContainsKey(htmlNodeName))
                ruleDic[htmlNodeName].ApplyType(htmlNode, block);
        }

    }


    public interface IHtmlType
    {
        string TagName { get; }
        void ApplyType(HtmlNode htmlNode, Block block);
    }


    public abstract class HtmlTypeBase : IHtmlType
    {
        internal string GetCleanContent(string content)
        {
            // German special characters will be shown as &auml; etc. if not been DeEntitized
            return HtmlEntity.DeEntitize(content);
        }

        internal void TextToRun(string content, RichTextBlockStyle style, Block block)
        {
            string cContent = GetCleanContent(content);
            if (string.IsNullOrEmpty(cContent))
                return;

            // replace ASCII emoticons with icons from Segoe UI Symbol
            cContent = ChangeIcons(cContent);

            // split text at known @-tags to insert a Hyperlink at that position to the user's profile page
// TODO: currently we are navigating to the external page, load profile of user in Friendica mobile with known UI
            var parts = SplitContentByUserlinks(cContent);

            foreach (string part in parts)
            {
// TODO: Setting berücksichtigen, ob User überhaupt etwas gespeichert haben möchte
                if (part.StartsWith("@link@"))
                {
                    string clearPart = part.Substring(6);
                    // query database to get the whole FriendicaUser element with all information
                    var returnUser = App.sqliteConnection.SelectSingleFromtblAllKnownUsers(clearPart);

                    if (returnUser != null)
                    {
                        Hyperlink h = new Hyperlink();
                        h.NavigateUri = new Uri(returnUser.UserUrl, UriKind.RelativeOrAbsolute);
                        Run r = new Run();
                        h.Inlines.Add(r);
                        r.Foreground = new SolidColorBrush(Colors.Black);
                        r.Text = "@" + returnUser.UserName;
                        (block as Paragraph).Inlines.Add(h);
                    }
                }
                else
                {
                    // display the content in a run
                    Run run = new Run();
                    RichTextBlockStyle.SetStyle(run, style);
                    run.Text = part;
                    (block as Paragraph).Inlines.Add(run);
                }       
            }
        }


        private string[] SplitContentByUserlinks(string content)
        {
            var userList = App.sqliteConnection.SelectAllFromtblAllKnownUsers();

            foreach (var user in userList)
            {
                // some users use @name, others @name@server, so we need to mask both ways for replacing with Hyperlinks
                content = content.Replace("@" + user.UserName, "<splitbyprofile>@link@" + user.UserUrl + "<splitbyprofile>");
                Uri uri = new Uri(user.UserUrl);
                var userLongName = user.UserScreenName + "@" + uri.Host;
                content = content.Replace("@" + userLongName, "<splitbyprofile>@link@" + user.UserUrl + "<splitbyprofile>");
            }

            string[] parts = Regex.Split(content, @"<splitbyprofile>");
            return parts;
        }


        internal void TextToRun(string content, RichTextBlockStyle style, Block block, bool isItalic, bool isBold, bool isUnderline)
        {
            string cContent = GetCleanContent(content);
            if (string.IsNullOrEmpty(cContent))
                return;

            // replace ASCII emoticons with icons from Segoe UI Symbol
            cContent = ChangeIcons(cContent);

            if (isBold)
                style.FontWeight = FontWeights.Bold;
            else
                style.FontWeight = FontWeights.Normal;

            if (isItalic && !isUnderline)
            {
                Italic italic = new Italic();
                Run run = new Run();
                RichTextBlockStyle.SetStyle(run, style);
                run.Text = cContent;
                italic.Inlines.Add(run);
                (block as Paragraph).Inlines.Add(italic);
            }
            else if (isUnderline && !isItalic)
            {
                Underline uline = new Underline();
                Run run = new Run();
                RichTextBlockStyle.SetStyle(run, style);
                run.Text = cContent;
                uline.Inlines.Add(run);
                (block as Paragraph).Inlines.Add(uline);
            }
            else if (isUnderline && isItalic)
            {
                Underline uline = new Underline();
                Italic italic = new Italic();
                Run run = new Run();
                RichTextBlockStyle.SetStyle(run, style);
                run.Text = cContent;
                italic.Inlines.Add(run);
                uline.Inlines.Add(italic);
                (block as Paragraph).Inlines.Add(uline);
            }
            else
            {
                Run run = new Run();
                RichTextBlockStyle.SetStyle(run, style);
                run.Text = cContent;
                (block as Paragraph).Inlines.Add(run);
            }
        }

        internal string ChangeIcons(string content)
        {
            // Caution: order of replacements can be important, i.e. the 2nd devil smiley needs to be replaced before the happy smiley :-)

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


        internal void Br(Block block)
        {
            var br = new LineBreak();
            (block as Paragraph).Inlines.Add(br);
        }


        #region IHtmlRule Members

        public abstract string TagName { get; }

        public abstract void ApplyType(HtmlNode htmlNode, Block block);

        #endregion
    } // end of public abstract class HtmlTypeBase : IHtmlType


    public class HToXaml : HtmlTypeBase
    {
        string tag;
        public HToXaml(string tag)
        {
            this.tag = tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            var s = RichTextBlockStyle.GetDefault(htmlNode);
            // grap central setting for displaying content in RichTextBlock from MainStyles.xaml
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("ms-appx:///Styles/MainStyles.xaml", UriKind.RelativeOrAbsolute);
            var baseFontSize = (double)myResourceDictionary["RunFontSizeHeader"];
// TODO: diese Resource auch für die zentrale Steuerung der Hintergrund-Farben verwenden

            s.FontSize = baseFontSize;
            s.FontWeight = FontWeights.Bold;

            TextToRun(htmlNode.InnerText, s, block);
            Br(block);
        }
    } // end of public class HToXaml : HtmlTypeBase


    public class BrToXaml : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "br"; }
        }

        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            Br(block);
        }
    }  // end of public class BrToXaml : HtmlTypeBase


    public class IToXaml : HtmlTypeBase
    {
        string tag;
        public IToXaml(string tag)
        {
            this.tag = tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            bool isBold = false;
            bool isUnderline = false;
            bool isItalic = false;

            var s = RichTextBlockStyle.GetDefault(htmlNode);

            // Prüfung ob Child-Element mit strong (fett) oder u (unterstrichen) vorhanden ist
            if (htmlNode.Name == "strong")
                isBold = true;
            else if (htmlNode.Name == "i" || htmlNode.Name == "em")
                isItalic = true;
            else if (htmlNode.Name == "u")
                isUnderline = true;

            if (htmlNode.HasChildNodes)
            {
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    if (childHtmlNode.Name == "strong")
                        isBold = true;
                    else if (childHtmlNode.Name == "i" || childHtmlNode.Name == "em")
                        isItalic = true;
                    else if (childHtmlNode.Name == "u")
                        isUnderline = true;

                    if (childHtmlNode.HasChildNodes)
                    {
                        foreach (var child2HtmlNode in childHtmlNode.ChildNodes)
                        {
                            if (child2HtmlNode.Name == "strong")
                                isBold = true;
                            else if (child2HtmlNode.Name == "i" || child2HtmlNode.Name == "em")
                                isItalic = true;
                            else if (child2HtmlNode.Name == "u")
                                isUnderline = true;
                        }
                    }
                }
            }

            TextToRun(htmlNode.InnerText, s, block, isItalic, isBold, isUnderline);
        }
    }  // end of public class IToXaml : HtmlTypeBase


    public class UToXaml : HtmlTypeBase
    {
        string tag;
        public UToXaml(string tag)
        {
            this.tag = tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            bool isBold = false;
            bool isUnderline = false;
            bool isItalic = false;

            var s = RichTextBlockStyle.GetDefault(htmlNode);

            // Prüfung ob Child-Element mit strong (fett) oder em (kursiv) vorhanden ist
            if (htmlNode.Name == "strong")
                isBold = true;
            else if (htmlNode.Name == "i" || htmlNode.Name == "em")
                isItalic = true;
            else if (htmlNode.Name == "u")
                isUnderline = true;

            if (htmlNode.HasChildNodes)
            {
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    if (childHtmlNode.Name == "strong")
                        isBold = true;
                    else if (childHtmlNode.Name == "i" || childHtmlNode.Name == "em")
                        isItalic = true;
                    else if (childHtmlNode.Name == "u")
                        isUnderline = true;

                    if (childHtmlNode.HasChildNodes)
                    {
                        foreach (var child2HtmlNode in childHtmlNode.ChildNodes)
                        {
                            if (child2HtmlNode.Name == "strong")
                                isBold = true;
                            else if (child2HtmlNode.Name == "i" || child2HtmlNode.Name == "em")
                                isItalic = true;
                            else if (child2HtmlNode.Name == "u")
                                isUnderline = true;
                        }
                    }
                }
            }

            TextToRun(htmlNode.InnerText, s, block, isItalic, isBold, isUnderline);
        }
    }  // end of public class UToXaml : HtmlTypeBase


    public class StrongToXaml : HtmlTypeBase
    {
        string tag;
        public StrongToXaml(string tag)
        {
            this.tag = tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            bool isBold = false;
            bool isUnderline = false;
            bool isItalic = false;

            var s = RichTextBlockStyle.GetDefault(htmlNode);

            // Prüfung ob Child-Element mit em (kursiv) oder u (unterstrichen) vorhanden ist
            if (htmlNode.Name == "strong")
                isBold = true;
            else if (htmlNode.Name == "i" || htmlNode.Name == "em")
                isItalic = true;
            else if (htmlNode.Name == "u")
                isUnderline = true;

            if (htmlNode.HasChildNodes)
            {
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    if (childHtmlNode.Name == "strong")
                        isBold = true;
                    else if (childHtmlNode.Name == "i" || childHtmlNode.Name == "em")
                        isItalic = true;
                    else if (childHtmlNode.Name == "u")
                        isUnderline = true;

                    if (childHtmlNode.HasChildNodes)
                    {
                        foreach (var child2HtmlNode in childHtmlNode.ChildNodes)
                        {
                            if (child2HtmlNode.Name == "strong")
                                isBold = true;
                            else if (child2HtmlNode.Name == "i" || child2HtmlNode.Name == "em")
                                isItalic = true;
                            else if (child2HtmlNode.Name == "u")
                                isUnderline = true;
                        }
                    }
                }
            }

            TextToRun(htmlNode.InnerText, s, block, isItalic, isBold, isUnderline);                
        }
    }  // end of public class StrongToXaml : HtmlTypeBase


    public class StrikeToXaml : HtmlTypeBase
    {
        string tag;
        public StrikeToXaml(string tag)
        {
            this.tag = tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            var s = RichTextBlockStyle.GetDefault(htmlNode);
            TextToRun(htmlNode.InnerText, s, block);
        }
    }  // end of public class StrikeToXaml : HtmlTypeBase


    public class AToXaml : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "a"; }
        }

        public override async void ApplyType(HtmlNode htmlNode, Block block)
        {
            var url = htmlNode.Attributes["href"];
            if (url == null)
                throw new Exception("請輸入href");

            var s = RichTextBlockStyle.GetDefault(htmlNode);

            var target = htmlNode.Attributes["target"];
            // someone wrote an entry with a text like "http://friendica..." - it was not intended to be a link to somewhere
            // but it throwed an exection which we now catch and write the text as plain text
            try
            {
                if (url.Value.Contains("youtube.com/"))
                {
                    // load data from youtube for displaying the preview data (we cannot play youtube directly in the app)
                    var getYoutube = new GetOEmbedYoutube();
                    var oembed = await getYoutube.GetOEmbedYoutubeByUrl(url.Value);

                    if (oembed != null)
                    {
                        // container for displaying the data
                        InlineUIContainer container = new InlineUIContainer();

                        var grid = new Grid();
                        grid.HorizontalAlignment = HorizontalAlignment.Stretch;

                        // display progress ring until picture is loaded 
                        var progressRing = new ProgressRing();
                        progressRing.Visibility = Visibility.Visible;
                        progressRing.Foreground = new SolidColorBrush(Colors.Black);
                        progressRing.IsActive = true;

                        // display preview picture
                        var imageBitmap = new BitmapImage(new Uri(oembed.ThumbnailUrl));
                        var image = new Image();
                        image.ImageOpened += Image_ImageOpened;
                        image.Tapped += Image_Tapped;
                        image.Source = imageBitmap;
                        image.Width = 240;
                        image.Height = 180;
                        image.Tag = url.Value;

                       // display play button
                        var imageBitmapPlay = new BitmapImage(new Uri("ms-appx:///Assets/play.png"));
                        var imagePlay = new Image();
                        imagePlay.Source = imageBitmapPlay;
                        imagePlay.Width = 40;
                        imagePlay.Height = 48;
                        imagePlay.Tag = url.Value;
                        imagePlay.Tapped += Image_Tapped;

                        // add progress ring, image and play button to grid before adding grid to UIcontainer
                        grid.Children.Add(progressRing);
                        grid.Children.Add(image);
                        grid.Children.Add(imagePlay);
                        Grid.SetRow(progressRing, 0);
                        Grid.SetRow(image, 1);
                        Grid.SetRow(imagePlay, 1);
                        container.Child = grid;

                        // add grid to the UI
                        (block as Paragraph).Inlines.Add(container);
                        Br(block);
                        Br(block);
                        TextToRun(oembed.ProviderName + ": ", s, block, false, true, false);

                        Hyperlink h2 = new Hyperlink();
                        h2.NavigateUri = new Uri(url.Value, UriKind.RelativeOrAbsolute);
                        Run r2 = new Run();
                        h2.Inlines.Add(r2);
                        r2.Foreground = new SolidColorBrush(Colors.Black);
                        r2.Text = GetCleanContent(oembed.Title);
                        (block as Paragraph).Inlines.Add(h2);

                        TextToRun(" (" + oembed.AuthorName + ")", s, block, false, false, false);
                        Br(block);
                    }
                    if (htmlNode.NextSibling != null)
                        Br(block);
                }

                else if (url.Value.EndsWith(".mpg") || url.Value.EndsWith(".mp4") || url.Value.EndsWith(".mp3"))
                {
                    // local embedding of the video
// TODO: some bugs (full screen does not collapse the appbars, fullscreen in MobileLandscape
//              needs double tap to go up, button for full screen disappears on smaller devices,
//              larger files are not streamed (depends on server? - loading takes a lot of time)                    
                    MediaElement media = new MediaElement();
                    media.Source = new Uri(url.Value, UriKind.RelativeOrAbsolute);
                    media.Width = 240;
                    if (url.Value.EndsWith(".mp3"))
                        media.Height = 48;
                    else
                        media.Height = 180;
                    media.AutoPlay = false;
                    media.TransportControls.IsCompact = true;
                    media.TransportControls.IsZoomButtonVisible = false;
                    media.TransportControls.IsSeekBarVisible = false;
                    media.TransportControls.IsPlaybackRateButtonVisible = false;
                    media.TransportControls.IsSeekBarVisible = true;
                    if (url.Value.EndsWith(".mp3"))
                        media.TransportControls.IsFullWindowButtonVisible = false;
                    else
                        media.TransportControls.IsFullWindowButtonVisible = true;
                    media.AreTransportControlsEnabled = true;

                    InlineUIContainer container = new InlineUIContainer();
                    container.Child = media;
                    (block as Paragraph).Inlines.Add(container);

                    // add link to file as informative text (no oembed title available) with a blank line distance
                    Br(block);
                    Br(block);

                    // link also enables to play the video from edge browser if mediaelement does not work, with mp3 no link necessary
                    if (url.Value.EndsWith(".mp3"))
                    {
                        Run r2 = new Run();
                        r2.Text = GetCleanContent(url.Value);
                        r2.Text = System.Net.WebUtility.UrlDecode(r2.Text);
                        (block as Paragraph).Inlines.Add(r2);
                        Br(block);
                    }
                    else
                    {
                        Hyperlink h2 = new Hyperlink();
                        h2.NavigateUri = new Uri(url.Value, UriKind.RelativeOrAbsolute);
                        Run r2 = new Run();
                        h2.Inlines.Add(r2);
                        r2.Foreground = new SolidColorBrush(Colors.Black);
                        r2.Text = GetCleanContent(url.Value);
                        (block as Paragraph).Inlines.Add(h2);
                        Br(block);
                    }
                    if (htmlNode.NextSibling != null)
                        Br(block);
                }
                else
                {
                    // normal Hyperlinks without youtube or video or audio tags
                    Hyperlink h = new Hyperlink();
                    h.NavigateUri = new Uri(url.Value, UriKind.RelativeOrAbsolute);
                    Run r = new Run();
                    h.Inlines.Add(r);
                    r.Foreground = new SolidColorBrush(Colors.Black);
                    r.Text = GetCleanContent(htmlNode.InnerText);


                    // Prüfung ob Child-Element mit img vorhanden ist
                    if (htmlNode.HasChildNodes)
                    {
                        foreach (var childHtmlNode in htmlNode.ChildNodes)
                        {
                            if (childHtmlNode.Name == "img")
                            {
                                ImageToXaml ImageInA = new ImageToXaml();
                                ImageInA.ApplyType(childHtmlNode, block);
                            }
                        }
                    }
                    (block as Paragraph).Inlines.Add(h);
                }
            }
            catch (Exception)
            {
                s = RichTextBlockStyle.GetDefault(htmlNode);
                TextToRun(htmlNode.InnerText, s, block);
            }
        }


        private async void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var image = sender as Image;
            // remove eventhandler during launch otherwise we open 2 tabs with the link
            image.Tapped -= Image_Tapped;
            await Windows.System.Launcher.LaunchUriAsync(new Uri(image.Tag.ToString()));
            image.Tapped += Image_Tapped;
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            // if user navigates away before Image was loaded, system crashes as Parent would then be null
            if (image.Parent != null)
            {
                var grid = image.Parent as Grid;
                var progressRing = grid.Children[0];
                progressRing.Visibility = Visibility.Collapsed;
            }
        }
    }  // end of public class AToXaml : HtmlTypeBase


    public class ImageToXaml : HtmlTypeBase
    {
        public string imgSrc;
        public override string TagName
        {
            get { return "img"; }
        }

        public override async void ApplyType(HtmlNode htmlNode, Block block)
        {
            var src = htmlNode.Attributes["src"];
            if (src == null)
                throw new Exception("請輸入src");

            InlineUIContainer ilContainer = new InlineUIContainer();
            var alt = htmlNode.Attributes["alt"];

            Image img = new Image();

            // Bei fremden Timeline-Einträgen gibt es keinen http-Link sondern einen base64-Datenstream, hier muss der Datenstream
            // verarbeitet werden. Bei einem http-Link wird die Source des Image im RichTextBox-Control auf den Link gesetzt
            if (src.Value.Contains("data:image/jpeg;base64") || src.Value.Contains("data:image/png;base64"))
            {
                // Lade base64-Stream in Variable bytes und erzeuge daraus einen MemoryStream
                src.Value = src.Value.Replace("data:image/jpeg;base64,", "");
                src.Value = src.Value.Replace("data:image/png;base64,", "");
                byte[] bytes = Convert.FromBase64String(src.Value);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    // Übertrage base64-Stream im ms in ein BitmapImage übergebe dies an das Image im RichTextBox-Control
                    BitmapImage bitmapImage = new BitmapImage();
                    IRandomAccessStream a1 = await ConvertToRandomAccessStream(ms);
                    bitmapImage.SetSource(a1);
                    ImageSource imgSource = bitmapImage;
                    img.Source = imgSource;
                }
            }
            else
            {
                // images from remote server may be used without reading base64 data or loading through api
                ImageSource imgSource = new BitmapImage(new Uri(src.Value, UriKind.RelativeOrAbsolute));
                img.Source = imgSource;
                if (img.Height > img.Width)
                    img.MaxHeight = Window.Current.Bounds.Height / 2;
                else
                    img.MaxHeight = Window.Current.Bounds.Height / 3;
                img.Margin = new Thickness(4, 4, 4, 4);
            }
            // Klick auf das Image führt zu einer Fullscreen-Anzeige
            img.Tapped += Image_Tapped;

            img.Stretch = Stretch.Uniform;
            if (alt != null && !string.IsNullOrEmpty(alt.Value))
                ToolTipService.SetToolTip(img, alt);
            ilContainer.Child = img;
            (block as Paragraph).Inlines.Add(ilContainer);
        }

        private void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // grap central setting for displaying content in RichTextBlock from MainStyles.xaml
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("ms-appx:///Styles/MainStyles.xaml", UriKind.RelativeOrAbsolute);
            var flyoutStyle = (Style)myResourceDictionary["FlyoutPresenterImageFullscreen"];

            // load full data from tapping sender
            var imgSender = sender as Image;
            Image img = new Image();
            img.Stretch = Stretch.Uniform;
            img.Source = imgSender.Source;
            
            ScrollViewer scrlViewer = new ScrollViewer();
            scrlViewer.ZoomMode = ZoomMode.Enabled;
            scrlViewer.MinZoomFactor = (float)0.7;
            scrlViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrlViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrlViewer.Margin = new Thickness(-8, -8, 0, 0);
            scrlViewer.Height = Window.Current.Bounds.Height;
            scrlViewer.Width = Window.Current.Bounds.Width;
            
            Canvas canvas = new Canvas();

            // Fonticon für Schließen 
            FontIcon fIcon = new FontIcon();
            fIcon.Glyph = System.Net.WebUtility.HtmlDecode("&#x1F5D9;"); ;
            fIcon.FontFamily = new FontFamily("Segoe UI Emoji");
            fIcon.FontSize = 16;

            Button buttonClose = new Button();
            buttonClose.Tapped += ButtonClose_Tapped;
            buttonClose.Content = fIcon;

            scrlViewer.Content = img;
            canvas.Children.Add(scrlViewer);
            canvas.Children.Add(buttonClose);

            // display border as flyout
            Flyout flyout = new Flyout();
            flyout.Content = canvas;
            buttonClose.Tag = flyout;
            flyout.FlyoutPresenterStyle = flyoutStyle;
            flyout.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Full;
            flyout.ShowAt(imgSender);
        }


        private void ButtonClose_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var button = sender as Button;
            var flyout = button.Tag as Flyout;
            flyout.Hide();
        }


        public static async Task<IRandomAccessStream> ConvertToRandomAccessStream(MemoryStream memoryStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var dw = new DataWriter(outputStream);
            var task = Task.Factory.StartNew(() => dw.WriteBytes(memoryStream.ToArray()));
            await task;
            await dw.StoreAsync();
            await outputStream.FlushAsync();
            return randomAccessStream;
        }
    }  // end of public class ImageToXaml : HtmlTypeBase


    public class BlockQuoteToXaml : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "blockquote"; }
        }

        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            if (htmlNode.InnerText == "")
                return;

            // grap central setting for displaying content in RichTextBlock from MainStyles.xaml
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("ms-appx:///Styles/MainStyles.xaml", UriKind.RelativeOrAbsolute);
            var baseFontSize = (double)myResourceDictionary["RunFontSizeBlockquote"];

            InlineUIContainer ilContainer = new InlineUIContainer();

            Border border = new Border();
            border.Background = new SolidColorBrush(Colors.AntiqueWhite);
            border.Opacity = 0.5;
            border.Padding = new Thickness(8, 0, 8, 0);
            // Margin with max. 4 in order to display the quotes nicely in a line with normal text
            border.Margin = new Thickness(4, 4, 4, 0);
            // set bottom to align nicely in a line with normal text
            border.VerticalAlignment = VerticalAlignment.Bottom;
            // black border on the right side to sign that this is a quote
            border.BorderThickness = new Thickness(4, 0, 0, 0);
            border.BorderBrush = new SolidColorBrush(Colors.Black);

            TextBlock tb = new TextBlock();
            tb.FontSize = baseFontSize;
            var cContent = GetCleanContent(htmlNode.InnerText);
            // show emojis in quotes, too
            tb.Text = ChangeIcons(cContent);
            tb.Foreground = new SolidColorBrush(Colors.Black);
            tb.TextWrapping = TextWrapping.Wrap;
            tb.FontStyle = FontStyle.Italic;

            border.Child = tb;
            ilContainer.Child = border;
            (block as Paragraph).Inlines.Add(ilContainer);
        }
    }  // end of public class BlockQuoteToXaml : HtmlTypeBase


    public class CodeToXaml : HtmlTypeBase
    {
        //public override string TagName
        //{
        //    get { return "code"; }
        //}

        string tag;
        public CodeToXaml(string tag)
        {
            this.tag = tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            if (htmlNode.InnerText == "")
                return;

            InlineUIContainer ilContainer = new InlineUIContainer();
            // prepare content within border
            var border = PrepareCodebox(htmlNode, true);

            ilContainer.Child = border;
            (block as Paragraph).Inlines.Add(ilContainer);
        }


        private Border PrepareCodebox(HtmlNode htmlNode, bool shortenedView)
        {
            // grap central setting for displaying content in RichTextBlock from MainStyles.xaml
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("ms-appx:///Styles/MainStyles.xaml", UriKind.RelativeOrAbsolute);
            var codeFontSize = (double)myResourceDictionary["RunFontSizeCode"];
            var baseFontSize = (double)myResourceDictionary["RunFontSizeCodeFull"];

            Border border = new Border();
            border.Background = new SolidColorBrush(Colors.AntiqueWhite);
            border.Opacity = 0.7;
            border.Padding = new Thickness(8, 0, 8, 0);
            // Margin with max. 4 in order to display the quotes nicely in a line with normal text
            border.Margin = new Thickness(4, 4, 4, 0);
            // set bottom to align nicely in a line with normal text
            border.VerticalAlignment = VerticalAlignment.Bottom;
            // black border on the right side to sign that this is a quote
            border.BorderThickness = new Thickness(1, 1, 1, 1);
            border.BorderBrush = new SolidColorBrush(Colors.Black);

            TextBlock tb = new TextBlock();
            if (shortenedView)
                tb.FontSize = codeFontSize;
            else
                tb.FontSize = baseFontSize;
            tb.FontFamily = new FontFamily("Courier New");
            tb.Foreground = new SolidColorBrush(Colors.Black);
            tb.TextWrapping = TextWrapping.Wrap;

            // Prüfung ob Child-Element mit img vorhanden ist
            if (htmlNode.HasChildNodes)
            {
                int counter = 0;
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    var cleanContent = GetCleanContent(childHtmlNode.InnerText);
                    // shorten text in list of entries, only in flyout we will show all
                    if (shortenedView)
                    {
                        if (counter > 3)
                        {
                            tb.Text += Environment.NewLine + Environment.NewLine + "... Tippen für vollständigen Code ...";
                            tb.Tag = htmlNode;
                            tb.Tapped += Codebox_Tapped;
                            break;
                        }
                    }
                    if (childHtmlNode.Name == "#text")
                    {
                        // include line number before code text
                        counter = counter + 1;
                        if (htmlNode.ChildNodes.Count > 1)
                            tb.Text += "R" + string.Format("{0:00}", counter) + "| " + cleanContent;
                        else
                            tb.Text += cleanContent;
                    }
                    // if there is a link within a code block, only display the text
                    if (childHtmlNode.Name == "a")
                    {
                        tb.Text += cleanContent;
                    }
                    if (childHtmlNode.Name == "br")
                    {
                        // check if not null, as this has caused an Unhandled exception on one entry
                        if (childHtmlNode.PreviousSibling != null && childHtmlNode.PreviousSibling.Name == "br")
                        {
                            counter = counter + 1;
                            tb.Text += "R" + string.Format("{0:00}", counter) + "| " + Environment.NewLine;
                        }
                        else
                        {
                            tb.Text += Environment.NewLine;
                        }
                    }
                }
            }
            border.Child = tb;
            return border;
        }


        private void Codebox_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // grap central setting for displaying content in RichTextBlock from MainStyles.xaml
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("ms-appx:///Styles/MainStyles.xaml", UriKind.RelativeOrAbsolute);
            var flyoutStyle = (Style)myResourceDictionary["FlyoutPresenterCodeFullscreen"];

            // load full data from tapping sender
            var tbSender = sender as TextBlock;
            var htmlNode = tbSender.Tag as HtmlNode;

            // prepare content within border
            var border = PrepareCodebox(htmlNode, false);

            // display border as flyout
            Flyout flyout = new Flyout();
            flyout.Content = border;
            flyout.FlyoutPresenterStyle = flyoutStyle;
            flyout.ShowAt(tbSender);
        }

    }  // end of public class CodeToXaml : HtmlTypeBase

    
    public class UlToXaml : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "ul"; }
        }

        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            var s = RichTextBlockStyle.GetDefault(htmlNode);
            var styleType = htmlNode.Attributes[0].Value;

            if (styleType == "listdecimal")
            {
                var counter = 0;
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    counter += 1;
                    TextToRun("     " + counter + ".   " + childHtmlNode.InnerText, s, block);
                    Br(block);
                }
            }
            else if (styleType == "listbullet")
            {
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    TextToRun("     " + System.Net.WebUtility.HtmlDecode("&#x2B25;") + "   " + childHtmlNode.InnerText, s, block);
                    Br(block);
                }
            }
            else
            {
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    // currently use same as above for listbullet as default escape
                    TextToRun("     " + System.Net.WebUtility.HtmlDecode("&#x1F539;") + "   " + childHtmlNode.InnerText, s, block);
                    Br(block);
                }
            }
        }
    }  // end of public class UlToXaml : HtmlTypeBase


    public class DefaultToXaml : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "#text"; }
        }
        
        public override void ApplyType(HtmlNode htmlNode, Block block)
        {
            var s = RichTextBlockStyle.GetDefault(htmlNode);
            TextToRun(htmlNode.InnerText, s, block);
        }
    }  // end of public class DefaultToXaml : HtmlTypeBase


    public class RichTextBlockStyle
    {
        public string FontFamily { get; set; }
        public double? FontSize { get; set; }
        public FontStyle? FontStyle { get; set; }
        public Brush Foreground { get; set; }
        public FontWeight? FontWeight { get; set; }
        public bool Underlined;

        public static RichTextBlockStyle GetDefault(HtmlNode htmlNode)
        {
            
            Dictionary<string, string> cssStyle = GetCssStyle(htmlNode);
            RichTextBlockStyle rtStyle = new RichTextBlockStyle();

            try
            {
                if (cssStyle.ContainsKey("font-family"))
                    rtStyle.FontFamily = cssStyle["font-family"];
                if (cssStyle.ContainsKey("font-size"))
                {
                    string size = cssStyle["font-size"];
                    size = size.Replace("px", "").Replace("pt", "");
                    rtStyle.FontSize = double.Parse(size);
                }
                if (cssStyle.ContainsKey("font-style"))
                {
                    var v = cssStyle["font-style"].ToLower();
                }
                if (cssStyle.ContainsKey("font-weight"))
                {
                    var v = cssStyle["font-weight"].ToLower();
                    if (v == "bold")
                        rtStyle.FontWeight = FontWeights.Bold;
                    else
                        rtStyle.FontWeight = FontWeights.Normal;
                }
                if (cssStyle.ContainsKey("color"))
                {
                    var v = cssStyle["color"].ToLower();
                    // white is not differentiating from standard text, so change to silver
                    if (v == "white")
                        v = "silver";
                    rtStyle.Foreground = ConvertStringToColorBrush(v);
                }
            }
            catch
            {
                //throw new Exception(string.Concat("�]�wstyle���~:", ex.Message)); // verursacht Fehlermeldung
                // FormatException scheint keine Konsequenz für die Darstellung der Einträge zu haben, daher Meldung nur anzeigen, wenn andere Exception
                // scheinbar gibt es ein Problem bei Einträgen mit #-tags, führt zu leeren Format-Anweisungen, die eine "[Format_InvalidString]"
                // Fehlermeldung ausgeben, Text bleibt aber korrekt angezeigt (interessanterweise taucht Fehlermeldung nur auf 8.1 auf)
                // wird in der Folge hier auch unterdrückt
// TODO: Info an User bei Fehler wieder einbauen
//                if (ex.Message != "FormatException" && !ex.Message.StartsWith("[Format_InvalidString]"))
//                    MessageBox.Show(ex.Message);
            }

            return rtStyle;
        }

        private static Dictionary<string, string> GetCssStyle(HtmlNode htmlNode)
        {
            //parent
            Dictionary<string, string> cssStyleDic;
            if (htmlNode.Name.ToLower() == "#document")
                cssStyleDic = new Dictionary<string, string>();
            else
                cssStyleDic = GetCssStyle(htmlNode.ParentNode);

            //parseStyle
            var style = htmlNode.Attributes["style"];
            if (style == null)
                return cssStyleDic;

            var cssSetting = GetCssStyle(style.Value);
            foreach (var item in cssSetting)
            {
                if (cssStyleDic.ContainsKey(item.Key))
                    cssStyleDic[item.Key] = item.Value;
                else
                    cssStyleDic.Add(item.Key, item.Value);
            }

            return cssStyleDic;
        }

        public static Dictionary<string, string> GetCssStyle(string style)
        {
            style = style.Replace("&quot;", "");
            if (string.IsNullOrEmpty(style))
                return new Dictionary<string, string>();

            try
            {
                return style.Split(';').Where(a => a != "").Select(a => a.Trim())
                        .ToDictionary(a => a.Split(':')[0].Trim(), b => b.Split(':')[1].Trim());
            }
            catch
            {
                throw new Exception("style�]�w���~");
            }
        }

        public static void SetStyle(Inline inline, RichTextBlockStyle style)
        {
            if (!string.IsNullOrEmpty(style.FontFamily))
                inline.FontFamily = new FontFamily(style.FontFamily);

            if (style.FontSize.HasValue)
                inline.FontSize = style.FontSize.Value;

            if (style.Foreground != null)
                inline.Foreground = style.Foreground;

            if (style.FontWeight.HasValue)
                inline.FontWeight = style.FontWeight.Value;

            if (style.FontStyle.HasValue)
                inline.FontStyle = style.FontStyle.Value;
          }

        public static SolidColorBrush ConvertStringToColorBrush(string colorStr)
        {
            try
            {
                string s = string.Format(@"
<SolidColorBrush 
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
Color=""{0}"" />"
, colorStr);

                return (SolidColorBrush)XamlReader.Load(s);
            }
            catch
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }
    } // end of public class RichTextBlockStyle

}
