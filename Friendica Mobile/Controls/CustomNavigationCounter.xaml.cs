using System;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
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
        }

    }
}
