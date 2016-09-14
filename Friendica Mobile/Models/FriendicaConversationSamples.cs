using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.Models
{
    class FriendicaConversationSamples
    {
        public ObservableCollection<FriendicaConversation> ConversationSamples;

        public FriendicaConversationSamples()
        {
            ConversationSamples = new ObservableCollection<FriendicaConversation>();
            FillSampleData();
        }

        private void FillSampleData()
        {
            var conv1 = new FriendicaConversation();
            conv1.IsLoaded = true;
            conv1.Messages = new ObservableCollection<FriendicaMessage>();
            var partnerC1 = new FriendicaUserExtended(9, "Petyr Baelish", "https://friendica.server.text/profile/petyr", "petyr", "https://upload.wikimedia.org/wikipedia/en/thumb/d/d5/Aidan_Gillen_playing_Petyr_Baelish.jpg/220px-Aidan_Gillen_playing_Petyr_Baelish.jpg", ContactTypes.Friends);

            var conv1Mess1 = new FriendicaMessage();
            conv1Mess1.MessageId = "6";
            conv1Mess1.MessageSenderId = 9;
            conv1Mess1.MessageText = "<code>#include &lt;iostream&gt;<br>using namespace std;<br>void main()<br>{<br>     cout &lt;&lt; \\\"Hello World!\\\" &lt;&lt; endl;   <br>     cout &lt;&lt; \\\"Welcome to C++ Programming\\\" &lt;&lt; endl; <br>}</code>";
            conv1Mess1.MessageRecipientId = 0;
            conv1Mess1.MessageCreatedAt = "Sun May 08 21:44:00 +0000 2016";
            conv1Mess1.MessageSenderScreenName = "Petyr Baelish";
            conv1Mess1.MessageRecipientScreenName = "Sample";
            conv1Mess1.MessageSender = partnerC1.User;
            conv1Mess1.MessageRecipient = null;
            conv1Mess1.MessageTitle = "A Storm of Swords";
            conv1Mess1.MessageSeen = "0";
            conv1Mess1.MessageParentUri = "conversation1";
            conv1.Messages.Add(conv1Mess1);
            


            var conv2 = new FriendicaConversation();
            conv2.IsLoaded = true;
            conv2.Messages = new ObservableCollection<FriendicaMessage>(); 
            var partnerC2 = new FriendicaUserExtended(6, "Arya Stark", "https://friendica.server.text/profile/aryastark", "aryastark", "https://upload.wikimedia.org/wikipedia/en/3/39/Arya_Stark-Maisie_Williams.jpg", ContactTypes.Friends);

            var conv2Mess1 = new FriendicaMessage();
            conv2Mess1.MessageId = "4";
            conv2Mess1.MessageSenderId = 6;
            conv2Mess1.MessageText = "Lorem ipsum dolor sit amet, <u>consectetur</u> adipisici elit, sed eiusmod <strong>tempor incidunt</strong> ut labore et dolore magna aliqua. :-)<br><br>Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquid ex ea commodi consequat. Quis aute iure <em>eprehenderit</em> in voluptate velit esse <span style=\\\"color: red;\\\">cillum dolore eu fugiat nulla pariatur.</span> Excepteur sint obcaecat cupiditat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            conv2Mess1.MessageRecipientId = 0;
            conv2Mess1.MessageCreatedAt = "Fri May 06 20:03:00 +0000 2016";
            conv2Mess1.MessageSenderScreenName = "Arya Stark";
            conv2Mess1.MessageRecipientScreenName = "Sample";
            conv2Mess1.MessageSender = partnerC2.User;
            conv2Mess1.MessageRecipient = null;
            conv2Mess1.MessageTitle = "A Games of Thrones";
            conv2Mess1.MessageSeen = "1";
            conv2Mess1.MessageParentUri = "conversation2";
            conv2.Messages.Add(conv2Mess1);

            var conv2Mess2 = new FriendicaMessage();
            conv2Mess2.MessageId = "5";
            conv2Mess2.MessageSenderId = 0;
            conv2Mess2.MessageText = "At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
            conv2Mess2.MessageRecipientId = 6;
            conv2Mess2.MessageCreatedAt = "Sun May 08 11:33:00 +0000 2016";
            conv2Mess2.MessageSenderScreenName = "Sample";
            conv2Mess2.MessageRecipientScreenName = "Arya Stark";
            conv2Mess2.MessageSender = null;
            conv2Mess2.MessageRecipient = partnerC2.User;
            conv2Mess2.MessageTitle = "A Games of Thrones";
            conv2Mess2.MessageSeen = "1";
            conv2Mess2.MessageParentUri = "conversation2";
            conv2.Messages.Add(conv2Mess2);

            var conv3 = new FriendicaConversation();
            conv3.IsLoaded = true;
            conv3.Messages = new ObservableCollection<FriendicaMessage>();
            var partnerC3 = new FriendicaUserExtended(3, "Daenerys Targaryen", "https://friendica.server.text/profile/daenerys", "daenerys", "https://upload.wikimedia.org/wikipedia/en/0/0d/Daenerys_Targaryen_with_Dragon-Emilia_Clarke.jpg", ContactTypes.Friends);

            var conv3Mess1 = new FriendicaMessage();
            conv3Mess1.MessageId = "1";
            conv3Mess1.MessageSenderId = 0;
            conv3Mess1.MessageText = "Quis aute iure reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. :coffee<br><br><br><strong class=\\\"author\\\">Max Mustermann wrote:</strong><blockquote>Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. :-)</blockquote><br><br><a href=\\\"https://de.wikipedia.org/wiki/Friendica\\\" target=\\\"_blank\\\">https://de.wikipedia.org/wiki/Friendica</a>";
            conv3Mess1.MessageRecipientId = 3;
            conv3Mess1.MessageCreatedAt = "Fri May 06 18:20:00 +0000 2016";
            conv3Mess1.MessageSenderScreenName = "Sample";
            conv3Mess1.MessageRecipientScreenName = "Daenerys Targaryen";
            conv3Mess1.MessageSender = null;
            conv3Mess1.MessageRecipient = partnerC3.User;
            conv3Mess1.MessageTitle = "A Clash of Kings";
            conv3Mess1.MessageSeen = "1";
            conv3Mess1.MessageParentUri = "conversation3";
            conv3.Messages.Add(conv3Mess1);

            var conv3Mess2 = new FriendicaMessage();
            conv3Mess2.MessageId = "2";
            conv3Mess2.MessageSenderId = 3;
            conv3Mess2.MessageText = "Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis.";
            conv3Mess2.MessageRecipientId = 0;
            conv3Mess2.MessageCreatedAt = "Fri May 06 19:23:00 +0000 2016";
            conv3Mess2.MessageSenderScreenName = "Daenerys Targaryen";
            conv3Mess2.MessageRecipientScreenName = "Sample";
            conv3Mess2.MessageSender = partnerC3.User;
            conv3Mess2.MessageRecipient = null;
            conv3Mess2.MessageTitle = "A Clash of Kings";
            conv3Mess2.MessageSeen = "1";
            conv3Mess2.MessageParentUri = "conversation3";
            conv3.Messages.Add(conv3Mess2);

            var conv3Mess3 = new FriendicaMessage();
            conv3Mess3.MessageId = "3";
            conv3Mess3.MessageSenderId = 0;
            conv3Mess3.MessageText = "Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat. Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi.";
            conv3Mess3.MessageRecipientId = 3;
            conv3Mess3.MessageCreatedAt = "Fri May 06 19:26:00 +0000 2016";
            conv3Mess3.MessageSenderScreenName = "Sample";
            conv3Mess3.MessageRecipientScreenName = "Daenerys Targaryen";
            conv3Mess3.MessageSender = null;
            conv3Mess3.MessageRecipient = partnerC3.User;
            conv3Mess3.MessageTitle = "A Clash of Kings";
            conv3Mess3.MessageSeen = "1";
            conv3Mess3.MessageParentUri = "conversation3";
            conv3.Messages.Add(conv3Mess3);

            ConversationSamples.Add(conv1);
            ConversationSamples.Add(conv2);
            ConversationSamples.Add(conv3);
        }
    }
}
