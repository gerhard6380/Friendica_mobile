using System.Windows.Input;
using Friendica_Mobile.Strings;
using Xamarin.Forms;
using System;

namespace Friendica_Mobile.ViewModel
{
    public class HelpViewModel : BaseViewModel
    {


#region Commands

        private ICommand _linkAppSupportpageCommand;
        public ICommand  LinkAppSupportpageCommand => _linkAppSupportpageCommand ?? (_linkAppSupportpageCommand = new Command(LinkAppSupportpage));
        private void LinkAppSupportpage()
        {
            Launchers.OpenUrlWithZrl("https://friendica.hasecom.at/profile/friendicamobile", true);
        }

        private ICommand _linkFriendicaSupportCommand;
        public ICommand LinkFriendicaSupportCommand => _linkFriendicaSupportCommand ?? (_linkFriendicaSupportCommand = new Command(LinkFriendicaSupport));
        private void LinkFriendicaSupport()
        {
            Launchers.OpenUrlWithZrl("https://forum.friendi.ca/profile/helpers", true);
        }

        private ICommand _linkFriendicaDevelopersCommand;
        public ICommand LinkFriendicaDevelopersCommand => _linkFriendicaDevelopersCommand ?? (_linkFriendicaDevelopersCommand = new Command(LinkFriendicaDevelopers));
        private void LinkFriendicaDevelopers()
        {
            Launchers.OpenUrlWithZrl("https://forum.friendi.ca/profile/developers", true);
        }

        private ICommand _hyperlinkGithubCommand;
        public ICommand HyperlinkGithubCommand => _hyperlinkGithubCommand ?? (_hyperlinkGithubCommand = new Command(HyperlinkGithub));
        public void HyperlinkGithub()
        {
            var url = new Uri("https://github.com/friendica/friendica");
            Device.OpenUri(url);
        }

        private ICommand _hyperlinkFriendicaCommand;
        public ICommand HyperlinkFriendicaCommand => _hyperlinkFriendicaCommand ?? (_hyperlinkFriendicaCommand = new Command(HyperlinkFriendica));
        public void HyperlinkFriendica()
        {
            var url = new Uri("https://friendi.ca");
            Device.OpenUri(url);
        }

        // https://www.youtube.com/channel/UCnRt5h9VTtaec3QAn6u3mqQ

#endregion


        public HelpViewModel()
        {
            Title = AppResources.pageTitleHelp_Text;
        }



    }
}
