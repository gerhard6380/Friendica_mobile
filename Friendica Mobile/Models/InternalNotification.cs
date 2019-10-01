using System;
using Xamarin.Forms;

namespace Friendica_Mobile.Models
{
    public class InternalNotification : BindableClass
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public ImageSource Image { get; set; }
    }
}
