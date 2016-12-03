using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Friendica_Mobile.Styles
{
    public sealed partial class UserControlProfileRichText : UserControl
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public enum FriendicaProfileRichTextFields { ProfileHomepage, ProfileAbout, ProfileSocialNetworks,
                                                     ProfileInterest, ProfileLikes, ProfileDislikes,
                                                     ProfileMusic, ProfileBook, ProfileTv, ProfileFilm,
                                                     ProfileRomance, ProfileWork, ProfileEducation};
        
        new public event EventHandler SizeChanged;

        // property for datatype parameter
        private FriendicaProfileRichTextFields _field;
        public FriendicaProfileRichTextFields Field
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
            typeof(UserControlProfileRichText), null);


        public Paragraph ElementRichText
        {
            get
            {
                return (Paragraph)GetValue(ElementRichTextProperty);
            }
            set
            {
                SetValue(ElementRichTextProperty, value);
            }
        }
        public static DependencyProperty ElementRichTextProperty = DependencyProperty.Register("ElementRichText", typeof(Paragraph), 
            typeof(UserControlProfileRichText), new PropertyMetadata(0.0, new PropertyChangedCallback(OnElementRichTextChanged)));

        private static void OnElementRichTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (UserControlProfileRichText)d;
            if (e.NewValue.GetType() != typeof(double))
            {
                if (((Paragraph)e.NewValue).Inlines.Count == 0)
                {
                    control.textblockProfilesFieldHeader.Opacity = 0.5;
                    control.rtblockProfilesField.Visibility = Visibility.Collapsed;
                    control.borderRtbField.BorderThickness = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    control.textblockProfilesFieldHeader.Opacity = 1.0;
                    control.rtblockProfilesField.Visibility = Visibility.Visible;
                    control.borderRtbField.BorderThickness = new Thickness(2, 2, 2, 2);
                }
            }
        }


        public UserControlProfileRichText()
        {
            this.InitializeComponent();
        }


        private void SetHeader(FriendicaProfileRichTextFields field)
        {
            switch (field)
            {
                case FriendicaProfileRichTextFields.ProfileHomepage:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesHomepage");
                    break;
                case FriendicaProfileRichTextFields.ProfileAbout:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesAbout");
                    break;
                case FriendicaProfileRichTextFields.ProfileSocialNetworks:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesSocialNetworks");
                    break;
                case FriendicaProfileRichTextFields.ProfileInterest:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesInterest");
                    break;
                case FriendicaProfileRichTextFields.ProfileLikes:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesLikes");
                    break;
                case FriendicaProfileRichTextFields.ProfileDislikes:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesDislikes");
                    break;
                case FriendicaProfileRichTextFields.ProfileMusic:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesMusic");
                    break;
                case FriendicaProfileRichTextFields.ProfileBook:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesBook");
                    break;
                case FriendicaProfileRichTextFields.ProfileTv:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesTv");
                    break;
                case FriendicaProfileRichTextFields.ProfileFilm:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesFilm");
                    break;
                case FriendicaProfileRichTextFields.ProfileRomance:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesRomance");
                    break;
                case FriendicaProfileRichTextFields.ProfileWork:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesWork");
                    break;
                case FriendicaProfileRichTextFields.ProfileEducation:
                    textblockProfilesFieldHeader.Text = loader.GetString("textblockProfilesEducation");
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


    

    }
}
