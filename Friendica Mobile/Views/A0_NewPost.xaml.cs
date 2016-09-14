using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Friendica_Mobile.Views
{
    /// <summary>
    /// View for creating new posts
    /// </summary>
    public sealed partial class A0_NewPost : Page
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        // color definitions
        Color QuoteColor = Colors.Gold;
        List<string> colors = new List<string>(new string[] { "000000", "808080", "C0C0C0", "FFFFFF",
                                                                  "800080", "000080", "0000FF", "00FFFF",
                                                                  "800000", "FF0000", "FFA500", "FFFF00",
                                                                  "008000", "808000", "00FF00", "FF00FF" });


        public A0_NewPost()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            // going back to last back after user has clicked on the "Sending" button
            pageMvvm.SendingNewPostStarted += PageMvvm_SendingNewPostStarted;
            this.Loaded += A0_NewPost_Loaded;
        }

        private void A0_NewPost_Loaded(object sender, RoutedEventArgs e)
        {
            VisualStateSelector selector = new VisualStateSelector(this);
            App.IsNavigating = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // if no transferred parameter start a new fresh post (not a comment) where ACL settings are displayed
            FriendicaNewPost newPost = new FriendicaNewPost();
            pageMvvm.ShowACLVisible = true;
    
            if (e.Parameter != null)
            {
                // if user has dropped something on the DropArea in Network.xaml he will land here with a FriendicaNewPost instance filled with the data
                // TODO: user could also land here from Cortana speech dictation of a new post (not yet implemented)
                // TODO: if user answers a toast message directly in action center, the answer lands here - difference: not showing post but directly posting to server
                if (e.Parameter.GetType() == typeof(clsConvertDroppedContent))
                {
                    var convert = e.Parameter as clsConvertDroppedContent;
                    pageMvvm.NewPostImage = convert.Image;
                    pageMvvm.ImageLatitude = convert.ImageLatitude;
                    pageMvvm.ImageLongitude = convert.ImageLongitude;
                    rebNewPostText.Document.SetText(TextSetOptions.None, convert.Post.NewPostStatus);
                    if (convert.Weblink != null)
                    {
                        var textFound = rebNewPostText.Document.Selection.FindText(convert.Weblink.AbsoluteUri, TextConstants.MaxUnitCount, FindOptions.None);
                        if (textFound != 0)
                        {
                            var link = convert.Weblink.AbsoluteUri;
                            ITextRange tr = rebNewPostText.Document.Selection;
                            if (String.IsNullOrEmpty(tr.Text))
                                return;
                            if (String.IsNullOrEmpty(link))
                                return;
                            if (!link.StartsWith("\""))
                                link = "\"" + link;
                            if (!link.EndsWith("\""))
                                link += "\"";
                            try
                            {
                                tr.Link = link;
                                tr.CharacterFormat.ForegroundColor = Colors.DarkBlue;
                                tr.CharacterFormat.Underline = UnderlineType.Thick;
                            }
                            catch
                            {
                            }
                        }
                    }
                    newPost = convert.Post;
                }

                // user clicked the "+" button on a posting to add a comment
                else if (e.Parameter.GetType() == typeof(FriendicaPostExtended))
                {
                    var mvvm = this.DataContext as NewPostViewmodel;
                    mvvm.ShowThread = new ObservableCollection<FriendicaThread>();
                    mvvm.PostToShow = e.Parameter as FriendicaPostExtended;
                    mvvm.LoadThread();
                    if (mvvm.ShowThread != null && mvvm.ShowThread.Count != 0)
                        newPost.NewPostInReplyToStatusId = mvvm.ShowThread.ElementAt(0).Posts.ElementAt(0).Post.PostId;
                    pageMvvm.ShowACLVisible = false;
                    pageMvvm.ShowThreadVisible = true;
                    pageMvvm.ReloadThreadData();
                }

                // user clicked the "+" button in ShowThread.xaml
                else if (e.Parameter.GetType() == typeof(ObservableCollection<FriendicaThread>))
                {
                    // collapse ACL setter box on a reply
                    pageMvvm.ShowACLVisible = false;
                    pageMvvm.ShowThreadVisible = true;
                    pageMvvm.ShowThread = e.Parameter as ObservableCollection<FriendicaThread>;
                    // set id of post where we want to reply to
                    var thread = pageMvvm.ShowThread.ElementAt(0);
                    newPost.NewPostInReplyToStatusId = thread.Posts.ElementAt(0).Post.PostId;
                    thread.NewPosts = thread.Posts.Where(m => m.NewEntryIndicatorBorder.Bottom == 4);
                    foreach (var post in thread.Posts)
                    {
                        if (!post.IsComment)
                            post.ToggleShowCommentsState = false;
                    }
                    thread.ReduceComments();
                    thread.IsLoaded = true;
                    pageMvvm.ReloadThreadData();
                }
            }

            newPost.NewPostSource = "Friendica";
            newPost.NewPostNetwork = "dfrn";

            pageMvvm.newPost = newPost;
            base.OnNavigatedTo(e);
            VisualStateSelector selector = new VisualStateSelector(this);
        }


        private string PrepareBBCode()
        {
            // Takes a RichEditBox control and returns a simple BBCode-formatted version of its contents
            string text, bbCode = "", strColour, strBold, strUnderline, strItalic, strBackground, strFont, strListType, strLink;
            bool colorActive = false, boldActive = false, italicActive = false, underlineActive = false, quoteActive = false, codeActive = false;
            bool listActive = false, linkActive = false;
            int lngOriginalStart, lngOriginalLength;
            int paragraphIndex, intCount, length = 0;
            ITextRange tr = rebNewPostText.Document.Selection;

            rebNewPostText.Document.GetText(TextGetOptions.None, out text);
            length = text.Length;

            // Store original selections, then select complete range from 1st character
            lngOriginalStart = tr.StartPosition;
            lngOriginalLength = tr.EndPosition;
            tr.SetRange(0, length);

            // Set up initial parameters
            paragraphIndex = tr.GetIndex(TextRangeUnit.Paragraph);
            strListType = tr.ParagraphFormat.ListType.ToString();
            strBold = tr.CharacterFormat.Bold.ToString();
            strItalic = tr.CharacterFormat.Italic.ToString();
            strUnderline = tr.CharacterFormat.Underline.ToString();
            strColour = tr.CharacterFormat.ForegroundColor.ToString();
            strBackground = tr.CharacterFormat.BackgroundColor.ToString();
            strFont = tr.CharacterFormat.Name;
            strLink = tr.Link;

            // include list formatting (we only support bullets)
            if (strListType.ToLower() == "bullet")
            {
                bbCode += "[list][*]";
                listActive = true;
            }

            // include link
            if (!String.IsNullOrEmpty(tr.Link))
            {
                var linkTarget = tr.Link;
                linkTarget = linkTarget.Replace("\"", "");
                var visibleText = tr.Text;
                visibleText = visibleText.Replace("HYPERLINK ", "");
                visibleText = visibleText.Replace(tr.Link, "");
                bbCode += "[url=" + linkTarget + "]" + visibleText + "[/url]";
                linkActive = true;
            }

            // Include color
            if (colors.Contains(strColour))
            {
                bbCode += "[color=#" + strColour.Substring(3) + "]";
                colorActive = true;
            }

            // Include bold tag, if required
            if (strBold.ToLower() == "on")
            {
                bbCode += "[b]";
                boldActive = true;
            }
            // Include italic tag, if required
            if (strItalic.ToLower() == "on" && strBackground != QuoteColor.ToString())
            {
                bbCode += "[i]";
                italicActive = true;
            }
            if (strUnderline.ToLower() == "on")
            {
                bbCode += "[u]";
                underlineActive = true;
            }

            // Loop around all remaining characters
            for (intCount = 0; intCount <= length; intCount++)
            {
                // Select current character
                tr.SetRange(intCount, intCount + 1);

                // jump over if still within link
                if (linkActive && tr.Link == strLink)
                    continue;
                else if (linkActive && tr.Link != strLink)
                {
                    linkActive = false;
                    continue;
                }
                else if (!linkActive && tr.Link != strLink)
                {
                    if (!String.IsNullOrEmpty(tr.Link))
                    {
                        var linkTarget = tr.Link;
                        linkTarget = linkTarget.Replace("\"", "");
                        var visibleText = tr.Text;
                        visibleText = visibleText.Replace("HYPERLINK ", "");
                        visibleText = visibleText.Replace(tr.Link, "");
                        bbCode += "[url=" + linkTarget + "]" + visibleText + "[/url]";
                        strLink = tr.Link;
                        linkActive = true;
                        continue;
                    }
                }

                //     Check/implement any changes in style
                if (tr.GetIndex(TextRangeUnit.Paragraph) != paragraphIndex)
                {
                    if (listActive && tr.ParagraphFormat.ListType.ToString().ToLower() == "bullet")
                        bbCode += "[*]";
                    else if (listActive && tr.ParagraphFormat.ListType.ToString().ToLower() != "bullet")
                    {
                        bbCode += "[/list]";
                        listActive = false;
                    }
                    else if (!listActive && tr.ParagraphFormat.ListType.ToString().ToLower() == "bullet")
                    {
                        bbCode += "[list][*]";
                        listActive = true;
                    }
                }

                // translate color from RichEditBox into HTML colors - only the 16 specified colors in text, because #xxxxxx would be recognised as a hashtag
                if (tr.CharacterFormat.ForegroundColor.ToString() != strColour)
                {
                    if (colorActive)
                    {
                        bbCode += "[/color]";
                        colorActive = false;
                    }
                    var color = tr.CharacterFormat.ForegroundColor.ToString().Substring(3);
                    if (color != "000000")
                    {
                        string colorName = "";
                        switch (color)
                        {
                            case "808080": colorName = "Gray"; break;
                            case "C0C0C0": colorName = "Silver"; break;
                            case "FFFFFF": colorName = "White"; break;
                            case "800080": colorName = "Purple"; break;
                            case "000080": colorName = "Navy"; break;
                            case "0000FF": colorName = "Blue"; break;
                            case "00FFFF": colorName = "Aqua"; break;
                            case "800000": colorName = "Maroon"; break;
                            case "FF0000": colorName = "Red"; break;
                            case "FFA500": colorName = "Orange"; break;
                            case "FFFF00": colorName = "Yellow"; break;
                            case "008000": colorName = "Green"; break;
                            case "808000": colorName = "Olive"; break;
                            case "00FF00": colorName = "Lime"; break;
                            case "FF00FF": colorName = "Fuchsia"; break;
                            default: colorName = "Black"; break;
                        }
                        bbCode += "[color=" + colorName + "]";
                        colorActive = true;
                    }
                }

                //     Check for bold changes
                if (tr.CharacterFormat.Bold.ToString().ToLower() != strBold.ToLower())
                {
                    if (boldActive && tr.CharacterFormat.Bold.ToString().ToLower() == "off")
                    {
                        bbCode += "[/b]";
                        boldActive = false;
                    }
                    else if (!boldActive && tr.CharacterFormat.Bold.ToString().ToLower() == "on")
                    {
                        bbCode += "[b]";
                        boldActive = true;
                    }
                }

                //     Check for italic changes
                if (tr.CharacterFormat.Italic.ToString().ToLower() != strItalic.ToLower())
                {
                    if (italicActive && tr.CharacterFormat.Italic.ToString().ToLower() == "off")
                    {
                        bbCode += "[/i]";
                        italicActive = false;
                    }
                    else if (!italicActive && tr.CharacterFormat.Italic.ToString().ToLower() == "on" && tr.CharacterFormat.BackgroundColor != QuoteColor)
                    {
                        bbCode += "[i]";
                        italicActive = true;
                    }
                }

                //     Check for underline changes
                if (tr.CharacterFormat.Underline.ToString().ToLower() != strUnderline.ToLower())
                {
                    if (underlineActive && tr.CharacterFormat.Underline.ToString().ToLower() != "single")
                    {
                        bbCode += "[/u]"; underlineActive = false;
                    }
                    else if (!underlineActive && tr.CharacterFormat.Underline.ToString().ToLower() == "single")
                    {
                        bbCode += "[u]"; underlineActive = true;
                    }
                }

                // check for quote blocks
                if (tr.CharacterFormat.BackgroundColor.ToString().ToLower() != strBackground.ToLower())
                {
                    if (quoteActive && tr.CharacterFormat.BackgroundColor.ToString().ToLower() != QuoteColor.ToString().ToLower())
                    {
                        bbCode += "[/quote]";
                        quoteActive = false;
                    }
                    else if (!quoteActive && tr.CharacterFormat.BackgroundColor.ToString().ToLower() == QuoteColor.ToString().ToLower())
                    {
                        bbCode += "[quote]";
                        quoteActive = true;
                    }
                }

                // check for code blocks
                if (tr.CharacterFormat.Name != strFont)
                {
                    if (codeActive && tr.CharacterFormat.Name != "Courier New")
                    {
                        bbCode += "[/code]";
                        codeActive = false;
                    }
                    else if (!codeActive && tr.CharacterFormat.Name == "Courier New")
                    {
                        bbCode += "[code]";
                        codeActive = true;
                    }
                }

                // Add the actual character
                bbCode += tr.Character;
                // Update variables with current style
                paragraphIndex = tr.GetIndex(TextRangeUnit.Paragraph);
                strListType = tr.ParagraphFormat.ToString();
                strBold = tr.CharacterFormat.Bold.ToString();
                strItalic = tr.CharacterFormat.Italic.ToString();
                strUnderline = tr.CharacterFormat.Underline.ToString();
                strColour = tr.CharacterFormat.ForegroundColor.ToString();
                strBackground = tr.CharacterFormat.BackgroundColor.ToString();
                strFont = tr.CharacterFormat.Name;
                strLink = tr.Link;
            }
            // Close off any open bold/italic tags
            if (boldActive)
                bbCode += "[/b]";
            if (italicActive)
                bbCode += "[/i]";
            if (underlineActive)
                bbCode += "[/u]";
            if (colorActive)
                bbCode += "[/color]";
            if (quoteActive)
                bbCode += "[/quote]";
            if (codeActive)
                bbCode += "[/code]";
            if (listActive)
                bbCode += "[/list]";
            // Restore original RichTextBox selection
            tr.SetRange(lngOriginalStart, lngOriginalLength);

            return bbCode;
        }


        private void SetButtonStates(ITextRange tr, bool boxFocused)
        {
            if (boxFocused)
            {
                buttonBold.IsEnabled = true;
                buttonItalic.IsEnabled = true;
                buttonUnderline.IsEnabled = true;
                buttonColor.IsEnabled = true;
                buttonCode.IsEnabled = true;
                buttonQuote.IsEnabled = true;
                buttonList.IsEnabled = true;
                buttonLink.IsEnabled = true;

                if (tr.CharacterFormat.Bold == FormatEffect.On)
                    buttonBold.IsChecked = true;
                else
                    buttonBold.IsChecked = false;

                if (tr.CharacterFormat.Italic == FormatEffect.On && tr.CharacterFormat.BackgroundColor != QuoteColor)
                    buttonItalic.IsChecked = true;
                else
                    buttonItalic.IsChecked = false;

                if (tr.CharacterFormat.Underline == UnderlineType.Single)
                    buttonUnderline.IsChecked = true;
                else
                    buttonUnderline.IsChecked = false;

                //var tb = buttonColor.Content as TextBlock;
                var tb = buttonColor.Content as FontIcon;
                if (colors.Contains(tr.CharacterFormat.ForegroundColor.ToString().Substring(3)))
                    tb.Foreground = new SolidColorBrush(tr.CharacterFormat.ForegroundColor);
                //else
                //    tb.Foreground = new SolidColorBrush(Colors.Black);

                if (tr.CharacterFormat.Name == "Courier New")
                    buttonCode.IsChecked = true;
                else
                    buttonCode.IsChecked = false;

                var border = buttonQuote.Content as Border;
                if (tr.CharacterFormat.BackgroundColor == QuoteColor)
                    buttonQuote.IsChecked = true;
                else
                    buttonQuote.IsChecked = false;

                if (tr.ParagraphFormat.ListType == MarkerType.Bullet)
                    buttonList.IsChecked = true;
                else
                    buttonList.IsChecked = false;

                var text = tr.Text;
                if (text.Contains("HYPERLINK"))
                {
                    buttonLinkCaption.Foreground = new SolidColorBrush(Colors.Blue);
                    buttonLink.Content = loader.GetString("buttonLinkShow");
                }
                else
                {
                    buttonLinkCaption.Foreground = new SolidColorBrush(Colors.Black);
                    buttonLink.Content = loader.GetString("buttonLinkInsert");
                }
            }
            else
            {
                buttonBold.IsEnabled = false;
                buttonItalic.IsEnabled = false;
                buttonUnderline.IsEnabled = false;
                buttonColor.IsEnabled = false;
                buttonCode.IsEnabled = false;
                buttonQuote.IsEnabled = false;
                buttonList.IsEnabled = false;
                buttonLink.IsEnabled = false;
            }

        }


        private void PageMvvm_SendingNewPostStarted(object sender, EventArgs e)
        {
            pageMvvm.SendingNewPostStarted -= PageMvvm_SendingNewPostStarted;

            // set indicator back to free navigation
            App.NavStatus = NavigationStatus.OK;

            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }


        private void rebNewPostText_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // change checked/unchecked state of the editor buttons if user changed the selected text
            var reb = sender as RichEditBox;
            ITextRange tr = reb.Document.Selection;
            bool boxFocused;

            if (reb.FocusState == FocusState.Keyboard || reb.FocusState == FocusState.Pointer || reb.FocusState == FocusState.Programmatic)
                boxFocused = true;
            else
                boxFocused = false;
            SetButtonStates(tr, boxFocused);
        }


        private void rebNewPostText_GotFocus(object sender, RoutedEventArgs e)
        {
            // enable editor buttons if user focuses the RichEditBox
            var reb = sender as RichEditBox;
            ITextRange tr = reb.Document.Selection;
            SetButtonStates(tr, true);
        }


        private void rebNewPostText_LostFocus(object sender, RoutedEventArgs e)
        {
            // loose focus of RichEditBox only if we are NOT moving focus to an editor button (otherwise user cannot click editor function)
            List<string> controls = new List<string>(new string[] { "buttonBold", "buttonItalic", "buttonUnderline",
                                            "buttonColor", "buttonCode", "buttonQuote", "buttonList", "buttonLink" });

            var focus = FocusManager.GetFocusedElement() as Control;
            if (focus == null || !controls.Contains(focus.Name))
            {
                SetButtonStates(null, false);

                // set BBcode in viewmodel only after loosing focus on RichEditBox (doing this on each change would need too much CPU power)
                var bbCode = PrepareBBCode();
                var mvvm = this.DataContext as NewPostViewmodel;
                mvvm.NewPostStatus = bbCode;
            }
        }


        private void buttonBold_Click(object sender, RoutedEventArgs e)
        {
            ITextRange tr = rebNewPostText.Document.Selection;
            tr.CharacterFormat.Bold = FormatEffect.Toggle;
            rebNewPostText.Focus(FocusState.Programmatic);
        }


        private void buttonItalic_Click(object sender, RoutedEventArgs e)
        {
            ITextRange tr = rebNewPostText.Document.Selection;
            if (tr.CharacterFormat.BackgroundColor != QuoteColor)
                tr.CharacterFormat.Italic = FormatEffect.Toggle;
            rebNewPostText.Focus(FocusState.Programmatic);
        }


        private void buttonUnderline_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            ITextRange tr = rebNewPostText.Document.Selection;
            if ((bool)button.IsChecked)
                tr.CharacterFormat.Underline = UnderlineType.Single;
            else
                tr.CharacterFormat.Underline = UnderlineType.None;
            rebNewPostText.Focus(FocusState.Programmatic);
        }


        private void Button_ColorSelector_Click(object sender, RoutedEventArgs e)
        {
            // take selected color from flyout (all 16 colors jump to this eventhandler), set selection with this color and 
            // change color button to this color
            var button = sender as Button;
            if (buttonColor.Flyout != null)
                buttonColor.Flyout.Hide();
            var color = button.Background as SolidColorBrush;
            ITextRange tr = rebNewPostText.Document.Selection;
            tr.CharacterFormat.ForegroundColor = color.Color;
            var fonticon = buttonColor.Content as FontIcon;
            fonticon.Foreground = new SolidColorBrush(tr.CharacterFormat.ForegroundColor);
            rebNewPostText.Document.ApplyDisplayUpdates();
            rebNewPostText.Focus(FocusState.Programmatic);
        }


        private void buttonCode_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            ITextRange tr = rebNewPostText.Document.Selection;
            if ((bool)button.IsChecked)
            {
                tr.CharacterFormat.BackgroundColor = Colors.LightGray;
                tr.CharacterFormat.Name = "Courier New";
            }
            else
            {
                tr.CharacterFormat.BackgroundColor = Colors.Transparent;
                tr.CharacterFormat.Name = "Segoe UI";
            }

            SetButtonStates(tr, true);
            rebNewPostText.Focus(FocusState.Programmatic);
        }


        private void buttonQuote_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            ITextRange tr = rebNewPostText.Document.Selection;
            if ((bool)button.IsChecked)
            {
                tr.CharacterFormat.Name = "Segoe UI";
                tr.CharacterFormat.BackgroundColor = QuoteColor;
                tr.CharacterFormat.Italic = FormatEffect.On;
            }
            else
            {
                tr.CharacterFormat.Name = "Segoe UI";
                tr.CharacterFormat.BackgroundColor = Colors.Transparent;
                tr.CharacterFormat.Italic = FormatEffect.Off;
            }
            SetButtonStates(tr, true);
            rebNewPostText.Focus(FocusState.Programmatic);
        }


        private void buttonList_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            ITextRange tr = rebNewPostText.Document.Selection;
            if ((bool)button.IsChecked)
            {
                tr.ParagraphFormat.ListType = MarkerType.Bullet;
            }
            else
            {
                tr.ParagraphFormat.ListType = MarkerType.None;
            }
            SetButtonStates(tr, true);
            rebNewPostText.Focus(FocusState.Programmatic);

        }


        private async void buttonLink_Click(object sender, RoutedEventArgs e)
        {
            ITextRange tr = rebNewPostText.Document.Selection;
            var text = tr.Text;
            if (text.Contains("HYPERLINK"))
            {
                inputLink.Text = tr.Link;
                buttonRemoveLink.Visibility = Visibility.Visible;
            }
            else if (tr.Text.Length == 0)
            {
                string errorMsg;
                errorMsg = loader.GetString("messageDialogNewPostLinkSelect");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
                if (buttonLink.Flyout != null)
                    buttonLink.Flyout.Hide();
            }
            else
            {
                inputLink.Text = "";
                buttonRemoveLink.Visibility = Visibility.Collapsed;
            }

        }


        private void inputLink_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var tb = sender as TextBox;
            // if user hits Enter set focus on the button
            if (e.Key == Windows.System.VirtualKey.Enter)
                buttonInsertLink.Focus(FocusState.Keyboard);

            // enable insert button only if link is valid
            var checkHttp = tb.Text.StartsWith("http://") || tb.Text.StartsWith("https://") || tb.Text.StartsWith("\"http://") || tb.Text.StartsWith("\"https://");
            if (tb.Text == "" || tb.Text == "http://" || tb.Text == "https://" || !checkHttp)
                buttonInsertLink.IsEnabled = false;
            else
                buttonInsertLink.IsEnabled = true;
        }


        private async void buttonInsertLink_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var buttonPanel = button.Parent as StackPanel;
            var sp = buttonPanel.Parent as StackPanel;
            if (buttonLink.Flyout != null)
                buttonLink.Flyout.Hide();
            string link = "";
            foreach (var child in sp.Children)
            {
                if (child.GetType() == typeof(TextBox))
                {
                    var control = child as Control;
                    if (control.Name == "inputLink")
                    {
                        var tb = child as TextBox;
                        link = tb.Text;
                        if (link == "https://" || link == "http://")
                            link = "";
                    }
                }
            }
            ITextRange tr = rebNewPostText.Document.Selection;
            if (String.IsNullOrEmpty(tr.Text))
                return;
            if (String.IsNullOrEmpty(link))
                return;
            if (!link.StartsWith("\""))
                link = "\"" + link;
            if (!link.EndsWith("\""))
                link += "\"";
            try
            {
                tr.Link = link;
                tr.CharacterFormat.ForegroundColor = Colors.DarkBlue;
                tr.CharacterFormat.Underline = UnderlineType.Thick;
            }
            catch
            {
                string errorMsg;
                errorMsg = loader.GetString("messageDialogNewPostLinkError");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }

            SetButtonStates(tr, true);
            rebNewPostText.Focus(FocusState.Programmatic);
        }


        private void buttonRemoveLink_Click(object sender, RoutedEventArgs e)
        {
            if (buttonLink.Flyout != null)
                buttonLink.Flyout.Hide();

            ITextRange tr = rebNewPostText.Document.Selection;
            tr.Link = "";
            tr.CharacterFormat.ForegroundColor = Colors.Black;
            tr.CharacterFormat.Underline = UnderlineType.None;
            SetButtonStates(tr, true);
            rebNewPostText.Focus(FocusState.Programmatic);
        }


        private void listBoxPrivateSelectedContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mvvm = this.DataContext as NewPostViewmodel;
            var listview = sender as ListBox;
            foreach (FriendicaUserExtended contact in e.AddedItems)
            {
                var isAlreadySelected = mvvm.SelectedContacts.Count(c => c.User.UserCid == contact.User.UserCid);
                if (isAlreadySelected == 0)
                    mvvm.SelectedContacts.Add(contact);
            }

            foreach (FriendicaUserExtended contact in e.RemovedItems)
                mvvm.SelectedContacts.Remove(contact);

            // initialize set sequence
            mvvm.SelectedContacts = mvvm.SelectedContacts;
        }


        private void listBoxPrivateSelectedGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mvvm = this.DataContext as NewPostViewmodel;
            var listview = sender as ListBox;
            foreach (FriendicaGroup group in e.AddedItems)
            {
                var isAlreadySelected = mvvm.SelectedGroups.Count(g => g.GroupGid == group.GroupGid);
                if (isAlreadySelected == 0)
                    mvvm.SelectedGroups.Add(group);
            }

            foreach (FriendicaGroup group in e.RemovedItems)
                mvvm.SelectedGroups.Remove(group);
            
            // initialize set sequence
            mvvm.SelectedGroups = mvvm.SelectedGroups;
        }


        private void listviewNetworkPosts_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var listview = sender as ListView;
            var control = e.OriginalSource as FrameworkElement;
            var post = control.DataContext as FriendicaPostExtended;

            // change focus to listview Richeditbox if still in focus
            if (rebNewPostText.FocusState != FocusState.Unfocused)
                listview.Focus(FocusState.Programmatic);           

            // show flyout with the plain text of the marked post
            textBoxTextForQuote.Text = post.Post.PostText;
            flyoutQuoteBox.ShowAt(listview);
        }


        private async void buttonInsertQuote_Click(object sender, RoutedEventArgs e)
        {
            MessageDialogMessage dialog;

            // check selected text, if user has nothing selected, take whole text (when longer then 160 chars ask user if wished to take all)
            var text = textBoxTextForQuote.SelectedText;
            if (text.Length == 0)
            {
                if (textBoxTextForQuote.Text.Length > 160)
                {
                    dialog = new MessageDialogMessage(loader.GetString("messageDialogNewPostQuoteLonger"), "", 
                                        loader.GetString("buttonOK"), loader.GetString("buttonCancel"));
                    await dialog.ShowDialog(0, 1);
                    // cancel insert if user don't want to take the whole text into quote
                    if (dialog.Result == 1)
                        return;
                }
                // load whole text into variable
                text = textBoxTextForQuote.Text;
            }
            // add a line break, so user can add text in the next line
            text += "\r";

            // close flyout and insert text into richeditbox
            flyoutQuoteBox.Hide();
            rebNewPostText.Document.Selection.SetText(TextSetOptions.None, text);
            // format inserted text as blockquote
            ITextRange tr = rebNewPostText.Document.Selection;
            tr.CharacterFormat.BackgroundColor = QuoteColor;
            tr.CharacterFormat.Italic = FormatEffect.On;

            // focus richeditbox, scroll to inserted text and set cursor to the first position after quote
            tr.ScrollIntoView(PointOptions.Start);
            rebNewPostText.Document.Selection.SetRange(tr.EndPosition, tr.EndPosition);
            rebNewPostText.Focus(FocusState.Programmatic);
        }


        private void buttonCancelInsertQuote_Click(object sender, RoutedEventArgs e)
        {
            flyoutQuoteBox.Hide();
        }

    }
}
