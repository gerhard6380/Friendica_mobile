using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Friendica_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendicaDataTemplates : ResourceDictionary
    {
        public FriendicaDataTemplates()
        {
            InitializeComponent();
        }

        /// <summary>
        /// changes text color according to selected theme, used this way as DynamicResource was not working
        /// </summary>
        void LabelContactsFriends_SizeChanged(object sender, System.EventArgs e)
        {
            var label = sender as Label;
            label.TextColor = (App.SelectedTheme == App.ApplicationTheme.Dark) ? Color.White : Color.Black;
        }

    }
}
