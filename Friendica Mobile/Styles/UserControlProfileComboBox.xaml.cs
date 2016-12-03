using Friendica_Mobile.Converters;
using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Friendica_Mobile.Styles
{
    public sealed partial class UserControlProfileComboBox : UserControl
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public enum FriendicaProfileComboBoxFields { ProfileGender, ProfileMarital, ProfileCountry,
                                                    ProfileRegion, ProfileSexual };
        new public event EventHandler SizeChanged;

        public FriendicaProfileComboBoxFields Field
        {
            get
            {
                return (FriendicaProfileComboBoxFields)GetValue(FieldProperty);
            }
            set
            {
                SetValue(FieldProperty, value);
                SetHeader(value);
            }
        }
        public static DependencyProperty FieldProperty = DependencyProperty.Register("Field", typeof(FriendicaProfileComboBoxFields),
            typeof(UserControlProfileComboBox), new PropertyMetadata(0.0, new PropertyChangedCallback(OnFieldChanged)));

        private static void OnFieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }


        // property for datatype parameter
        public FriendicaEnumBaseViewmodel Enum
        {
            get
            {
                return (FriendicaEnumBaseViewmodel)GetValue(EnumProperty);
            }
            set
            {
                SetValue(EnumProperty, (FriendicaEnumBaseViewmodel)value);
            }
        }

        public static DependencyProperty EnumProperty = DependencyProperty.Register("Enum", typeof(FriendicaEnumBaseViewmodel),
            typeof(UserControlProfileComboBox), null);


        // property for selectable values of second combobox
        public FriendicaEnumBaseViewmodel EnumSecondField
        {
            get
            {
                return (FriendicaEnumBaseViewmodel)GetValue(EnumSecondFieldProperty);
            }
            set
            {
                SetValue(EnumSecondFieldProperty, (FriendicaEnumBaseViewmodel)value);
            }
        }

        public static DependencyProperty EnumSecondFieldProperty = DependencyProperty.Register("EnumSecondField", typeof(FriendicaEnumBaseViewmodel),
            typeof(UserControlProfileComboBox), null);


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
            typeof(UserControlProfileComboBox), null);


        public Visibility VisibilitySecondField
        {
            get
            {
                return (Visibility)GetValue(VisibilitySecondFieldProperty);
            }
            set
            {
                SetValue(VisibilitySecondFieldProperty, (Visibility)value);
            }
        }
        public static DependencyProperty VisibilitySecondFieldProperty = DependencyProperty.Register("VisibilitySecondField", typeof(Visibility),
            typeof(UserControlProfileComboBox), null);


        private string _elementText;
        public string ElementText
        {
            get
            {
                return (string)GetValue(ElementTextProperty);
            }
            set
            {
                _elementText = value;
                SetValue(ElementTextProperty, value);
            }
        }
        public static DependencyProperty ElementTextProperty = DependencyProperty.Register("ElementText", typeof(string), 
            typeof(UserControlProfileComboBox), new PropertyMetadata(0.0, new PropertyChangedCallback(OnElementTextChanged)));

        private static void OnElementTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UserControlProfileComboBox)d;

            if (e.NewValue.GetType() != typeof(double))
            {
                var _parNew = (e.NewValue.ToString()).Split(new char[] { '|' });

                if (_parNew.Length == 2)
                {
                    control.FirstElement = e.NewValue.ToString();
                    control.VisibilitySecondField = Visibility.Collapsed;
                }
                if (_parNew.Length == 3)
                {
                    control.FirstElement = "Country|" + _parNew[1];
                    control.SecondElement = "Region|" + _parNew[1] + "|" + _parNew[2];
                    control.VisibilitySecondField = Visibility.Visible;
                }

                SetVisibility(control, control.ElementText);
            }
        }


        private string _firstElement;
        public string FirstElement
        {
            get
            {
                return (string)GetValue(FirstElementProperty);
            }
            set
            {
                _firstElement = value;
                SetValue(FirstElementProperty, value);
            }
        }
        public static DependencyProperty FirstElementProperty = DependencyProperty.Register("FirstElement", typeof(string),
            typeof(UserControlProfileComboBox), new PropertyMetadata(0.0, new PropertyChangedCallback(OnFirstElementChanged)));

        private static void OnFirstElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fieldName = "";
            var control = (UserControlProfileComboBox)d;
            var _parNew = e.NewValue.ToString().Split(new char[] { '|' });
            var _parOld = e.OldValue.ToString().Split(new char[] { '|' });

            switch (control.Field)
            {
                case FriendicaProfileComboBoxFields.ProfileGender:
                    fieldName = "FriendicaGender";
                    break;
                case FriendicaProfileComboBoxFields.ProfileMarital:
                    fieldName = "FriendicaMarital";
                    break;
                case FriendicaProfileComboBoxFields.ProfileRegion:
                    fieldName = "FriendicaRegion";
                    if (_parNew[0] == "Country")
                    {
                        // if country is changed, prepare new list of states for display
                        if (e.OldValue.ToString() == "0" || _parNew[1] != _parOld[1])
                            control.EnumSecondField = new FriendicaEnumRegionViewmodel(_parNew[1]);
                    }
                    break;
                case FriendicaProfileComboBoxFields.ProfileSexual:
                    fieldName = "FriendicaSexual";
                    break;
            }

            // set ElementText in Viewmodel to new selections
            control.SetElementText(fieldName);
        }

        private void SetElementText(string fieldname)
        {
            var firstText = FirstElement.Split(new char[] { '|' })[1];

            string secondText = "";
            if (fieldname == "FriendicaRegion")
            {
                if (_secondElement == null || SecondElement == null)
                {
                }
                else
                {
                    var par = SecondElement.Split(new char[] { '|' });
                    secondText = (par.Length == 2) ? par[1] : par[2];
                }
                ElementText = fieldname + "|" + firstText + "|" + secondText;
            }
            else
                ElementText = fieldname + "|" + firstText;
        }

        private string _secondElement;
        public string SecondElement
        {
            get
            {
                return (string)GetValue(SecondElementProperty);
            }
            set
            {
                _secondElement = value;
                SetValue(SecondElementProperty, value);
            }
        }
        public static DependencyProperty SecondElementProperty = DependencyProperty.Register("SecondElement", typeof(string),
            typeof(UserControlProfileComboBox), new PropertyMetadata(0.0, new PropertyChangedCallback(OnSecondElementChanged)));

        private static void OnSecondElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UserControlProfileComboBox)d;
            // set ElementText in Viewmodel to new selections
            control.SetElementText("FriendicaRegion");
        }


        private static void SetVisibility(UserControlProfileComboBox control, string value)
        { 
            var _parameter = value.Split(new char[] { '|' });
            // if value is a single parameter item (FriendicaGender etc.)
            if (_parameter.Length == 2)
            {
                if (_parameter[1] == "" && !control.comboboxProfilesField.IsEnabled)
                {
                    control.textblockProfilesFieldHeader.Opacity = 0.5;
                    control.comboboxProfilesField.Visibility = Visibility.Collapsed;
                }
                else
                {
                    control.textblockProfilesFieldHeader.Opacity = 1.0;
                    control.comboboxProfilesField.Visibility = Visibility.Visible;
                }
                control.comboboxProfilesField2.Visibility = Visibility.Collapsed;
            }
            // if value is a double parameter item (FriendicaRegion)
            if (_parameter.Length == 3)
            {
                if (_parameter[1] == "" && _parameter[2] == "" && !control.comboboxProfilesField.IsEnabled)
                {
                    control.textblockProfilesFieldHeader.Opacity = 0.5;
                    control.comboboxProfilesField.Visibility = Visibility.Collapsed;
                    control.VisibilitySecondField = Visibility.Collapsed;
                }
                else
                {
                    control.textblockProfilesFieldHeader.Opacity = 1.0;
                    if (_parameter[2] == "" && !control.comboboxProfilesField.IsEnabled)
                        control.comboboxProfilesField.Visibility = Visibility.Collapsed;
                    else if (_parameter[1] == "" && !control.comboboxProfilesField2.IsEnabled)
                        control.VisibilitySecondField = Visibility.Collapsed;
                    else
                    {
                        control.comboboxProfilesField.Visibility = Visibility.Visible;
                        control.VisibilitySecondField = Visibility.Visible;
                    }
                }
            }
        }


        public UserControlProfileComboBox()
        {
            this.InitializeComponent();
        }


        private void SetHeader(FriendicaProfileComboBoxFields field)
        {
            switch (field)
            {
                case FriendicaProfileComboBoxFields.ProfileGender:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesGender");
                    Enum = new FriendicaEnumGenderViewmodel();
                    break;
                case FriendicaProfileComboBoxFields.ProfileMarital:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesMarital");
                    Enum = new FriendicaEnumMaritalViewmodel();
                    break;
                case FriendicaProfileComboBoxFields.ProfileRegion:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesRegion");
                    Enum = new FriendicaEnumCountryViewmodel();
                    EnumSecondField = new FriendicaEnumRegionViewmodel(_elementText);
                    break;
                case FriendicaProfileComboBoxFields.ProfileSexual:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesSexual");
                    Enum = new FriendicaEnumSexualViewmodel();
                    break;
                default:
                    break;
            }
        }

        private void comboboxProfilesField_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender != null)
            {
                SetVisibility(this, this.ElementText);

                object element = sender as ComboBox;
                do
                {
                    element = VisualTreeHelper.GetParent((DependencyObject)element);
                }
                while (element.GetType() != typeof(clsVariableGridView));

                SizeChanged?.Invoke(element, EventArgs.Empty);
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
    }
}
