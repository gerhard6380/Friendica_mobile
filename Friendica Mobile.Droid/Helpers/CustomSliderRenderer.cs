using Android.Content;
using Android.Graphics;
using Friendica_Mobile.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Slider), typeof(CustomSliderRenderer))]
namespace Friendica_Mobile.Droid
{
    public class CustomSliderRenderer : SliderRenderer
    {
        public CustomSliderRenderer(Context context) : base(context)
        {
        }


        protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Settings.AppThemeModeChanged += (sender, args) => { SetTheme(); };
                SetTheme();
            }
        }


        private void SetTheme()
        {
            // set color of the line for the slider according to requested theme
            if (Control != null)
            {
                if (App.SelectedTheme == App.ApplicationTheme.Light)
                    Control.ProgressDrawable.SetColorFilter(Xamarin.Forms.Color.FromHex("#000000").ToAndroid(), PorterDuff.Mode.SrcIn);
                else
                    Control.ProgressDrawable.SetColorFilter(Xamarin.Forms.Color.FromHex("#FFFFFF").ToAndroid(), PorterDuff.Mode.SrcIn);
            }
        }
    }
}