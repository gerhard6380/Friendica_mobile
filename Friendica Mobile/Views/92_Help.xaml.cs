using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friendica_Mobile.Styles;
using Friendica_Mobile.ViewModel;
using Xamarin.Forms;

namespace Friendica_Mobile.Views
{
    public partial class Help : BaseContentPage
    {
        public Help()
        {
            InitializeComponent();

            // TODO: funktioniert noch nicht zuverlässig
            //var mvvm = this.BindingContext as HelpViewModel;
            //var click = new ClickGestureRecognizer();
            //click.Clicked += (sender, e) => 
            //{
            //    mvvm.HyperlinkGithub();
            //};
            //SpanGithubLink.GestureRecognizers.Add(click);

            //var clickFriendica = new ClickGestureRecognizer();
            //clickFriendica.Clicked += (sender, e) => 
            //{
            //    mvvm.HyperlinkFriendica();
            //};
            //SpanFriendicaLink.GestureRecognizers.Add(clickFriendica);
        }
    }
}
