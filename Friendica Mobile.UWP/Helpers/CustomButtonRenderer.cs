﻿using SeeberXamarin.Controls;
using Friendica_Mobile.UWP;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomButton), typeof(CustomButtonRenderer))]
namespace Friendica_Mobile.UWP
{
    public class CustomButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // change the requested theme according to the defined app theme 
                Settings.AppThemeModeChanged += (sender, args) => { SetTheme(); };
                SetTheme();
            }
        }


        private void SetTheme()
        {
            if (Control != null)
            {
                if (Friendica_Mobile.App.SelectedTheme == Friendica_Mobile.App.ApplicationTheme.Light)
                    Control.RequestedTheme = Windows.UI.Xaml.ElementTheme.Light;
                else
                    Control.RequestedTheme = Windows.UI.Xaml.ElementTheme.Dark;
            }
        }

    }
}