using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    public partial class CustomRadioButtonColor : ContentView
    {
        public static readonly string FIRadioButtonChecked = "\uECCB";
        public static readonly string FIRadioButtonUnchecked = "\uECCC";

        public event EventHandler RadioButtonClicked;

        // prepare Bindable Property for "IsChecked" 
        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create("IsChecked",
                                                            typeof(bool), typeof(CustomRadioButtonColor),
                                                            false, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                            });

		// prepare Bindable Property for "ItemName" 
        public static readonly BindableProperty ItemNameProperty = BindableProperty.Create("ItemName",
                                                            typeof(string), typeof(CustomRadioButtonColor),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                            });

        // prepare Bindable Property for "Text" 
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text",
                                                            typeof(string), typeof(CustomRadioButtonColor),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                var button = bindable as CustomRadioButtonColor;
                                                                (bindable as CustomRadioButtonColor).LabelRadioButton.Text = (string)newValue;
                                                                button.LabelRadioButton.IsVisible = (button.LabelRadioButton.Text != "");
                                                            });

        // prepare Bindable Property for "TextColor"
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create("TextColor",
                                                            typeof(Color), typeof(CustomRadioButtonColor),
                                                            Color.Accent, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                var button = bindable as CustomRadioButtonColor;
                                                                button.FontIconCheckedRadioButton.TextColor = (Color)newValue;
                                                                button.FontIconUncheckedRadioButton.TextColor = (Color)newValue;
                                                            });

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        // holding a language neutral reference name for diverse purposes
        public string ItemName
        {
            get { return (string)GetValue(ItemNameProperty); }
            set { SetValue(ItemNameProperty, value); }
        }

        // translated string for displaying to the user
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // color for the button
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }


        public CustomRadioButtonColor()
        {
            InitializeComponent();
        }


        void ButtonRadioButton_Clicked(object sender, System.EventArgs e)
        {    
            // fire event to inform RadioButtonGroup that we need to go through the settings for all options
            RadioButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
