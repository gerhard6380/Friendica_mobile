using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Friendica_Mobile.Styles
{
    public partial class CustomNavigationCounter : ContentView
    {
        // prepare Bindable Property for the Label showing the text to take the text
        public static readonly BindableProperty CounterProperty = BindableProperty.Create("Counter",
                                                            typeof(int), typeof(CustomNavigationCounter),
                                                            0, BindingMode.OneWay,
                                                            propertyChanged: CounterValueChanged);


        // properties to be used in XAML
        public int Counter
        {
            get { return (int)GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }

        // change text in label when changed in Viewmodel
        private static void CounterValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as CustomNavigationCounter).FrameCounter.IsVisible = ((int)newvalue > 0);
            (bindable as CustomNavigationCounter).LabelCounter.Text = Convert.ToString((int)newvalue);
        }


        public CustomNavigationCounter()
        {
            InitializeComponent();
            Settings.NavigationSideChanged += (sender, e) => { SetNavigationSide(); };
            App.ShellSizeChanged += (sender, e) => { SetNavigationSide(); };
            SetNavigationSide();
        }

        private void SetNavigationSide()
        {
            if (App.DeviceType == Xamarin.Essentials.DeviceIdiom.Phone)
            {
                // TODO: hochformat
                if (Settings.NavigationOnRightSide && App.ShellWidth < App.ShellHeight)
                {
                    FrameCounter.HorizontalOptions = LayoutOptions.End;
                    FrameCounter.Margin = new Thickness(0, 0, 24, 0);
                }
                else
                {
                    FrameCounter.HorizontalOptions = LayoutOptions.Start;
                    FrameCounter.Margin = new Thickness(24, 0, 0, 0);
                }
            }
        }
    }
}
