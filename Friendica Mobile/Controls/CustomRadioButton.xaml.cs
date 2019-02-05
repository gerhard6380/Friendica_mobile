using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    public partial class CustomRadioButton : ContentView
    {
        public static readonly string FIRadioButtonChecked = "\uECCB";
        public static readonly string FIRadioButtonUnchecked = "\uECCA";

        public event EventHandler RadioButtonClicked;

        // prepare Bindable Property for "IsChecked" 
        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create("IsChecked",
                                                            typeof(bool), typeof(CustomRadioButton),
                                                            false, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                            });

		// prepare Bindable Property for "ItemName" 
        public static readonly BindableProperty ItemNameProperty = BindableProperty.Create("ItemName",
                                                            typeof(string), typeof(CustomRadioButton),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                            });

        // prepare Bindable Property for "Text" 
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text",
                                                            typeof(string), typeof(CustomRadioButton),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                (bindable as CustomRadioButton).LabelRadioButton.Text = (string)newValue;
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


        public CustomRadioButton()
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
