using Android.App;
using Android.Widget;
using Android.OS;
using Friendica_Mobile.PCL.Viewmodels;
using Friendica_Mobile.PCL.Strings;

namespace Friendica_Mobile.Droid
{
    [Activity(Label = "Friendica_Mobile.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        
        NetworkViewmodel mvvm;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            PCL.StaticMessageDialog.Dialog = new MessageDialogDroid(this);
            AppResources.Culture = new LocalizeDroid().GetCurrentCultureInfo();
            mvvm = new NetworkViewmodel();


            PCL.Settings.FriendicaServer = "http://mozartweg.dyndns.org/friendica";
            PCL.Settings.FriendicaUsername = "gerhard";
            PCL.Settings.FriendicaPassword = "30031982";

            await mvvm.LoadInitial();

            Button button1 = FindViewById<Button>(Resource.Id.button1);
            button1.Text = AppResources.buttonLike_Content;
            button1.Text = PCL.Settings.FriendicaServer;
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, System.EventArgs e)
        {
            var test = mvvm.Posts[0];
            //test.Testen();
        }
    }
}

