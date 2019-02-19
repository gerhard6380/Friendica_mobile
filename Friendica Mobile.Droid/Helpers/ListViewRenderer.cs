using Android.Content;
using Friendica_Mobile.Droid.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ListView), typeof(CustomListViewRenderer))]
namespace Friendica_Mobile.Droid.Helpers
{

    public class CustomListViewRenderer : ListViewRenderer
    {
        public CustomListViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                Control.NestedScrollingEnabled = true;
            }
        }
    }
}
