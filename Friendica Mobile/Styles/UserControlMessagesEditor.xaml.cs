using Friendica_Mobile.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Friendica_Mobile.Styles
{
    public sealed partial class UserControlMessagesEditor : UserControl
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        // color definitions
        Color QuoteColor = Colors.Gold;
        List<string> colors = new List<string>(new string[] { "000000", "808080", "C0C0C0", "FFFFFF",
                                                                  "800080", "000080", "0000FF", "00FFFF",
                                                                  "800000", "FF0000", "FFA500", "FFFF00",
                                                                  "008000", "808000", "00FF00", "FF00FF" });

        public UserControlMessagesEditor()
        {
            this.InitializeComponent();
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
                //buttonList.IsEnabled = true;
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

                //if (tr.ParagraphFormat.ListType == MarkerType.Bullet)
                //    buttonList.IsChecked = true;
                //else
                //    buttonList.IsChecked = false;

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
                //buttonList.IsEnabled = false;
                buttonLink.IsEnabled = false;
            }

        }


        private string PrepareBBCode()
        {
            // Takes a RichEditBox control and returns a simple BBCode-formatted version of its contents
            string text, bbCode = "", strColour, strBold, strUnderline, strItalic, strBackground, strFont, strListType, strLink;
            bool colorActive = false, boldActive = false, italicActive = false, underlineActive = false, quoteActive = false, codeActive = false;
            bool listActive = false, linkActive = false;
            int lngOriginalStart, lngOriginalLength;
            int paragraphIndex, intCount, length = 0;
            ITextRange tr = rebNewMessageText.Document.Selection;

            rebNewMessageText.Document.GetText(TextGetOptions.None, out text);
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


        private void rebNewMessageText_SelectionChanged(object sender, RoutedEventArgs e)
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


        private void rebNewMessageText_GotFocus(object sender, RoutedEventArgs e)
        {
            // enable editor buttons if user focuses the RichEditBox
            var reb = sender as RichEditBox;
            ITextRange tr = reb.Document.Selection;
            SetButtonStates(tr, true);
            App.NavStatus = NavigationStatus.NewMessageChanged;
        }


        private void rebNewMessageText_LostFocus(object sender, RoutedEventArgs e)
        {
            App.NavStatus = NavigationStatus.OK;
            // loose focus of RichEditBox only if we are NOT moving focus to an editor button (otherwise user cannot click editor function)
            List<string> controls = new List<string>(new string[] { "buttonBold", "buttonItalic", "buttonUnderline",
                                            "buttonColor", "buttonCode", "buttonQuote", "buttonList", "buttonLink" });

            var focus = FocusManager.GetFocusedElement() as Control;
            if (focus == null || !controls.Contains(focus.Name))
            {
                SetButtonStates(null, false);

                // set BBcode in viewmodel only after loosing focus on RichEditBox (doing this on each change would need too much CPU power)
                var bbCode = PrepareBBCode();
                var mvvm = this.DataContext as MessagesViewmodel;
                mvvm.NewMessageContent = bbCode;

                // relocate focus from editor button to send button
                if (focus != null && focus.FocusState == FocusState.Keyboard)
                        buttonMessagesSend.Focus(FocusState.Keyboard);
            }
        }


        private void buttonBold_Click(object sender, RoutedEventArgs e)
        {
            ITextRange tr = rebNewMessageText.Document.Selection;
            tr.CharacterFormat.Bold = FormatEffect.Toggle;
            rebNewMessageText.Focus(FocusState.Programmatic);
        }


        private void buttonItalic_Click(object sender, RoutedEventArgs e)
        {
            ITextRange tr = rebNewMessageText.Document.Selection;
            if (tr.CharacterFormat.BackgroundColor != QuoteColor)
                tr.CharacterFormat.Italic = FormatEffect.Toggle;
            rebNewMessageText.Focus(FocusState.Programmatic);
        }


        private void buttonUnderline_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            ITextRange tr = rebNewMessageText.Document.Selection;
            if ((bool)button.IsChecked)
                tr.CharacterFormat.Underline = UnderlineType.Single;
            else
                tr.CharacterFormat.Underline = UnderlineType.None;
            rebNewMessageText.Focus(FocusState.Programmatic);
        }


        private void Button_ColorSelector_Click(object sender, RoutedEventArgs e)
        {
            // take selected color from flyout (all 16 colors jump to this eventhandler), set selection with this color and 
            // change color button to this color
            var button = sender as Button;
            if (buttonColor.Flyout != null)
                buttonColor.Flyout.Hide();
            var color = button.Background as SolidColorBrush;
            ITextRange tr = rebNewMessageText.Document.Selection;
            tr.CharacterFormat.ForegroundColor = color.Color;
            var fonticon = buttonColor.Content as FontIcon;
            fonticon.Foreground = new SolidColorBrush(tr.CharacterFormat.ForegroundColor);
            rebNewMessageText.Document.ApplyDisplayUpdates();
            rebNewMessageText.Focus(FocusState.Programmatic);
        }


        private void buttonCode_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            ITextRange tr = rebNewMessageText.Document.Selection;
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
            rebNewMessageText.Focus(FocusState.Programmatic);
        }


        private void buttonQuote_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            ITextRange tr = rebNewMessageText.Document.Selection;
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
            rebNewMessageText.Focus(FocusState.Programmatic);
        }


        private async void buttonLink_Click(object sender, RoutedEventArgs e)
        {
            ITextRange tr = rebNewMessageText.Document.Selection;
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
            ITextRange tr = rebNewMessageText.Document.Selection;
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
            rebNewMessageText.Focus(FocusState.Programmatic);
        }


        private void buttonRemoveLink_Click(object sender, RoutedEventArgs e)
        {
            if (buttonLink.Flyout != null)
                buttonLink.Flyout.Hide();

            ITextRange tr = rebNewMessageText.Document.Selection;
            tr.Link = "";
            tr.CharacterFormat.ForegroundColor = Colors.Black;
            tr.CharacterFormat.Underline = UnderlineType.None;
            SetButtonStates(tr, true);
            rebNewMessageText.Focus(FocusState.Programmatic);
        }

        private void rebNewMessageText_TextCompositionEnded(RichEditBox sender, TextCompositionEndedEventArgs args)
        {
            buttonMessagesSend.Focus(FocusState.Keyboard);
        }
    }
}
