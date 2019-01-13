using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Friendica_Mobile.Styles
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomNavigationButton : ContentView
    {
        // used to identify the class name of the view related to the button - defines to highlight the active page button with accent color
        private string _className;
        public string ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        private bool _isRotating;
        public bool IsRotating
        {
            get { return _isRotating; }
            set
            {
                _isRotating = value;
                PerformRotating();
            }
        }

        private async void PerformRotating()
        {
            // when property is true, we start to rotate the gearing wheel by 45° each 1/4 second, we repeat this until property is false
            if (IsRotating)
            {
                var rotation = LabelFontIcon.Rotation + 45;
                await LabelFontIcon.RotateTo(rotation, 250);
                PerformRotating();
            }
        }
        
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

        // prepare Bindable Property for the IsActivePage indicating that this is the active part of the app
        public static readonly BindableProperty IsActivePageProperty = BindableProperty.Create("IsActivePage",
                                                            typeof(bool), typeof(CustomNavigationButton),
                                                            false, BindingMode.OneWay,
                                                            propertyChanged: IsActivePageValueChanged);

        // prepare Bindable Property for the IsEnabled
        public static readonly BindableProperty IsButtonEnabledProperty = BindableProperty.Create("IsButtonEnabled",
                                                            typeof(bool), typeof(CustomNavigationButton),
                                                            true, BindingMode.OneWay,
                                                            propertyChanged: IsButtonEnabledValueChanged);

        // prepare Bindable Property for the CounterNewElements
        public static readonly BindableProperty CounterNewElementsProperty = BindableProperty.Create("CounterNewElements",
                                                            typeof(int), typeof(CustomNavigationButton),
                                                            0, BindingMode.OneWay,
                                                            propertyChanged: CounterNewElementsValueChanged);

        // prepare Bindable Property for the Command
        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command",
                                                            typeof(Command), typeof(CustomNavigationButton), 
                                                            null, BindingMode.OneWay,
                                                            propertyChanged: CommandValueChanged);


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

        public bool IsActivePage
        {
            get { return (bool)GetValue(IsActivePageProperty); }
            set { SetValue(IsActivePageProperty, value); }
        }

        public bool IsButtonEnabled
        {
            get { return (bool)GetValue(IsButtonEnabledProperty); }
            set { SetValue(IsButtonEnabledProperty, value); }
        }

        public int CounterNewElements
        {
            get { return (int)GetValue(CounterNewElementsProperty); }
            set { SetValue(CounterNewElementsProperty, value); }
        }

        public Command Command
        {
            get { return (Command)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        // change text in label when changed in Viewmodel
        private static void TextValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            string text;
            if ((string)newvalue == ".")
            {
                text = "";
                (bindable as CustomNavigationButton).LabelButtonText.IsVisible = false;
            }
            else
                text = (string)newvalue;
            (bindable as CustomNavigationButton).LabelButtonText.Text = text;
        }

        // change text in label when changed in Viewmodel
        private static void FontIconValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            switch (Device.RuntimePlatform)
            {
                case (Device.iOS):
                case (Device.macOS):
                    
                    (bindable as CustomNavigationButton).LabelFontIcon.FontFamily = "Segoe MDL2 Assets";
                    break;
            }
            (bindable as CustomNavigationButton).LabelFontIcon.Text = (string)newvalue;
        }

        // set a semi-transparent background on the navigation button if this is the active page
        private static async void IsActivePageValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as CustomNavigationButton).GridActivePage.IsVisible = (bool)newvalue;
            if ((bool)newvalue)
            {
                var button = (bindable as CustomNavigationButton).GridButton;
                await button.ScaleTo(1.3, 500, Easing.CubicIn);
                await button.ScaleTo(1, 250, Easing.CubicInOut);
            }
        }

        // change text color in labels when changed IsEnabled property
        private static void IsButtonEnabledValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as CustomNavigationButton).SetTextColor();
			// IsVisible=false takes command away from Navigationbutton
            (bindable as CustomNavigationButton).ButtonCommand.IsVisible = (bool)newvalue;
        }

        // change text of counter when changed CounterNewElements property
        private static void CounterNewElementsValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as CustomNavigationButton).CounterElement.Counter = (int)newvalue;
        }


        // set Command to the button's Command when CommandProperty is changed
        private static void CommandValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (newvalue != null && newvalue.GetType() == typeof(Command))
                (bindable as CustomNavigationButton).ButtonCommand.Command = (Command)newvalue;
        }


        public CustomNavigationButton()
        {
            InitializeComponent();
            SetTextColor();
            Settings.NavigationSideChanged += (sender, e) => { SetNavigationSide(); };
            App.ShellSizeChanged += (sender, e) => { SetNavigationSide(); };
            SetNavigationSide();
        }





        private void SetTextColor()
        {
            // text color is set by resource file, but in case we have a page which disables navigating away without saving we want to
            // set the back button text color to gray
            if (!IsButtonEnabled)
                LabelFontIcon.TextColor = LabelButtonText.TextColor = Color.DimGray;
            else
            {
                LabelFontIcon.SetDynamicResource(Label.TextColorProperty, "NavigationTextColor");
            }
        }

        private void SetNavigationSide()
        {
            if (App.DeviceType == Plugin.DeviceInfo.Abstractions.Idiom.Phone)
            {
                if (Settings.NavigationOnRightSide && App.ShellWidth < App.ShellHeight)
                {
                    GridButton.ColumnDefinitions[0].Width = GridLength.Star;
                    GridButton.ColumnDefinitions[1].Width = 48;
                    LabelFontIcon.SetValue(Grid.ColumnProperty, 1);
                    LabelFontIcon.Margin = new Thickness(-12, 0, 0, 0);
                    LabelButtonText.SetValue(Grid.ColumnProperty, 0);
                    LabelButtonText.HorizontalOptions = LayoutOptions.EndAndExpand;
                    CounterElement.SetValue(Grid.ColumnProperty, 1);
                }
                else
                {
                    GridButton.ColumnDefinitions[0].Width = 48;
                    GridButton.ColumnDefinitions[1].Width = GridLength.Star;
                    LabelFontIcon.SetValue(Grid.ColumnProperty, 0);
                    LabelFontIcon.Margin = new Thickness(0, 0, 0, 0);
                    LabelButtonText.SetValue(Grid.ColumnProperty, 1);
                    LabelButtonText.HorizontalOptions = LayoutOptions.StartAndExpand;
                    CounterElement.SetValue(Grid.ColumnProperty, 0);
                }
            }
        }

    }
}
