using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Friendica_Mobile.Views
{
    public partial class ImageFullscreen : ContentPage
    { 
        public ImageFullscreen(ImageSource source)
        {
            // load imagesource from clicked image into the source of this window
            InitializeComponent();
            DependencyService.Get<IStatusBar>().HideStatusBar();
            if (source != null)
                ImageSource.Source = source;
        }
    }
}
