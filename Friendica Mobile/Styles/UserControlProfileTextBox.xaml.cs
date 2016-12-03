using Friendica_Mobile.Mvvm;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Friendica_Mobile.Styles
{
    public sealed partial class UserControlProfileTextBox : UserControl
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public enum FriendicaProfileTextBoxFields { ProfileName, ProfileDescription, ProfilePublicKeywords,
                                                    ProfilePrivateKeywords, ProfileAddress, ProfileCity,
                                                    ProfilePostalCode, ProfileHometown, ProfileMaritalWith,
                                                    ProfilePolitic, ProfileReligion };
        

        new public event EventHandler SizeChanged;

        // property for datatype parameter
        private FriendicaProfileTextBoxFields _field;
        public FriendicaProfileTextBoxFields Field
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
            typeof(UserControlProfileTextBox), null);


        public string ElementText
        {
            get
            {
                return (string)GetValue(ElementTextProperty);
            }
            set
            {
                SetValue(ElementTextProperty, value);
            }
        }
        public static DependencyProperty ElementTextProperty = DependencyProperty.Register("ElementText", typeof(string), 
            typeof(UserControlProfileTextBox), new PropertyMetadata(0.0, new PropertyChangedCallback(OnElementTextChanged)));

        private static void OnElementTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UserControlProfileTextBox)d;
            SetVisibility(control, e.NewValue.ToString());
        }

        private static void SetVisibility(UserControlProfileTextBox control, string value)
        {
            if (value == "" && !control.textboxProfilesField.IsEnabled)
            {
                control.textblockProfilesFieldHeader.Opacity = 0.5;
                control.textboxProfilesField.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.textblockProfilesFieldHeader.Opacity = 1.0;
                control.textboxProfilesField.Visibility = Visibility.Visible;
            }
        }


        public bool IsEditorEnabled { get; set; }

        public UserControlProfileTextBox()
        {
            this.InitializeComponent();
        }


        private void SetHeader(FriendicaProfileTextBoxFields field)
        {
            switch (field)
            {
                case FriendicaProfileTextBoxFields.ProfileName:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesName");
                    break;
                case FriendicaProfileTextBoxFields.ProfileDescription:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesDescription");
                    break;
                case FriendicaProfileTextBoxFields.ProfilePublicKeywords:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesPublicKeywords");
                    break;
                case FriendicaProfileTextBoxFields.ProfilePrivateKeywords:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesPrivateKeywords");
                    break;
                case FriendicaProfileTextBoxFields.ProfileAddress:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesAddress");
                    break;
                case FriendicaProfileTextBoxFields.ProfileCity:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesCity");
                    break;
                case FriendicaProfileTextBoxFields.ProfilePostalCode:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesPostalCode");
                    break;
                case FriendicaProfileTextBoxFields.ProfileHometown:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesHometown");
                    break;
                case FriendicaProfileTextBoxFields.ProfileMaritalWith:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesMaritalWith");
                    break;
                case FriendicaProfileTextBoxFields.ProfilePolitic:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesPolitic");
                    break;
                case FriendicaProfileTextBoxFields.ProfileReligion:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesReligion");
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

        private void textboxProfilesField_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender != null)
            {
                SetVisibility(this, this.ElementText);

                object element = sender as TextBox;
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
