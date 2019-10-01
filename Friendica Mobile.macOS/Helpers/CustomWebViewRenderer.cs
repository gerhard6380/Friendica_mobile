//using Foundation;
//using Friendica_Mobile.macOS.Helpers;
//using SeeberXamarin.Controls;
//using WebKit;
//using Xamarin.Forms;
//using Xamarin.Forms.Platform.MacOS;

//[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
//namespace Friendica_Mobile.macOS.Helpers
//{

//    public class CustomWebViewRenderer : ViewRenderer<CustomWebView, WKWebView>
//    {
//        WKWebView _wkWebView;

//        protected override void OnElementChanged(ElementChangedEventArgs<CustomWebView> e)
//        {
//            base.OnElementChanged(e);

//            if (Control == null)
//            {
//                var config = new WKWebViewConfiguration();
//                _wkWebView = new WKWebView(Frame, config);
//                SetNativeControl(_wkWebView);
//            }
//            if (e.NewElement != null)
//            {
//                if ((Element.Source as UrlWebViewSource) != null)
//                {
//                    string url = (Element.Source as UrlWebViewSource).Url;
//                    Control.LoadRequest(new NSUrlRequest(new NSUrl(url)));
//                }
//                else if ((Element.Source as HtmlWebViewSource) != null)
//                {
//                    Control.LoadHtmlString((Element.Source as HtmlWebViewSource).Html, new NSUrl(""));
//                }
//            }
//        }

//    }
//}
