using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    public partial class IconButton : ContentView
    {
        public event EventHandler IconButtonClicked;

        // prepare Bindable Property for FontIcon 
        public static readonly BindableProperty FontIconProperty = BindableProperty.Create("FontIcon",
                                                            typeof(string), typeof(IconButton),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                (bindable as IconButton).LabelIconButton.Text = (string)newValue;
                                                                (bindable as IconButton).SetTextColor();
                                                            });

        // prepare Bindable Property for Caption
        public static readonly BindableProperty CaptionProperty = BindableProperty.Create("Caption",
                                                            typeof(string), typeof(IconButton),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                var text = (string)newValue;
                                                                if (!string.IsNullOrEmpty(text))
                                                                {
                                                                    (bindable as IconButton).LabelTextButton.Text = (string)newValue;
                                                                    (bindable as IconButton).LabelTextButton.IsVisible = true;
                                                                }
                                                                else
                                                                    (bindable as IconButton).LabelTextButton.IsVisible = false;
                                                            });

        // prepare Bindable Property for IsEnabled 
        public new static readonly BindableProperty IsEnabledProperty = BindableProperty.Create("IsEnabled",
                                                            typeof(bool), typeof(IconButton),
                                                            true, BindingMode.OneWay,
                                                            propertyChanged: IsEnabledValueChanged);

        // prepare Bindable Property for Tooltip - to be shown on UWP and macOS when user place mouse cursor over button (only when enabled)
        public static readonly BindableProperty TooltipProperty = BindableProperty.Create("Tooltip",
                                                            typeof(string), typeof(IconButton),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                (bindable as IconButton).ButtonIconButton.Tooltip = (string)newValue;
                                                            });

        // prepare Bindable Property for Command
        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command",
                                                            typeof(Command), typeof(IconButton),
                                                            null, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
        {
            if (newValue != null && newValue.GetType() == typeof(Command))
            {
                (bindable as IconButton).ButtonIconButton.Command = (Command)newValue;
            }
        });

        // prepare Bindable Property for BackgroundColor (decide correct background on using the control, sometimes we want to have it a transparent background)
        public new static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create("BackgroundColor",
                                                            typeof(Color), typeof(IconButton),
                                                            Color.Transparent, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                if (newValue != null && newValue is Color)
                                                                {
                                                                    (bindable as IconButton).GridIconButton.BackgroundColor = (Color)newValue;
                                                                }
                                                            });

        // prepare Bindable Property for FontIconColor (normally same as NavigationTextColor, but sometimes we need a different coloring
        public static readonly BindableProperty FontIconColorProperty = BindableProperty.Create("FontIconColor",
                                                            typeof(Color), typeof(IconButton),
                                                            Color.Transparent, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                (bindable as IconButton).SetTextColor();
                                                                //if (newValue != null && newValue is Color)
                                                                //{
                                                                //    (bindable as IconButton).LabelIconButton.TextColor = (Color)newValue;
                                                                //}
                                                            });


        public string FontIcon
        {
            get { return (string)GetValue(FontIconProperty); }
            set { SetValue(FontIconProperty, value); }
        }
        public new bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }
        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }
        public Command Command
        {
            get { return (Command)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public new Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        public Color FontIconColor
        {
            get { return (Color) GetValue(FontIconColorProperty); }
            set { SetValue(FontIconProperty, value); }
        }


        public IconButton()
        {
            InitializeComponent();
        }


        // change enabled status of button 
        private static void IsEnabledValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as IconButton).SetTextColor();
            (bindable as IconButton).ButtonIconButton.IsVisible = (bool)newvalue;

        }

        private void SetTextColor()
        {
            // text color is normally set by resource file, but probably we want to disable a button programmatically, then text shall be gray
            if (!IsEnabled)
                LabelIconButton.TextColor = Color.Gray;
            else
            {
                if (FontIconColor == Color.Transparent)
                    LabelIconButton.SetDynamicResource(Label.TextColorProperty, "ButtonTextColor");
                else
                    LabelIconButton.TextColor = FontIconColor;
            }
        }

        void IconButton_Clicked(object sender, System.EventArgs e)
        {
            // fire event if button is clicked, this will be used in A2_ImageFullscreen to switch back to main program page
                IconButtonClicked?.Invoke(this, EventArgs.Empty);    
        }
    }
}
