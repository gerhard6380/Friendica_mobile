using System;
using System.ComponentModel;
using SeeberXamarin.Controls;
using Friendica_Mobile.UWP;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomTabViewScrollView), typeof(ScrollbarDisabledRenderer))]
namespace Friendica_Mobile.UWP
{
    public class ScrollbarDisabledRenderer : ScrollViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null || this.Element == null)
                return;

            if (e.OldElement != null)
                e.OldElement.PropertyChanged -= OnElementPropertyChanged;

            e.NewElement.PropertyChanged += OnElementPropertyChanged;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Control != null)
            {
                Control.HorizontalScrollBarVisibility = Windows.UI.Xaml.Controls.ScrollBarVisibility.Hidden;
                Control.VerticalScrollBarVisibility = Windows.UI.Xaml.Controls.ScrollBarVisibility.Hidden;
            }
        }
    }
}
