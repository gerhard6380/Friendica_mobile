using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NavigationMenu.Styles
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomNavigationButton : ContentView
    {
        // prepare Bindable Property for the Label showing the text to take the text
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text",
                                                            typeof(string), typeof(CustomNavigationButton),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: TextValueChanged);

        // prepare Bindable Property for the FontIconLabel showing the unicode text of the icon
        public static readonly BindableProperty FontIconProperty = BindableProperty.Create("FontIcon",
                                                            typeof(string), typeof(CustomNavigationButton),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: FontIconValueChanged);

        // prepare Bindable property for the backgroundcolor of the button
        public static readonly BindableProperty ColorBackgroundProperty = BindableProperty.Create("ColorBackground",
                                                            typeof(Color), typeof(CustomNavigationButton),
                                                            Application.Current.Resources["ButtonBackgroundColor"], BindingMode.OneWay,
                                                            propertyChanged: ColorBackgroundValueChanged);

        // prepare Bindable property for the textcolor of the button
        public static readonly BindableProperty ColorTextProperty = BindableProperty.Create("ColorText",
                                                            typeof(Color), typeof(CustomNavigationButton),
                                                            Application.Current.Resources["ButtonForegroundColor"], BindingMode.OneWay,
                                                            propertyChanged: ColorTextValueChanged);

        // properties to be used in XAML
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string FontIcon
        {
            get { return (string)GetValue(FontIconProperty); }
            set { SetValue(FontIconProperty, value); }
        }

        public Color ColorBackground
        {
            get { return (Color)GetValue(ColorBackgroundProperty); }
            set { SetValue(ColorBackgroundProperty, value); }
        }

        public Color ColorText
        {
            get { return (Color)GetValue(ColorTextProperty); }
            set { SetValue(ColorTextProperty, value); }
        }


        // change text in label when changed in Viewmodel
        private static void TextValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            string text;
            if ((string)newvalue == ".")
            {
                text = "";
                //(bindable as CustomNavigationButton).LabelButtonText.IsVisible = false;
            }
            else
                text = (string)newvalue;
            //(bindable as CustomNavigationButton).LabelButtonText.Text = text;
        }

        // change text in label when changed in Viewmodel
        private static void FontIconValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            switch (Device.RuntimePlatform)
            {
                case (Device.iOS):
                    //(bindable as CustomNavigationButton).LabelFontIcon.FontFamily = "Segoe MDL2 Assets";
                    break;
            }
            //(bindable as CustomNavigationButton).LabelFontIcon.Text = (string)newvalue;
        }

        // change background color
        private static void ColorBackgroundValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            //(bindable as CustomNavigationButton).StackButtonWithIcon.BackgroundColor = (Color)newvalue;
        }

        // change text color
        private static void ColorTextValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            //(bindable as CustomNavigationButton).LabelFontIcon.TextColor = (Color)newvalue;
            //(bindable as CustomNavigationButton).LabelButtonText.TextColor = (Color)newvalue;
        }

        // prepare Bindable Property for the CustomHyperlinkButton to take the command to use in Viewmodel when user taps the link
        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command",
                                                            typeof(ICommand), typeof(CustomNavigationButton), 
                                                            null, BindingMode.OneWay,
                                                            propertyChanged: CommandValueChanged);
        
        // property to be used in XAML to insert Command from Viewmodel
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // change Command on button
        private static void CommandValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            //if (newvalue != null)
                //(bindable as CustomNavigationButton).ButtonOverText.Command = (ICommand)newvalue;
        }


        public CustomNavigationButton()
        {
            InitializeComponent();
        }

        private void StackButtonWithIcon_SizeChanged(object sender, System.EventArgs e)
        {
            //ButtonOverText.WidthRequest = StackButtonWithIcon.Width;
        }
    }
}