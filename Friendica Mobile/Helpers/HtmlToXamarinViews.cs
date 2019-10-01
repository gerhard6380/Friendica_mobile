using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Strings;
using HtmlAgilityPack;
using SeeberXamarin.Controls;
using Xamarin.Forms;

namespace Friendica_Mobile
{
    public class ViewComponents
    {
        public List<View> ViewElements { get; set; }
        public FormattedString FormattedText { get; set; }
        public List<View> SharedContentElements { get; set; }


        public void CloseSpan()
        {
            if (FormattedText != null && FormattedText.Spans.Count > 0)
                AddElement(new Label() { FormattedText = FormattedText });
            FormattedText = new FormattedString();
        }

        public bool HasOpenSpans()
        {
            return (FormattedText != null && FormattedText.Spans.Count > 0);
        }

        public void AddElement(View element)
        {
            if (SharedContentElements != null)
                SharedContentElements.Add(element);
            else
                ViewElements.Add(element);
        }
    }

    public class SpanStyle
    {
        public bool IsBool;
        public bool IsItalic;
        public bool IsUnderline;
        public bool IsStrikeThrough;
        public string Color;

        public static void SetStyle(Span span, SpanStyle style)
        {
            if (style.IsBool)
                span.FontAttributes |= FontAttributes.Bold;
            if (style.IsItalic)
                span.FontAttributes |= FontAttributes.Italic;
            if (style.IsUnderline)
                span.TextDecorations |= TextDecorations.Underline;
            if (style.IsStrikeThrough)
                span.TextDecorations |= TextDecorations.Strikethrough;

            // set text color according to stylesheet color in html (white replaced by silver in CheckStyle as main text is white)
            try
            {
                if (!String.IsNullOrEmpty(style.Color))
                {
                    span.TextColor = (Color)new ColorTypeConverter().ConvertFromInvariantString(style.Color);
                }
            }
            catch { span.TextColor = Xamarin.Forms.Color.Silver; }
        }

        public static SpanStyle GetStyle(HtmlNode htmlNode, SpanStyle style = null)
        {
            if (style == null)
                style = new SpanStyle();
            CheckStyle(htmlNode, style);

            // check the next levels to catch multiple tags like <b><i><u>text</u></i></b>
            if (htmlNode.HasChildNodes)
            {
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    GetStyle(childHtmlNode, style);
                }
            }
            return style;
        }

        private static void CheckStyle(HtmlNode node, SpanStyle style)
        {
            if (node.Name == "strong" || node.Name == "b")
                style.IsBool = true;
            if (node.Name == "i" || node.Name == "em")
                style.IsItalic = true;
            if (node.Name == "u")
                style.IsUnderline = true;
            if (node.Name == "strike")
                style.IsStrikeThrough = true;

            Dictionary<string, string> cssStyle = GetCssStyle(node);
            if (cssStyle.ContainsKey("color"))
            {
                var v = cssStyle["color"];
                // white is not differentiating from standard text, so change to silver
                if (v.ToLower() == "white")
                    v = "Silver";
                style.Color = v;
            }
        }

        private static Dictionary<string, string> GetCssStyle(HtmlNode htmlNode)
        {
            Dictionary<string, string> cssStyleDic = new Dictionary<string, string>();
            HtmlAttribute style;
            if (htmlNode.Name == "#text" && htmlNode.ParentNode.Name == "span")
                style = htmlNode.ParentNode.Attributes["style"];
            else
                style = htmlNode.Attributes["style"];
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

        private static Dictionary<string, string> GetCssStyle(string style)
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
                throw new Exception("style");
            }
        }

    }

    public class HtmlToXamarinViews
    {
        // contains the html document
        HtmlDocument htmlDoc;
        // Dictionary with the tags to be converted from HTML to Xamarin Forms View elements
        Dictionary<string, IHtmlType> ruleDic = new Dictionary<string, IHtmlType>();
        // list of all views to be returned to StackLayout in template "TemplateFriendicaPosts"
        ViewComponents Components;


        public HtmlToXamarinViews(string html)
        {
            Components = new ViewComponents();
            Init(html);
        }


        private void Init(string html)
        {
            // initialize HtmlAgilityPack
            htmlDoc = new HtmlDocument();
            htmlDoc.OptionCheckSyntax = true;
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.OptionAutoCloseOnEnd = true;
            // within pre/code blocks we need <br> instead of \r\n as otherwise CodeToView interpretes all as one line and needs more space than intended
            var htmlAdapted = html.Replace("\r\n", "<br>");
            htmlDoc.LoadHtml(htmlAdapted);

            // add tag types which should be converted into Xamarin Forms View elements
            AppendHtmlType(new HToView("h1"));
            AppendHtmlType(new HToView("h2"));
            AppendHtmlType(new HToView("h3"));
            AppendHtmlType(new HToView("h4"));
            AppendHtmlType(new HToView("h5"));
            AppendHtmlType(new HToView("h6"));
            AppendHtmlType(new HToView("h7"));
            AppendHtmlType(new DefaultToView("b"));
            AppendHtmlType(new DefaultToView("strong"));
            AppendHtmlType(new DefaultToView("i"));
            AppendHtmlType(new DefaultToView("em"));
            AppendHtmlType(new DefaultToView("u"));
            AppendHtmlType(new DefaultToView("strike"));

            // special handling of several items
            AppendHtmlType(new BrToView());
            AppendHtmlType(new HrToView());
            AppendHtmlType(new AToView());
            AppendHtmlType(new ImageToView());
            AppendHtmlType(new ListToView("ul"));
            AppendHtmlType(new ListToView("ol"));
            AppendHtmlType(new BlockquoteToView());
            AppendHtmlType(new CodeToView("code"));
            AppendHtmlType(new CodeToView("key"));
            AppendHtmlType(new CodeToView("pre"));

            // add default type
            AppendHtmlType(new DefaultToView());
        }


        // build dictionary with all html types to be converted
        public void AppendHtmlType(IHtmlType hType)
        {
            if (ruleDic.ContainsKey(hType.TagName))
                ruleDic[hType.TagName] = hType;
            else
                ruleDic.Add(hType.TagName, hType);
        }

        /// <summary>
        /// return text from html 
        /// </summary>
        /// <returns></returns>
        public string ConvertHtmlToPlainText()
        {
            return htmlDoc.DocumentNode.InnerText; 
        }

        // convert html to view elements and return list of View elements
        public List<View> ApplyHtmlToXamarinViews()
        {
            Components.ViewElements = new List<View>();
            
            // work through all html tags to convert into Xamarin View content
            foreach (var node in htmlDoc.DocumentNode.ChildNodes)
                ConvertHtmlNode(node, Components);

            // add open spans to text if still content is in FormattedText collector
            Components.CloseSpan();

            return Components.ViewElements;
        }


        // working on the html node
        public void ConvertHtmlNode(HtmlNode htmlNode, ViewComponents components)
        {
            string htmlNodeName = htmlNode.Name.ToLower();
            // when being within a paragraph or div-section we will work on each child element
            if (new[] { "p", "div" }.Contains(htmlNodeName))
            {
                if (htmlNode.GetAttributeValue("class", "") == "shared-wrapper")
                {
                    components.CloseSpan();
                    // shared content to be separated from post content by a gray border
                    PrepareSharedContent(htmlNode);
                }
                else
                {
                    // go through the childs of the p or div html tag
                    foreach (var childHtmlNode in htmlNode.ChildNodes)
                    {
                        // don't do anything if there is no content in the node
                        if (string.IsNullOrEmpty(htmlNode.InnerHtml))
                            continue;
                        ConvertHtmlNode(childHtmlNode, components);
                    }
                    // paragraph is finished, add to view elements
                    components.CloseSpan();
                }
            }
            if (new[] { "span" }.Contains(htmlNodeName))
            {
                if (htmlNode.GetAttributeValue("class", "") == "shared-wrapper")
                {
                    // shared content to be separated from post content by a gray border
                    PrepareSharedContent(htmlNode);
                }
                else
                {
                    // go through the childs of the span html tag
                    foreach (var childHtmlNode in htmlNode.ChildNodes)
                    {
                        // don't do anything if there is no content in the node
                        if (string.IsNullOrEmpty(htmlNode.InnerHtml))
                            continue;
                        ConvertHtmlNode(childHtmlNode, components);
                        // don't finalize the collection of FormattedText spans as it could continue after span end tag
                    }
                }
            }

            if (ruleDic.ContainsKey(htmlNodeName))
                ruleDic[htmlNodeName].ApplyType(htmlNode, components);
        }


        // working on the shared content
        private void PrepareSharedContent(HtmlNode htmlNode)
        {
            var frame = new Frame()
            {
                BorderColor = Color.AntiqueWhite,
                BackgroundColor = Color.Transparent,
                Padding = 2,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.StartAndExpand
            };
            var stack = new StackLayout() { Spacing = 10, VerticalOptions = LayoutOptions.Start };
            frame.Content = stack;

            var grid = new Grid() { VerticalOptions = LayoutOptions.Start };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            stack.Children.Add(grid);

            foreach (var childHtmlNode in htmlNode.ChildNodes)
            {
                if (string.IsNullOrEmpty(childHtmlNode.InnerHtml))
                    continue;

                if (childHtmlNode.GetAttributeValue("class", "") == "shared_header")
                {
                    // display information from header (e.g. data from orginal sender)
                    foreach (var childHtmlNode2 in childHtmlNode.ChildNodes)
                    {
                        // add user image
                        if (childHtmlNode2.GetAttributeValue("class", "") == "shared-userinfo")
                        {
                            var elements = new ViewComponents();
                            elements.ViewElements = new List<View>();
                            ConvertHtmlNode(childHtmlNode2, elements);
                            try
                            {
                                // now take the profile image and set parameters for displaying
                                var imageGrid = elements.ViewElements[0] as Grid;
                                var image = imageGrid.Children[1];
                                image.Margin = new Thickness(4, 4, 0, 0);
                                image.WidthRequest = 48;
                                image.HeightRequest = 48;
                                grid.Children.Add(image);
                                Grid.SetColumn(image, 0);
                                Grid.SetRowSpan(image, 2);
                            } catch { }
                        }
                        // add username
                        else if (childHtmlNode2.GetAttributeValue("class", "") == "shared-wall-item-name"
                            || (childHtmlNode2.Name == "div" && childHtmlNode2.FirstChild.GetAttributeValue("class", "") == "shared-wall-item-name"))
                        {
                            var elements = new ViewComponents();
                            elements.ViewElements = new List<View>();
                            ConvertHtmlNode(childHtmlNode2, elements);
                            try
                            {
                                var name = elements.ViewElements[0];
                                grid.Children.Add(name);
                                Grid.SetColumn(name, 1);
                                Grid.SetRow(name, 0);
                            } catch { }
                        }
                        // add date, time
                        else if (childHtmlNode2.GetAttributeValue("class", "") == "shared-wall-item-ago")
                        {
                            var text = childHtmlNode2.InnerText;
                            var label = new Label { Text = text, Style = (Style)Application.Current.Resources["LabelHtmlSharedTime"] };
                            grid.Children.Add(label);
                            Grid.SetColumn(label, 1);
                            Grid.SetRow(label, 1);
                        }
                    }
                }
                else if (childHtmlNode.GetAttributeValue("class", "") == "shared_content")
                {
                    Components.SharedContentElements = new List<View>();
                    // prepare content
                    ConvertHtmlNode(childHtmlNode, Components);
                    Components.CloseSpan();
                    // now add the content
                    var stackContent = new StackLayout() { Spacing = 0, VerticalOptions = LayoutOptions.StartAndExpand };
                    foreach (var item in Components.SharedContentElements)
                    {
                        stackContent.Children.Add(item);
                    }
                    stack.Children.Add(stackContent);
                    // null out the sharedcontentelements
                    Components.SharedContentElements = null;
                }
            }
            // add the frame to the content
            Components.ViewElements.Add(frame);
        }
    }


    /// <summary>
    /// Interface IHtmlType
    /// implemented by different classes for each html tag type to generate Xamarin Forms View content
    /// </summary>
    public interface IHtmlType
    {
        string TagName { get; }
        void ApplyType(HtmlNode htmlNode, ViewComponents components);
    }


    /// <summary>
    /// abstract class based on Interface used to subclass for concrete html types
    /// base class contains functions to be used in the subclasses
    /// </summary>
    public abstract class HtmlTypeBase : IHtmlType
    {
        // pass through interface members to subclasses
        public abstract string TagName { get; }
        public abstract void ApplyType(HtmlNode htmlNode, ViewComponents components);

        internal void TextToSpan(string content, ViewComponents components, SpanStyle style)
        {
            // basic preparation of text
            string cContent = GetCleanContent(content);
            cContent = ChangeIcons(cContent);
            if (string.IsNullOrEmpty(cContent))
                return;

            // split text at known @-tags to insert a Hyperlink at that position to the user's profile page in browser
            // --> since Friendica 2019.09 we get friendica_html as output field containing more information, including links to profiles 
            var span = new Span()
            {
                Text = cContent,
                Style = (Style)Application.Current.Resources["SpanStandard"]
            };
            SpanStyle.SetStyle(span, style);
            if (components.FormattedText == null)
                components.FormattedText = new FormattedString();
            components.FormattedText.Spans.Add(span);

        }

        // insert a blank line as a paragraph separator
        internal void InsertBr(ViewComponents components)
        {
            var label = new Label() { Text = " " };
            components.AddElement(label);
        }

        // replace special html characters like &uuml; etc. with correct characters
        internal string GetCleanContent(string content)
        {
            return HtmlEntity.DeEntitize(content);
        }

        // change icons and replace special html characters
        internal string GetIconizedCleanContent(string content)
        {
            var contentIconized = ChangeIcons(content);
            return HtmlEntity.DeEntitize(contentIconized);
        }


        // replace smilies, icons etc. in content
        internal string ChangeIcons(string content)
        {
            // Caution: order of replacements can be important, i.e. the 2nd devil smiley needs to be replaced before the happy smiley :-)
            // symbol for like/dislike
            content = content.Replace(":like", System.Net.WebUtility.HtmlDecode("&#x1F44D;"));
            content = content.Replace(":dislike", System.Net.WebUtility.HtmlDecode("&#x1F44E;"));
            // symbol facepalm 
            content = content.Replace(":facepalm", System.Net.WebUtility.HtmlDecode("&#x1F926;"));
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

            // generally replace smilies
            content = System.Net.WebUtility.HtmlDecode(content.Replace("&##1", "&#1"));
            return content;
        }
    }


    /// <summary>
    /// fallback class for all non-treated html types to convert the text into a span element
    /// </summary>
    public class DefaultToView : HtmlTypeBase
    {
        string tag;

        public DefaultToView(string tag = "")
        {
            this.tag = (tag == "") ? "#text" : tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, ViewComponents components)
        {
            if (components.FormattedText == null)
                components.FormattedText = new FormattedString();
            TextToSpan(htmlNode.InnerText, components, SpanStyle.GetStyle(htmlNode));
        }
    }


    /// <summary>
    /// create a line break in the output = empty label
    /// </summary>
    public class BrToView : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "br"; }
        }

        public override void ApplyType(HtmlNode htmlNode, ViewComponents components)
        {
            // don't work on second tag if we came accross a "<br><br>"
            if (htmlNode.PreviousSibling != null && htmlNode.PreviousSibling.Name == "br")
                return;

            // finalize current span elements
            if (components.FormattedText != null)
            {
                components.AddElement(new Label() { FormattedText = components.FormattedText });
                components.FormattedText = null;
            }

            // add another line with space only if we see a "<br><br>"
            if (htmlNode.NextSibling != null && htmlNode.NextSibling.Name == "br") 
                InsertBr(components);
        }

    }

    /// <summary>
    /// create a line in the output
    /// </summary>
    public class HrToView : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "hr"; }
        }

        public override void ApplyType(HtmlNode htmlNode, ViewComponents components)
        {
            if (components.FormattedText != null)
            {
                components.AddElement(new Label() { FormattedText = components.FormattedText });
                components.FormattedText = null;
            }

            var line = new BoxView()
            {
                BackgroundColor = Color.FloralWhite,
                Margin = new Thickness(24, 0, 24, 0),
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.Fill
            };
            components.AddElement(line);
        }
    }

    /// <summary>
    /// create header lines - we will not distinguish between h1, h2 etc. all have the same size in app
    /// </summary>
    public class HToView : HtmlTypeBase
    {
        string tag;

        public HToView(string tag)
        {
            this.tag = tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, ViewComponents components)
        {
            var label = new Label()
            {
                Text = GetCleanContent(htmlNode.InnerText),
                Style = (Style)Application.Current.Resources["LabelHtmlHeader"]
            };

            if (htmlNode.FirstChild != null && htmlNode.FirstChild.Name == "a")
            {
                // header contains link
                var aTag = htmlNode.FirstChild;
                var url = aTag.Attributes["href"];
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (sender, e) => { Device.OpenUri(new Uri(url.Value)); };
                label.GestureRecognizers.Add(tapGesture);
                label.TextDecorations = TextDecorations.Underline;
            }
            components.AddElement(label);
        }

    }


    /// <summary>
    /// create links - includes also handling of video and audio playback
    /// </summary>
    public class AToView : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "a"; }
        }

        public override async void ApplyType(HtmlNode htmlNode, ViewComponents components)
        {
            var url = htmlNode.Attributes["href"];
            if (url == null)
                throw new Exception("href");

            var target = htmlNode.Attributes["target"];
            // someone wrote an entry with a text like "http://friendica..." - it was not intended to be a link
            // to somewhere but it throwed an exception which we now catch and write the text as plain text
            try
            {
                // handle youtube videos separately as they cannot be played in video player
                if (url.Value.Contains("youtube.com/"))
                {
                    // we need to add the stack and grid for image and title before loading data async (after await it is too late)
                    var grid = new Grid() { HorizontalOptions = LayoutOptions.FillAndExpand };
                    var stack = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand };
                    stack.Children.Add(grid);
                    components.AddElement(stack);

                    // load embed data from youtube
                    var helpers = new HttpFriendicaHelpers(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
                    var embedResults = await helpers.GetOEmbedYoutubeByUrlAsync(url.Value);
                    switch (embedResults)
                    {
                        // display image and information 
                        case HttpFriendicaHelpers.OEmbedResults.OK:
                            // image with play icon
                            var image = new Image()
                            {
                                Source = helpers.OEmbedYoutube.ThumbnailUrl
                            };
                            var playIcon = new FontIconLabel()
                            {
                                Text = Styles.Icon.FIPlay,
                                TextColor = Color.White,
                                FontSize = 48,
                                FontAttributes = FontAttributes.Bold,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Center
                            };
                            image.SizeChanged += (sender, e) => { image.HeightRequest = App.ShellHeight / ((image.Height > image.Width) ? 2 : 3); };
                            grid.Children.Add(image);
                            grid.Children.Add(playIcon);

                            // text with link 
                            var spanProvider = new Span()
                            {
                                Text = helpers.OEmbedYoutube.ProviderName + ": ",
                                Style = (Style)Application.Current.Resources["SpanBold"]
                            };
                            var spanLink = new Span()
                            {
                                Text = helpers.OEmbedYoutube.Title,
                                Style = (Style)Application.Current.Resources["SpanHyperlink"]
                            };
                            var tapGesture = new TapGestureRecognizer();
                            tapGesture.Tapped += (sender, e) => { Device.OpenUri(new Uri(url.Value)); };
                            spanLink.GestureRecognizers.Add(tapGesture);
                            grid.GestureRecognizers.Add(tapGesture);
                            var spanAuthor = new Span()
                            {
                                Text = " (" + helpers.OEmbedYoutube.AuthorName + ")",
                                Style = (Style)Application.Current.Resources["SpanStandard"]
                            };
                            if (components.FormattedText == null)
                                components.FormattedText = new FormattedString();
                            // add text below to image into stack
                            components.FormattedText.Spans.Add(spanProvider);
                            components.FormattedText.Spans.Add(spanLink);
                            components.FormattedText.Spans.Add(spanAuthor);
                            var label = new Label() { FormattedText = components.FormattedText, Margin = new Thickness(0, 0, 0, 24) };
                            components.FormattedText = new FormattedString();
                            stack.Children.Add(label);
                            break;
                        // on error we will only display the url as a clickable link
                        case HttpFriendicaHelpers.OEmbedResults.NotAnswered:
                        case HttpFriendicaHelpers.OEmbedResults.NoUrl:
                        case HttpFriendicaHelpers.OEmbedResults.SerializationError:
                        case HttpFriendicaHelpers.OEmbedResults.UnexpectedError:
                            AddMediaLink(components, url.Value, true);
                            break;
                    }
                }
                // play videos in internal video player 
                else if (url.Value.EndsWith("mpg") || url.Value.EndsWith("mp4"))
                {
                    AddMediaLink(components, url.Value, true);
                }
                // play audio files in internal player
                else if (url.Value.EndsWith("mp3"))
                {
                    AddMediaLink(components, url.Value, false);
                }
                // handle normal links 
                else
                {
                    bool imageFound = false;
                    // check if there is a child with an image
                    if (htmlNode.HasChildNodes)
                    {
                        foreach (var childHtmlNode in htmlNode.ChildNodes)
                        {
                            if (childHtmlNode.Name == "img")
                            {
                                // set true to avoid placing the link additionally to the image
                                imageFound = true;

                                // write collected text to view before placing the image
                                if (components.HasOpenSpans())
                                    components.CloseSpan();

                                var imageInA = new ImageToView();
                                imageInA.ApplyType(childHtmlNode, components);
                            }
                        }
                    }

                    if (!imageFound)
                    {
                        var span = new Span()
                        {
                            Text = (htmlNode.FirstChild.Name == "#text") ? GetIconizedCleanContent(htmlNode.FirstChild.InnerText) : null,
                            Style = (Style)Application.Current.Resources["SpanHyperlink"]
                        };
                        // fall back to get the author name if it is a shared content
                        if (string.IsNullOrEmpty(span.Text))
                        {
                            if (htmlNode.FirstChild.Name == "span" && htmlNode.FirstChild.GetAttributeValue("class", "") == "shared-author")
                                span.Text = GetIconizedCleanContent(htmlNode.FirstChild.InnerText);
                        }
                        // fallback to show url if nothing else was found
                        if (string.IsNullOrEmpty(span.Text))
                            span.Text = url.Value;

                        var tapGesture = new TapGestureRecognizer();
                        tapGesture.Tapped += (sender, e) => { Device.OpenUri(new Uri(url.Value)); };
                        span.GestureRecognizers.Add(tapGesture);
                        if (components.FormattedText == null)
                            components.FormattedText = new FormattedString();
                        components.FormattedText.Spans.Add(span);
                    }
                }
            }
            catch { TextToSpan(htmlNode.InnerText, components, SpanStyle.GetStyle(htmlNode)); }
        }

        private void AddMediaLink(ViewComponents components, string url, bool isVideo)
        {
            // show a symbol and the url of the media file in a horizontal stack
            var iconLabel = new FontIconLabel()
            {
                FontSize = 20,
                WidthRequest = 36,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.White
            };
            iconLabel.Text = (isVideo) ? Styles.Icon.FIVideo : Styles.Icon.FIAudio;
            var label = new Label()
            {
                Text = url,
                Style = (Style)Application.Current.Resources["SpanHyperlink"]
            };
            var stack = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12)
            };
            stack.Children.Add(iconLabel);
            stack.Children.Add(label);
            components.AddElement(stack);

            // on tapping the link a small overlay with the player is shown 
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (sender, e) =>
            {
                var nav = Application.Current.MainPage as NavigationPage;
                var shell = nav.RootPage as Views.CustomShell;
                var page = shell.Detail as Styles.BaseContentPage;
                var mvvm = page.BindingContext as ViewModel.BaseViewModel;
                mvvm.MediaUrlVM = url;
                mvvm.IsAudioOnly = false;
                mvvm.IsMediaPlayerVisible = true;
            };
            stack.GestureRecognizers.Add(tapGesture);
        }
    }



    /// <summary>
    /// create an image within the content view - tappable to open a bigger version with zooming option
    /// </summary>
    public class ImageToView : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "img"; }
        }

        public override void ApplyType(HtmlNode htmlNode, ViewComponents components)
        {
            var src = htmlNode.Attributes["src"];
            if (src == null)
                throw new Exception("missing src");
            // retrieve alt text from img html tag to use in tooltips (Windows, macOS)
            var altAtt = htmlNode.Attributes["alt"];
            string alt = "";
            if (altAtt != null)
                alt = altAtt.DeEntitizeValue;
            // check if we have a smiley
            var classSmiley = htmlNode.Attributes["class"];
            var isSmiley = (classSmiley == null) ? false : htmlNode.Attributes["class"].Value == "smiley";
            // check if there is a defined size request in url
            var sizeStyle = htmlNode.Attributes["style"];
            var sizeRequested = (sizeStyle == null) ? 0 : Convert.ToDouble(Regex.Match(sizeStyle.Value, @"\d+").Value) * 5;

            // tried using CachedImage from FFImageLoading instead of XF.Image as CachedImage provides LoadingPlaceholder 
            // and ErrorPlaceholder, but CachedImage was not working on old iPad2, so realized ErrorPlaceholder with two
            // images stacked over each other

            var image = new CustomImage()
            {
                HorizontalOptions = LayoutOptions.Center,
                Tooltip = alt
            };

            // MacOS needs always height and width, however we do not know the sizes for photos from internet, so width is 
            // maximum of shell, height is depending on orientation
            if (Device.RuntimePlatform == Device.macOS)
            {
                var maxHeight = App.ShellHeight / ((image.Height > image.Width) ? 2 : 3);
                image.HeightRequest = maxHeight;
                image.WidthRequest = App.ShellWidth;
            }

            // load image source
            ImageSource imageSource = null;
            if (src.Value.Contains("data:image/jpeg;base64") || src.Value.Contains("data:image/png;base64"))
            {
                src.Value = src.Value.Replace("data:image/jpeg;base64,", "");
                src.Value = src.Value.Replace("data:image/png;base64,", "");
                imageSource = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(src.Value)));
            }
            else
            {
                if (src.Value.StartsWith("redir"))
                {
                    // TODO: was machen wir damit?
                }
                else
                    imageSource = ImageSource.FromUri(new Uri(src.Value));
            }
                
            image.Source = imageSource;

            // add tap gesture to open image in a fullscreen window
            var gesture = new TapGestureRecognizer();
            gesture.Tapped += (sender, e) =>
            {
                // open image in a new window as within the current NavigationPage we cannot get rid of the title bar
                // pass over the source data into the new window
                var imageClicked = sender as Image;
                var fullscreen = new Views.ImageFullscreen(imageClicked.Source);
                var nav = Application.Current.MainPage as NavigationPage;

                // set title bar to black is looking better for fullscreen displaying
                nav.BarBackgroundColor = Color.Black;
                nav.Popped += (sender2, e2) =>
                {
                    // go back to correct title bar color after returning from fullscreen
                    nav.BarBackgroundColor = (OnPlatform<Color>)Application.Current.Resources["AccentColor"];
                };
                Application.Current.MainPage.Navigation.PushAsync(fullscreen);
            };
            image.GestureRecognizers.Add(gesture);

            // TODO: not quite finalized coding, some images are correct some are not
            // limit height of the loaded image
            //image.SizeChanged += (sender, e) =>
            //{
            //    var imageInGrid = sender as CustomImage;
            //    if (imageInGrid.IsLoading)
            //        return;

            //    var gridImage = imageInGrid.Parent as Grid;
            //    var errorImageInGrid = gridImage.Children[0] as Image;

            //    // in case we have a user profile photo in a retweet we want to show it as a small icon only
            //    if (htmlNode.ParentNode.Name == "a")
            //    {
            //        var classType = htmlNode.ParentNode.GetAttributeValue("class", "");
            //        if (classType == "shared-userinfo")
            //        {
            //            imageInGrid.WidthRequest = 48;
            //            imageInGrid.HeightRequest = 48;
            //            errorImageInGrid.WidthRequest = 48;
            //            errorImageInGrid.HeightRequest = 48;
            //        }
            //    }
            //    else
            //    {
            //        // limit height to 1/3 of screen on landscape images or 1/2 on portrait images
            //        var isLandscape = (imageInGrid.Width > imageInGrid.Height);
            //        var maxHeight = App.ShellHeight / (isLandscape ? 3 : 2);

            //        // if image is bigger than allowed height reduce to allowed height
            //        if (imageInGrid.Height > maxHeight)
            //            imageInGrid.HeightRequest = maxHeight;
            //        // if image is bigger than available width reduce to shell width
            //        else if (imageInGrid.Width > App.ShellWidth)
            //            imageInGrid.WidthRequest = App.ShellWidth;
            //        // image is smaller so let's scale up to fill width
            //        else
            //        {
            //            var scale = gridImage.Width / imageInGrid.Width;
            //            var requestedImageHeight = (imageInGrid.Height * scale > maxHeight) ? maxHeight : imageInGrid.Height * scale;
            //            if (imageInGrid.Scale == 1)
            //                gridImage.HeightRequest = requestedImageHeight;
            //            imageInGrid.Scale = scale;
            //        }
            //    }

            //    // make errorimage invisible if loading has succeed, otherwise it could happen that real image is smaller than 96px and noimage.jpg gets visibile behind it
            //    if (image.Height > 0 && image.Width > 0)
            //        errorImageInGrid.IsVisible = false;
            //};

            // stack an errorimage below the image in a grid so user see noimage.jpg if error on loading real image
            var errorImage = new CustomImage()
            {
                HorizontalOptions = LayoutOptions.Center,
                Source = ImageSource.FromResource("Friendica_Mobile.noimage.jpg"),
                Tooltip = "errorimage",
                WidthRequest = 96,
                HeightRequest = 96
            };
            if (isSmiley)
                errorImage.WidthRequest = errorImage.HeightRequest = 32;

            var grid = new Grid() { Margin = new Thickness(0, 24, 0, 24) };

            // set small size if we are displaying a user profile image in a retweeted post
            if (htmlNode.ParentNode.Name == "a" && htmlNode.ParentNode.GetAttributeValue("class", "") == "shared-userinfo")
            {
                grid.WidthRequest = 48;
                grid.HeightRequest = 48;
            }

            grid.Children.Add(errorImage);
            grid.Children.Add(image);

            grid.SizeChanged += (sender, e) =>
            {
                
                var gridImage = sender as Grid;

                var imageInGrid = gridImage.Children[1] as CustomImage;
                var errorImageInGrid = gridImage.Children[0] as CustomImage;
                if (imageInGrid.IsLoading)
                    return;

                // in case we have a user profile photo in a retweet we want to show it as a small icon only
                if (htmlNode.ParentNode.Name == "a") // TODO: && htmlNode.ParentNode.GetAttributeValue("class", "") == "shared-userinfo"
                {
                    var classType = htmlNode.ParentNode.GetAttributeValue("class", "");
                    if (classType == "shared-userinfo")
                    {
                        imageInGrid.WidthRequest = 48;
                        imageInGrid.HeightRequest = 48;
                        errorImageInGrid.WidthRequest = 1;
                        errorImageInGrid.HeightRequest = 1;
                    }
                }
                else
                {
                    //if (isSmiley)
                    //{
                    //    gridImage.WidthRequest = gridImage.HeightRequest = 32;
                    //}
                    //else
                    //{
                        // limit height to 1/3 of screen on landscape images or 1/2 on portrait images
                        var isLandscape = (imageInGrid.Width > imageInGrid.Height);
                        var factor = imageInGrid.Width / imageInGrid.Height;
                        var maxHeight = App.ShellHeight / (isLandscape ? 3 : 2);
                        //if (sizeRequested > 0 && sizeRequested < maxHeight)
                        //    maxHeight = sizeRequested;

                        // calculate scal of image to available size
                        var scale = gridImage.Width / imageInGrid.Width;
                        var requestedImageHeight = (imageInGrid.Height * scale > maxHeight) ? maxHeight : imageInGrid.Height * scale;
                        var requestedImageWidth = requestedImageHeight * factor;
                        var newScale = requestedImageWidth / imageInGrid.Width;

                        // scale image up or down and set grid to scaled image size
                        if (imageInGrid.Scale == 1)
                        {
                            gridImage.WidthRequest = requestedImageWidth;
                            gridImage.HeightRequest = requestedImageHeight;
                            imageInGrid.Scale = newScale;
                        }
                    //}
                }

                // make errorimage invisible if loading has succeed, otherwise it could happen that real image is smaller than 96px and noimage.jpg gets visibile behind it
                if (imageInGrid.Height > 0 && imageInGrid.Width > 0)
                    errorImageInGrid.IsVisible = false;
            };
            components.AddElement(grid);
        }
    }

    /// <summary>
    /// create unordinated lists - we will not distinguish between h1, h2 etc. all have the same size in app
    /// </summary>
    public class ListToView : HtmlTypeBase
    {
        string tag;

        public ListToView(string tag)
        {
            this.tag = tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, ViewComponents components)
        {
            var styleType = htmlNode.Attributes[0].Value;

            if (styleType == "listdecimal")
            {
                var counter = 0;
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    counter += 1;
                    TextToSpan("      " + counter + ".   " + childHtmlNode.InnerText, components, SpanStyle.GetStyle(htmlNode));
                    components.CloseSpan();
                }
            }
            else
            {
                // styleType = "listbullet" as well as no declaration
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    TextToSpan("      " + "&#x2B25;" + "   " + childHtmlNode.InnerText, components, SpanStyle.GetStyle(htmlNode));
                    components.CloseSpan();
                }
                // TODO: containing links?
            }
        }
    }

    /// <summary>
    /// create quotes within the content view - have lighter background to distinguish it better from other content
    /// </summary>
    public class BlockquoteToView : HtmlTypeBase
    {
        public override string TagName
        {
            get { return "blockquote"; }
        }

        public override void ApplyType(HtmlNode htmlNode, ViewComponents components)
        {
            // different handling when we want to display a retweeted item
            if (htmlNode.GetAttributeValue("class", "") == "shared_content")
            {
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    // the remainder of the retweeted text can be displayed as usual (blockquote style)
                    var remainingText = new HtmlToXamarinViews(childHtmlNode.InnerHtml);
                    remainingText.ConvertHtmlNode(childHtmlNode, components);
                }
            }
            else
            {
                if (htmlNode.InnerText == "")
                    return;

                if (components.HasOpenSpans())
                    components.CloseSpan();

                // prepare a new paragraph with the quote
                var outerFrame = new Frame()
                {
                    BackgroundColor = Color.Black,
                    Opacity = 0.5,
                    Margin = new Thickness(8, 0, 8, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    CornerRadius = 0,
                    HasShadow = false
                };
                var innerFrame = new Frame()
                {
                    BackgroundColor = Color.AntiqueWhite,
                    Margin = new Thickness(4, 0, 0, 0),
                    Padding = new Thickness(4, 4, 4, 4),
                    CornerRadius = 0,
                    HasShadow = false
                };
                var stack = new StackLayout();
                innerFrame.Content = stack;

                var quoteComponents = new ViewComponents();
                quoteComponents.ViewElements = new List<View>();
                foreach (var childHtmlNode in htmlNode.ChildNodes)
                {
                    // the remainder of the retweeted text can be displayed as usual (blockquote style)
                    var blockquoteContent = new HtmlToXamarinViews(childHtmlNode.InnerHtml);
                    blockquoteContent.ConvertHtmlNode(childHtmlNode, quoteComponents);
                }

                foreach (var element in quoteComponents.ViewElements)
                {
                    if (element.GetType() == typeof(Label))
                    {
                        var label = new Label()
                        {
                            TextColor = Color.Black,
                            FontAttributes = FontAttributes.Italic,
                            FontSize = 12
                        };
                        string text = "";
                        foreach (var span in ((Label)element).FormattedText.Spans)
                            text += span.Text;
                        label.Text = text;
                        stack.Children.Add(label);
                    }
                    else
                        stack.Children.Add(element);
                }


                //var label = new Label()
                //{
                //    TextColor = Color.Black,
                //    FontAttributes = FontAttributes.Italic,
                //    FontSize = 12
                //};
                //// basic preparation of text
                //string cContent = GetCleanContent(htmlNode.InnerText);
                //cContent = ChangeIcons(cContent);
                //label.Text = cContent;

                //stack.Children.Add(label);
                outerFrame.Content = innerFrame;
                components.AddElement(outerFrame);
            }

        }
    }

    /// <summary>
    /// create code blocks - distinguish it better from other content, use fixed-width font, tap to open bigger window if more code included
    /// </summary>
    public class CodeToView : HtmlTypeBase
    {
        string tag;

        public CodeToView(string tag)
        {
            this.tag = tag;
        }

        public override string TagName
        {
            get { return this.tag; }
        }

        public override void ApplyType(HtmlNode htmlNode, ViewComponents components)
        {
            if (htmlNode.InnerText == "")
                return;

            if (components.HasOpenSpans())
                components.CloseSpan();

            var frame = PrepareCodebox(htmlNode, true);
            components.AddElement(frame);
        }

        private Frame PrepareCodebox(HtmlNode htmlNode, bool shortenedView)
        {
            // prepare a new paragraph with the code
            var frame = new Frame()
            {
                BackgroundColor = Color.AntiqueWhite,
                Opacity = 0.7,
                BorderColor = Color.Black,
                Margin = new Thickness(8, 0, 8, 0),
                Padding = new Thickness(4, 4, 4, 4),
                CornerRadius = 0,
                HasShadow = false
            };
            var stack = new StackLayout();
            frame.Content = stack;
            var label = new Label()
            {
                TextColor = Color.Black,
                FontFamily = (Device.RuntimePlatform == Device.Android) ? "monospace" : "Courier New",
                FontSize = (shortenedView) ? 12 : 14
            };

            if (htmlNode.HasChildNodes)
            {
                var html = (htmlNode.Name == "pre") ? htmlNode.ChildNodes[0].ChildNodes : htmlNode.ChildNodes;

                int counter = 0;
                foreach (var childHtmlNode in html)
                {
                    string cContent = GetCleanContent(childHtmlNode.InnerText);
                    // shorten text in list of posts, only in flyout we will show everything
                    if (shortenedView)
                    {
                        if (counter > 4)
                            continue;
                        if (counter > 3)
                        {
                            counter += 1;
                            label.Text += Environment.NewLine + Environment.NewLine + "... " + AppResources.TextDisplayCodeFullscreenHint +  "  ...";
                            // add tap gesture to open in a fullscreen window
                            var gesture = new TapGestureRecognizer();
                            gesture.Tapped += (sender, e) =>
                            {
                                var nav = Application.Current.MainPage as NavigationPage;
                                var shell = nav.RootPage as Views.CustomShell;
                                var page = shell.Detail as Styles.BaseContentPage;
                                var mvvm = page.BindingContext as ViewModel.BaseViewModel;
                                mvvm.CodeFullscreenContent = PrepareCodebox(htmlNode, false);
                                mvvm.IsCodeFullscreenVisible = true;
                            };
                            frame.GestureRecognizers.Add(gesture);
                        }
                    }
                    if (childHtmlNode.Name == "#text")
                    {
                        // include line number before code text, if there is more than 1 line
                        counter += 1;
                        if (html.Count > 1)
                            label.Text += "L" + string.Format("{0:00}", counter) + "| " + cContent;
                        else
                            label.Text += cContent;
                    }
                    // if there is a link within a code block, only display the text
                    if (childHtmlNode.Name == "a")
                    {
                        label.Text += cContent;
                    }
                    if (childHtmlNode.Name == "br")
                    {
                        // check if not null, as this has caused an unhandled exception on one entry
                        if (childHtmlNode.PreviousSibling != null && childHtmlNode.PreviousSibling.Name == "br")
                        {
                            counter += 1;
                            label.Text += "L" + string.Format("{0:00}", counter) + "| " + Environment.NewLine;
                        }
                        else
                            label.Text += Environment.NewLine;
                    }
                }
            }
            stack.Children.Add(label);
            return frame;
        }
    }
}
