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
        
        TestViewmodel mvvm;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            var localize = new LocalizeDroid();
            var dialog = new MessageDialogDroid(this);
            mvvm = new TestViewmodel(dialog, localize);

            mvvm.Testen();

            Button button1 = FindViewById<Button>(Resource.Id.button1);
            button1.Text = AppResources.buttonLike_Content;
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, System.EventArgs e)
        {
            mvvm.ShowDialog();
        }
    }
}

