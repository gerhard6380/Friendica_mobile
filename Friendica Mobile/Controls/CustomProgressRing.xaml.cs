using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    public partial class CustomProgressRing : ContentView
    {
        // BindableProperty for the CustomProgressRing to hold the text to be shown next to ProgressRing
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", 
                                                                               typeof(string), typeof(CustomProgressRing), 
                                                                               "", BindingMode.OneWay, 
                                                                               propertyChanged: (bindable, value, newValue) =>
        {
            (bindable as CustomProgressRing).LabelCustomProgressRing.Text = (string)newValue;
        });

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public CustomProgressRing()
        {
            InitializeComponent();
        }

    }
}
