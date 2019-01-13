using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Friendica_Mobile.Styles
{
    public partial class BaseContentPage : ContentPage
    {
        // property holding the content of the derived page
        public View PageContent
        {
            get { return (View)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        // property holding the commandbar items of the derived page
        public ObservableCollection<CustomCommandBarItem> CommandBarCommandList { get; set; }

        // derived pages will show their content in this View element
        public static readonly BindableProperty PageContentProperty = BindableProperty.Create("PageContent",
            typeof(View), typeof(BaseContentPage), null, propertyChanged: (bindable, value, newValue) =>
            {
                var control = (BaseContentPage)bindable;
                control.PageContentContainer.Content = control.PageContent;
            });


        public BaseContentPage()
        {
            InitializeComponent();
            // set position of the commandbar
            SetCommandBar();

            // initialize list of command bar items and refresh the CommandList in the CustomCommandBar item when items are added
            CommandBarCommandList = new ObservableCollection<CustomCommandBarItem>();
            CommandBarCommandList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => 
            {
                var list = new ObservableCollection<CustomCommandBarItem>();
                foreach (var item in CommandBarCommandList)
                    list.Add(item);
                CommandBar.CommandList = list;  
            };
        }


        void Handle_OnCommandBarPositionChanged(object sender, System.EventArgs e)
        {
            SetCommandBar();
        }

        private void SetCommandBar()
        {
            // set the margins of the detail area and the command bar to the wished position
            if (CommandBar.IsVisible)
            {
                if (CommandBar.CommandBarPosition == CustomCommandBar.BarPositions.Top)
                {
                    GridPageContent.Margin = new Thickness(0, 48, 0, 0);
                    CommandBar.VerticalOptions = LayoutOptions.Start;
                    GridFlyout.VerticalOptions = LayoutOptions.Start;
                    GridFlyout.TranslationY = 48;
                }
                else
                {
                    GridPageContent.Margin = new Thickness(0, 0, 0, 48);
                    CommandBar.VerticalOptions = LayoutOptions.End;
                    GridFlyout.VerticalOptions = LayoutOptions.End;
                    GridFlyout.TranslationY = -48;
                }
            }
            else
            {
                GridPageContent.Margin = new Thickness(0, 0, 0, 0);
            }
        }


        void Handle_MoreOptionsButtonClicked(object sender, System.EventArgs e)
        {
            // increase the size of the commandbar if 3-dots are clicked
            if (CommandBar.Height > 48)
                CommandBar.HeightRequest = 48;
            else
                CommandBar.HeightRequest = 72;

            // show flyout only if there are elements to be shown
            var items = CommandBar.GetSecondaryElements();
            if (items.Count > 0)
            {
                // fade in flyout with other opitons
                SetGridFlyoutBaseTap();
                GridFlyout.Opacity = 0;
                GridFlyoutBase.IsVisible = true;
                GridFlyout.FadeTo(1, 500);

                StackLayoutSecondaryCommandsContainer.Children.Clear();
                foreach (var item in items)
                    StackLayoutSecondaryCommandsContainer.Children.Add(item);
            }
        }

        private void SetGridFlyoutBaseTap()
        {
            // when user taps anything around the buttons we want to close the flyout
            TapGestureRecognizer gridTap = new TapGestureRecognizer();
            gridTap.Tapped += async (object sender, EventArgs e) => 
            {
                // fade out slowly before making the item invisible (necessary to avoid further taps)
                await GridFlyout.FadeTo(0, 500);
                GridFlyoutBase.IsVisible = false;
                CommandBar.HeightRequest = 48;
            };
            GridFlyoutBase.GestureRecognizers.Add(gridTap);
        }

    }
}
