using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Friendica_Mobile.UWP.Styles
{
    public sealed partial class UserControlProfileDatePicker : UserControl
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public enum FriendicaProfileDatePickerFields { ProfileDateOfBirth, ProfileMaritalSince };
        
        new public event EventHandler SizeChanged;

        // property for datatype parameter
        private FriendicaProfileDatePickerFields _field;
        public FriendicaProfileDatePickerFields Field
        {
            get { return _field; }
            set
            {
                _field = value;
                SetHeader(value);
            }
        }

        public double ElementWidth
        {
            get
            {
                return (double)GetValue(ElementWidthProperty);
            }
            set
            {
                SetValue(ElementWidthProperty, (double)value);
            }
        }
        public static DependencyProperty ElementWidthProperty = DependencyProperty.Register("ElementWidth", typeof(double),
            typeof(UserControlProfileDatePicker), null);


        public DateTimeOffset ElementDate
        {
            get
            {
                return (DateTimeOffset)GetValue(ElementDateProperty);
            }
            set
            {
                SetValue(ElementDateProperty, value);
            }
        }
        public static DependencyProperty ElementDateProperty = DependencyProperty.Register("ElementDate", typeof(DateTimeOffset), 
            typeof(UserControlProfileDatePicker), new PropertyMetadata(0.0, new PropertyChangedCallback(OnElementDateChanged)));

        private static void OnElementDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UserControlProfileDatePicker)d;
            if (e.NewValue.GetType() != typeof(double))
                SetVisibility(control, (DateTimeOffset)e.NewValue);
        }

        private static void SetVisibility(UserControlProfileDatePicker control, DateTimeOffset value)
        {
            control.toggleProfileDateDefined.Visibility = (control.toggleProfileDateDefined.IsEnabled) ? Visibility.Visible : Visibility.Collapsed;
            
            if (value == default(DateTimeOffset))
            {
                control.DateDefined = false;
                control.DateHasYear = false;
                control.textblockProfilesFieldHeader.Opacity = 0.5;
            }
            else
            {
                control.DateDefined = true;
                control.DateHasYear = value.Year != 1900;
                control.textblockProfilesFieldHeader.Opacity = 1.0;
            }
        }

        public bool DateDefined
        {
            get
            {
                return (bool)GetValue(DateDefinedProperty);
            }
            set
            {
                SetValue(DateDefinedProperty, (bool)value);
            }
        }
        public static DependencyProperty DateDefinedProperty = DependencyProperty.Register("DateDefined", typeof(bool),
            typeof(UserControlProfileDatePicker), null);

        public bool DateHasYear
        {
            get
            {
                return (bool)GetValue(DateHasYearProperty);
            }
            set
            {
                SetValue(DateHasYearProperty, (bool)value);
            }
        }
        public static DependencyProperty DateHasYearProperty = DependencyProperty.Register("DateHasYear", typeof(bool),
            typeof(UserControlProfileDatePicker), null);


        public UserControlProfileDatePicker()
        {
            this.InitializeComponent();
            datepickerProfilesField.MinYear = new DateTimeOffset(1900,1,1,0,0,0,TimeSpan.Zero);
            datepickerProfilesField.MaxYear = new DateTimeOffset(DateTime.Today);
            calendarProfilesField.MinDate = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
            calendarProfilesField.MaxDate = new DateTimeOffset(DateTime.Today);
        }


        private void SetHeader(FriendicaProfileDatePickerFields field)
        {
            switch (field)
            {
                case FriendicaProfileDatePickerFields.ProfileDateOfBirth:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesDateOfBirth");
                    break;
                case FriendicaProfileDatePickerFields.ProfileMaritalSince:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesMaritalSince");
                    break;
                default:
                    break;
            }
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender != null)
            {
                object element = sender as StackPanel;
                do
                {
                    element = VisualTreeHelper.GetParent((DependencyObject)element);
                }
                while (element.GetType() != typeof(clsVariableGridView));

                SizeChanged?.Invoke(element, EventArgs.Empty);
            }
        }

        private void toggleProfileWithoutYear_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            // if switch is on and year not 1900 set year to 1900
            if (!toggle.IsOn && DateDefined && ElementDate.Year != 1900)
                ElementDate = new DateTimeOffset(1900, ElementDate.Month, ElementDate.Day, 0, 0, 0, TimeSpan.Zero);
        }

        private void toggleProfileDateDefined_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            if (!toggle.IsOn)
                ElementDate = default(DateTimeOffset);
        }

        private void toggleProfileDateDefined_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender != null)
            {
                SetVisibility(this, this.ElementDate);

                object element = sender as ToggleSwitch;
                do
                {
                    element = VisualTreeHelper.GetParent((DependencyObject)element);
                }
                while (element.GetType() != typeof(clsVariableGridView));

                SizeChanged?.Invoke(element, EventArgs.Empty);
            }

        }

    }
}
