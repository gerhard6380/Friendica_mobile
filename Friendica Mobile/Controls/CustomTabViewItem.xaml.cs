using System;
using System.Collections.Generic;
using Friendica_Mobile;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    public partial class CustomTabViewItem : ContentView
    {
        public event EventHandler TabHeaderClicked;

        // prepare Bindable Property for the tab header text 
        public static readonly BindableProperty HeaderProperty = BindableProperty.Create("Header",
                                                            typeof(string), typeof(CustomTabViewItem),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
        {
            (bindable as CustomTabViewItem).LabelHeader.Text = (string)newValue;
        });

        // prepare Bindable Property for the IsActiveTab bool 
        public static readonly BindableProperty IsActiveTabProperty = BindableProperty.Create("IsActiveTab",
                                                            typeof(bool), typeof(CustomTabViewItem),
                                                            false, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
        {
            (bindable as CustomTabViewItem).LabelHeader.FontAttributes = ((bool)newValue) ? FontAttributes.Bold : FontAttributes.None;
            (bindable as CustomTabViewItem).BoxViewIndicator.IsVisible = ((bool)newValue);
        });

        // prepare Bindable Property for the BadgeValue
        public static readonly BindableProperty CounterProperty = BindableProperty.Create("Counter",
                                                            typeof(int), typeof(CustomTabViewItem),
                                                            0, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
        {
            (bindable as CustomTabViewItem).CounterElement.Counter = (int)newValue;
        });

        // prepare Bindable Property for the Content of the tab
        public static readonly BindableProperty TabContentProperty = BindableProperty.Create("TabContent",
                                                            typeof(View), typeof(CustomTabViewItem),
                                                                                             null, BindingMode.OneWay);
        

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public int Counter
        {
            get { return (int)GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }
        public bool IsActiveTab
        {
            get { return (bool)GetValue(IsActiveTabProperty); }
            set { SetValue(IsActiveTabProperty, value); }
        }
        public View TabContent
        {
            get { return (View)GetValue(TabContentProperty); }
            set { SetValue(TabContentProperty, value); }
        }


        public CustomTabViewItem()
        {
            InitializeComponent();
            // react on turning phone to landscape (lower font size to reduce needed height)
            App.ShellSizeChanged += (sender, e) => { SetFontSize(); };
        }


        void ButtonTabViewItem_Clicked(object sender, System.EventArgs e)
        {
            // invoke changing content in tabview when user clicks the button
            TabHeaderClicked?.Invoke(this, EventArgs.Empty);
        }

        private void SetFontSize()
        {
            // landscape on phones should use less height space, so reduce the font size a bit
            if (App.DeviceType == Plugin.DeviceInfo.Abstractions.Idiom.Phone && App.ShellWidth > App.ShellHeight)
            {
                LabelHeader.FontSize = 18;
            }
            else
                LabelHeader.FontSize = 24;
        }
    }
}
