using Xamarin.Forms;

namespace Friendica_Mobile.Styles
{
    public partial class CustomCommandBarItem : ContentView
    {
        public enum CommandLevel { Primary, Secondary }

        // prepare Bindable Property for the icon symbol 
        public static readonly BindableProperty FontIconProperty = BindableProperty.Create("FontIcon",
                                                            typeof(string), typeof(CustomCommandBarItem),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: FontIconValueChanged);

        // prepare Bindable Property for the extended text 
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text",
                                                            typeof(string), typeof(CustomCommandBarItem),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: TextValueChanged);

        // prepare Bindable Property for the extended text 
        public new static readonly BindableProperty IsEnabledProperty = BindableProperty.Create("IsEnabled",
                                                            typeof(bool), typeof(CustomCommandBarItem),
                                                            true, BindingMode.OneWay,
                                                            propertyChanged: IsEnabledValueChanged);

        // prepare Bindable Property for the extended text 
        public static readonly BindableProperty IsHorizontalProperty = BindableProperty.Create("IsHorizontal",
                                                            typeof(bool), typeof(CustomCommandBarItem),
                                                            false, BindingMode.OneWay,
                                                            propertyChanged: IsHorizontalValueChanged);
        
        // prepare Bindable Property for the level of commmand (primary or secondary) 
        public static readonly BindableProperty LevelProperty = BindableProperty.Create("Level",
                                                            typeof(CommandLevel), typeof(CustomCommandBarItem),
                                                            CommandLevel.Primary, BindingMode.OneWay);

        // prepare Bindable Property for the level of commmand (primary or secondary) 
        public static readonly BindableProperty OrderProperty = BindableProperty.Create("Priority",
                                                            typeof(int), typeof(CustomCommandBarItem),
                                                            0, BindingMode.OneWay);

        // prepare Bindable Property for the BadgeValue
        public static readonly BindableProperty CounterProperty = BindableProperty.Create("Counter",
                                                            typeof(int), typeof(CustomCommandBarItem),
                                                            0, BindingMode.OneWay,
                                                            propertyChanged: CounterValueChanged);

        // prepare Bindable Property for the Command
        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command",
                                                            typeof(Command), typeof(CustomCommandBarItem),
                                                            null, BindingMode.OneWay,
                                                            propertyChanged: CommandValueChanged);


        public string FontIcon
        {
            get { return (string)GetValue(FontIconProperty); }
            set { SetValue(FontIconProperty, value); }
        }
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public new bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }
        public CommandLevel Level
        {
            get { return (CommandLevel)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }
        public int Priority
        {
            get { return (int)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }
        public int Counter
        {
            get { return (int)GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }
        public Command Command
        {
            get { return (Command)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public bool IsHorizontal
        {
            get { return (bool)GetValue(IsHorizontalProperty); }
            set { SetValue(IsHorizontalProperty, value); }
        }

        // change text of icon in label 
        private static void FontIconValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            switch (Device.RuntimePlatform)
            {
                case (Device.iOS):
                case (Device.macOS):

                    (bindable as CustomCommandBarItem).LabelFontIcon.FontFamily = "Segoe MDL2 Assets";
                    (bindable as CustomCommandBarItem).LabelFontIconHorizontal.FontFamily = "Segoe MDL2 Assets";
                    break;
            }
            (bindable as CustomCommandBarItem).LabelFontIcon.Text = (string)newvalue;
            (bindable as CustomCommandBarItem).LabelFontIconHorizontal.Text = (string)newvalue;
        }

        // change text of description 
        private static void TextValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as CustomCommandBarItem).LabelDescription.Text = (string)newvalue;
            (bindable as CustomCommandBarItem).LabelDescriptionHorizontal.Text = (string)newvalue;
        }

        // change enabled status of button 
        private static void IsEnabledValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as CustomCommandBarItem).SetTextColor();
            // don't set IsEnabled = false as this will lead to a different style on uwp, use InputTransparent = true 
            // instead, prevents user clicks but keeps the style
            (bindable as CustomCommandBarItem).ButtonCommandBarItem.InputTransparent = !(bool)newvalue;
            (bindable as CustomCommandBarItem).ButtonCommandBarItemHorizontal.InputTransparent = !(bool)newvalue;
        }

        // change orientation of button 
        private static void IsHorizontalValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as CustomCommandBarItem).StackLayoutVertical.IsVisible = !(bool)newvalue;
            (bindable as CustomCommandBarItem).StackLayoutHorizontal.IsVisible = (bool)newvalue;
        }

        // change text of counter when changed Counter property
        private static void CounterValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            (bindable as CustomCommandBarItem).CounterElement.Counter = (int)newvalue;
            (bindable as CustomCommandBarItem).CounterElementHorizontal.Counter = (int)newvalue;
        }

        // set Command to the button's Command when CommandProperty is changed
        private static void CommandValueChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (newvalue != null && newvalue.GetType() == typeof(Command))
            {
                (bindable as CustomCommandBarItem).ButtonCommandBarItem.Command = (Command)newvalue;
                (bindable as CustomCommandBarItem).ButtonCommandBarItemHorizontal.Command = (Command)newvalue;
            }
        }


        public CustomCommandBarItem()
        {
            InitializeComponent();
        }


        private void SetTextColor()
        {
            // text color is normally set by resource file, but probably we want to disable a button programmatically, then text shall be gray
            if (!IsEnabled)
            {
                LabelFontIcon.TextColor = LabelDescription.TextColor = Color.Gray;
                LabelFontIconHorizontal.TextColor = LabelDescriptionHorizontal.TextColor = Color.Gray;
            }
            else
            {
                LabelFontIcon.SetDynamicResource(Label.TextColorProperty, "NavigationTextColor");
                LabelDescription.SetDynamicResource(Label.TextColorProperty, "NavigationTextColor");
                LabelFontIconHorizontal.SetDynamicResource(Label.TextColorProperty, "NavigationTextColor");
                LabelDescriptionHorizontal.SetDynamicResource(Label.TextColorProperty, "NavigationTextColor");
            }
        }


        void Handle_SizeChanged(object sender, System.EventArgs e)
        {
            // change style if bar is higher than 48, then we want to show the descriptions
            var view = sender as ContentView;
            LabelDescription.IsVisible = (view.Height > 48);
            LabelFontIcon.VerticalTextAlignment = (view.Height > 48) ? TextAlignment.Start : TextAlignment.Center;
            LabelFontIcon.Margin = (view.Height > 48) ? new Thickness(0, 4, 0, 0) : new Thickness(0, 0, 0, 0);
        }

        void Label_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetTextColor();
        }
    }
}
