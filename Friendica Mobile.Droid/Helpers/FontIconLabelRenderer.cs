using Android.Content;
using Android.Graphics;
using Friendica_Mobile.Droid.Helpers;
using Friendica_Mobile.Styles;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FontIconLabel), typeof(FontIconLabelRenderer))]
namespace Friendica_Mobile.Droid.Helpers
{
    
    public class FontIconLabelRenderer : LabelRenderer
    {
        public FontIconLabelRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                Control.Typeface = Typeface.CreateFromAsset(Context.Assets, "Fonts/SEGMDL2.TTF");
            }
        }
    }
}
