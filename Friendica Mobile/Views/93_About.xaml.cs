using System;
using System.Collections.Generic;
using Friendica_Mobile.Styles;
using Friendica_Mobile.ViewModel;
using Xamarin.Forms;

namespace Friendica_Mobile.Views
{
    public partial class About : BaseContentPage
    {
        public About()
        {
            InitializeComponent();

            // load email address after config is loaded (binding is not working properly in span)
            var mvvm = this.BindingContext as AboutViewModel;
            mvvm.OnConfigLoaded += (sender, e) => { SpanAdminEmail.Text = mvvm.SiteConfig.ConfigSite.SiteEmail; };
        }

        // needed to avoid rendering errors in ios designer
        void Handle_OnCommandBarPositionChanged(object sender, System.EventArgs e)
        {
        }
        void Handle_MoreOptionsButtonClicked(object sender, System.EventArgs e)
        {
        }

    }
}
