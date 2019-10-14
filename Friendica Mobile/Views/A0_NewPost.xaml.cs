using System.Text;
using System.Threading.Tasks;
using Friendica_Mobile.Models;
using Friendica_Mobile.Strings;
using Friendica_Mobile.Styles;
using Friendica_Mobile.ViewModel;
using SeeberXamarin.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Friendica_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewPost : BaseContentPage
    {
        // indicator the save the current view setup (on smartphones only)
        private int CurrentMode = 1; // 1 = 66/33%, 2 = bottom small, 4 = top small

        /// <summary>
        /// constructor used to transfer parameters when calling the class in navigation
        /// </summary>
        /// <param name="thread">if available but IsRetweetPost=false we do have a comment</param>
        /// <param name="post">used if IsRetweetedPost = true to identify the post to be retweeted </param>
        /// <param name="IsRetweetedPost">true if user wants to retweet a certain post</param>
        public NewPost(FriendicaThread thread = null, FriendicaPost post = null, bool IsRetweetedPost = false)
        {
            InitializeComponent();

            // set view setup depending on available space
            App.ShellSizeChanged += App_ShellSizeChanged;
            SetView();

            // react on transferred parameters through navigation
            if (IsRetweetedPost && post != null)
            {
                // user wants to retweet a post
                var mvvm = this.BindingContext as NewPostViewModel;
                mvvm.IsRetweeting = true;
                mvvm.RetweetPost = post;
                mvvm.ShowACLSettings = true;
                mvvm.ShowCurrentThread = true;
                mvvm.CurrentThread.Add(thread);
            }
            else if (thread != null)
            {
                // this is a normal comment, because we have received a thread
                // so let's display this thread and hide ACL settings (comments have the ACL rights of the in-reply-to post)
                var mvvm = this.BindingContext as NewPostViewModel;

                if (post != null)
                    mvvm.NewPost.InReplyToStatusId = post.Post.PostId;

                mvvm.ShowACLSettings = false;
                mvvm.ShowCurrentThread = true;
                mvvm.CurrentThread.Add(thread);
            }
            else
            {
                // we haven't received anything, so this will be a new post starting a new thread
                var mvvm = this.BindingContext as NewPostViewModel;
                mvvm.ShowACLSettings = true;
                mvvm.ShowCurrentThread = false;
                mvvm.NewPost.InReplyToStatusId = 0;
            }

            // create javscript editor
            var webViewSource = new HtmlWebViewSource();
            webViewSource.Html = WebViewEditorHtml.CreateEditorHtml();
            NewPostEditor.Source = webViewSource;
        }


        void App_ShellSizeChanged(object sender, System.EventArgs e)
        {
            SetView();
        }

        
        async void CommandBarNewPostSend_Clicked(object sender, System.EventArgs e)
        {
            var mvvm = this.BindingContext as NewPostViewModel;
            string html = await NewPostEditor.EvaluateJavaScriptAsync($"getHtml()");
            // the following will clean the html text and set it for posting
            mvvm.SetHtmlStatus(html);
            mvvm.SendNewPostAsync();
        }


        // user can change size of areas on smartphones (in portrait mode only) to use more space for the editor or the ACL section
        private void ButtonChangeAreas_Clicked(object sender, System.EventArgs e)
        {
            if (CurrentMode == 2)
            {
                // minimize editor to have more space for ACL or history 
                ButtonChangeAreas.FontIcon = Styles.Icon.FISplitScreen;
                ButtonChangeAreas.RotateTo(0);
                CurrentMode = 4;
                GridRow1.Height = 48;
                GridRow2.Height = GridLength.Auto;
            }
            else if (CurrentMode == 4)
            {
                // switch back to original sizes (3/5 for editor, 2/5 for ACL/history area)
                ButtonChangeAreas.FontIcon = Styles.Icon.FIMaximizeTop;
                ButtonChangeAreas.RotateTo(90);
                CurrentMode = 1;
                GridRow1.Height = new GridLength(0.67, GridUnitType.Star);
                GridRow2.Height = new GridLength(0.33, GridUnitType.Star);
            }
            else
            {
                // minimize ACL/history are to give more space to the editor itself
                ButtonChangeAreas.FontIcon = Styles.Icon.FIMaximizeBottom;
                ButtonChangeAreas.RotateTo(90);
                CurrentMode = 2;
                GridRow1.Height = GridLength.Auto;
                GridRow2.Height = 48;
            }
        }


        private void ButtonInsertLink_Clicked(object sender, System.EventArgs e)
        {
            // show prompt to enter the link (only on UWP as Javascript prompts are not displayed)
            EntryLink.Text = "https://";
            PromptInsertLink.IsVisible = true;
            EntryLink.Focus();
        }


        private async void ButtonCloseLinkPrompt_Clicked(object sender, System.EventArgs e)
        {
            await InsertLinkAsync();
        }


        private void ButtonCloseLinkPromptCancel_Clicked(object sender, System.EventArgs e)
        {
            // remove text and close prompt
            EntryLink.Text = "";
            PromptInsertLink.IsVisible = false;
        }


        private void PromptInsertLinkTap_Tapped(object sender, System.EventArgs e)
        {
            // remove text and close prompt
            EntryLink.Text = "";
            PromptInsertLink.IsVisible = false;
        }


        private async void EntryLink_Completed(object sender, System.EventArgs e)
        {
            await InsertLinkAsync();
        }

        
        void ImageNewPost_Tapped(object sender, System.EventArgs e)
        {
            // open image in a new window as within the current NavigationPage we cannot get rid of the title bar
            // pass over the source data into the new window
            var imageClicked = sender as CustomImage;
            var fullscreen = new Views.ImageFullscreen(imageClicked.Source);
            var nav = Application.Current.MainPage as NavigationPage;

            // set title bar to black is looking better for fullscreen displaying
            nav.BarBackgroundColor = Color.Black;
            nav.Popped += (sender2, e2) =>
            {
                // go back to correct title bar color after returning from fullscreen
                nav.BarBackgroundColor = (Color)Application.Current.Resources["AccentColor"];
            };
            Application.Current.MainPage.Navigation.PushAsync(fullscreen);
        }


        void ListViewContacts_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            // change selection even if text is clicked/tapped by user
            if (e.SelectedItem == null || sender == null)
                return;

            var item = e.SelectedItem as SelectableData<FriendicaUser>;
            item.IsSelected = !item.IsSelected;
            // now we can remove the selection again to enable clicking again to deselect
            var list = sender as ListView;
            list.SelectedItem = null;
        }


        void ListViewGroups_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            // change selection even if text is clicked/tapped by user
            if (e.SelectedItem == null || sender == null)
                return;

            var item = e.SelectedItem as SelectableData<FriendicaGroup>;
            item.IsSelected = !item.IsSelected;
            // now we can remove the selection again to enable clicking again to deselect
            var list = sender as ListView;
            list.SelectedItem = null;
        }


        private void SetView()
        {
            if (App.ShellWidth < 600)
            {
                // don't show the button on phone in landscape orientation 
                if (Device.Idiom == TargetIdiom.Phone)
                    ButtonChangeAreas.IsVisible = true;

                // too small for side by side, stack the areas vertically
                Grid.SetRowSpan(ScrollViewEditor, 1);
                Grid.SetColumnSpan(ScrollViewEditor, 2);
                Grid.SetRow(ACLConversationLayout, 1);
                Grid.SetRowSpan(ACLConversationLayout, 1);
                Grid.SetColumn(ACLConversationLayout, 0);
                Grid.SetColumnSpan(ACLConversationLayout, 2);
            }
            else
            {
                // don't show the button on phone in landscape orientation 
                ButtonChangeAreas.IsVisible = false;

                Grid.SetRowSpan(ScrollViewEditor, 2);
                Grid.SetColumnSpan(ScrollViewEditor, 1);
                Grid.SetRow(ACLConversationLayout, 0);
                Grid.SetRowSpan(ACLConversationLayout, 2);
                Grid.SetColumn(ACLConversationLayout, 1);
                Grid.SetColumnSpan(ACLConversationLayout, 1);
            }
        }


        private async Task InsertLinkAsync()
        {
            // abort if there is no link to insert
            if (EntryLink.Text == "" || EntryLink.Text == "https://" || EntryLink.Text == "http://")
                return;
            // insert link (user needs to select text before which will then get the link inserted, hint in prompt)
            await NewPostEditor.EvaluateJavaScriptAsync($"setLinkUWP('" + EntryLink.Text + "')");
            EntryLink.Text = "";
            PromptInsertLink.IsVisible = false;
        }
    }

}
