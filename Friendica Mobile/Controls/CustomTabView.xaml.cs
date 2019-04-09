using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Friendica_Mobile;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    public partial class CustomTabView : ContentView
    {
        // prepare Bindable Property for the tabview items 
        public static readonly BindableProperty TabViewItemsProperty = BindableProperty.Create("TabViewItems",
                                                            typeof(ObservableCollection<CustomTabViewItem>), typeof(CustomTabView),
                                                                                               null, BindingMode.OneWay);

        public ObservableCollection<CustomTabViewItem> TabViewItems
        {
            get { return (ObservableCollection<CustomTabViewItem>)GetValue(TabViewItemsProperty); }
            set { SetValue(TabViewItemsProperty, value); }
        }


        public CustomTabView()
        {
            InitializeComponent();

            // initialize the collection of items and listen to changes
            TabViewItems = new ObservableCollection<CustomTabViewItem>();
            TabViewItems.CollectionChanged += TabViewItems_CollectionChanged;
        }


        void TabViewItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // rebuild the collection each time the item list is changed
            var list = sender as ObservableCollection<CustomTabViewItem>;
            if (StackLayoutTabContainer.Children.Count > 0)
                this.StackLayoutTabContainer.Children.Clear();

            foreach (var item in list)
            {
                // remove existing click event handlers on the items before adding to avoid multiple event firings
                item.TabHeaderClicked -= Item_TabHeaderClicked;
                item.TabHeaderClicked += Item_TabHeaderClicked;
                this.StackLayoutTabContainer.Children.Add(item);

                // set content for the active tab
                if (item.IsActiveTab)
                    SetContent(item);
            }
        }

        void Item_TabHeaderClicked(object sender, EventArgs e)
        {
            var clickedItem = sender as CustomTabViewItem;

            // move clicked item to center (only if screen space is too low to show all items)
            // this makes it easier for the user to navigate from item to item and back
            ScrollViewTabHeaders.ScrollToAsync(clickedItem, ScrollToPosition.Center, false);

            foreach (var item in TabViewItems)
            {
                // remove click event handler for all items to add it again for all non-active
                // tabs, click on the active tab header should not fire the click eventhander again
                item.TabHeaderClicked -= Item_TabHeaderClicked;
                if (item == clickedItem)
                {
                    item.IsActiveTab = true;
                    SetContent(item);
                }
                else
                {
                    item.IsActiveTab = false;
                    item.TabHeaderClicked += Item_TabHeaderClicked;
                }
            }
        }

        void SetContent(CustomTabViewItem item)
        {
            // load content of the tab in the visible area
            this.GridTabContent.Children.Clear();
            if (item.TabContent != null)
                this.GridTabContent.Children.Add(item.TabContent);
            App.DefineResources();
        }
    }
}
