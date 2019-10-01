using System;
using SeeberXamarin.Controls;
using Friendica_Mobile.UWP;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using Windows.Storage;
using System.Collections.Generic;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace Friendica_Mobile.UWP
{
    /// <summary>
    /// UWP WebView doesn't support JavaScript within a stringified html created in WebViewEditorHtml class
    /// we need to save the html code to a file in the local storage and then load the file in order get the JavaScript functions working
    /// </summary>
    public class CustomWebViewRenderer : WebViewRenderer
    {
        protected override async void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // hook once into NavigationStarting to abort loading the html string and load file instead
                Control.NavigationCompleted += Control_NavigationCompleted;
                
                // retrieve html code and save it into file webeditor.htm in local storage
                var html = (Element.Source as HtmlWebViewSource).Html;
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                // WebView cannot load html files in root folder of local storage, so we create a html subfolder
                storageFolder = await storageFolder.CreateFolderAsync("html", CreationCollisionOption.OpenIfExists);
                var file = await storageFolder.CreateFileAsync("webeditor.htm", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, html, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                Control.Navigate(new Uri("ms-appdata:///local/html/webeditor.htm"));
            }
        }

        private void Control_NavigationCompleted(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs args)
        {
            if (args.Uri.AbsolutePath.Contains("webeditor.htm"))
                Control.NavigationCompleted -= Control_NavigationCompleted;
            else
                Control.Navigate(new Uri("ms-appdata:///local/html/webeditor.htm"));
        }
    }
}