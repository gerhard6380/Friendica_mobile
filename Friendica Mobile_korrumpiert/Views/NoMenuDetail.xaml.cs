using System;
using System.Collections.Generic;
using Friendica_Mobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Friendica_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NoMenuDetail
    {
        public NoMenuDetail()
        {
            InitializeComponent();
        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            var vm = BindingContext as NoMenuViewModel;
            //vm.NavigationAllowed = !vm.NavigationAllowed;
            //labelSetting.Text = vm.NavigationAllowed.ToString();
        }
    }
}
