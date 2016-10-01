using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Friendica_Mobile.Models
{
    class FriendicaProfileSamples
    {
        public ObservableCollection<FriendicaProfile> ProfileSamples;
        public bool MultiProfiles = true;
        public string GlobalDirectory = "http://dir.friendica.com";
        public FriendicaUser FriendicaOwner;

        public FriendicaProfileSamples()
        {
            ProfileSamples = new ObservableCollection<FriendicaProfile>();
            FriendicaOwner = new FriendicaUser("\"id\":1,\"id_str\":\"1\",\"name\":\"That's me\",\"screen_name\":\"test1\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":2,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":1,\"network\":\"dfrn\"");
            FillSampleData();
        }

        private void FillSampleData()
        {
            var profile1 = new FriendicaProfile();
            profile1.ProfileName = "Default";
            profile1.ProfileIsDefault = true;
            profile1.ProfileHideFriends = true;
            profile1.ProfilePhoto = "http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png";
            profile1.ProfileThumb = "http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png";
            profile1.ProfilePublish = true;
            profile1.ProfileNetPublish = true;
            profile1.ProfilePublicKeywords = "clash, of, kings, feast, for, crowns, games, of, thrones, storm, of, swords";
            profile1.ProfileLikes = "Games Of Thrones";
            profile1.ProfileUsers = new List<FriendicaUser>();

            var profile2 = new FriendicaProfile();
            profile2.ProfileName = "Starks";
            profile2.ProfileIsDefault = false;
            profile2.ProfileHideFriends = false;
            profile2.ProfilePhoto = "http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png";
            profile2.ProfileThumb = "http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png";
            profile2.ProfilePublish = true;
            profile2.ProfileNetPublish = true;
            profile2.ProfileDescription = "Fan of Games of Thrones";
            profile2.ProfileDateOfBirth = "1948-02-29";
            profile2.ProfileAddress = "1477 Gandy Street";
            profile2.ProfileCity = "East Syracuse";
            profile2.ProfileRegion = "New York";
            profile2.ProfilePostalCode = "13057";
            profile2.ProfileCountry = "USA";
            profile2.ProfileHometown = "New Jersey";
            profile2.ProfileGender = "Male";
            profile2.ProfileMarital = "Partner";
            profile2.ProfileMaritalWith = "Dorothy";
            profile2.ProfileMaritalSince = "2015-12-18";
            profile2.ProfileSexual = "Women";
            profile2.ProfilePolitic = "Descriptive words about the political views of the user.";
            profile2.ProfileReligion = "Descriptive words about the religious attitudes of the user.";
            profile2.ProfilePublicKeywords = "clash, of, kings, feast, for, crowns, games, of, thrones, storm, of, swords";
            profile2.ProfilePrivateKeywords = "Stark, Lannister, Targaryen, Snow, Baelish";
            profile2.ProfileLikes = "Everything connected with magic stories and dragons. <img src=\"http://eskipaper.com/images/free-dragon-picture-4.jpg\" alt =\"Bild/Foto\">";
            profile2.ProfileDislikes = "Long winters and Children Of The Forests";
            profile2.ProfileAbout = "Some descriptive words in general about the user.";
            profile2.ProfileMusic = "Ramin Djawadi - Games Of Thrones Soundtrack <a href=\"https://www.amazon.de/Game-Thrones-Ramin-Djawadi/dp/B004YXL61G\" target=\"_blank\">https://www.amazon.de/Game-Thrones-Ramin-Djawadi/dp/B004YXL61G</a>";
            profile2.ProfileBook = "<span style=\"color: red;\"><strong>Lord of the Rings</strong></span>";
            profile2.ProfileTv = "<em>Game of Thrones</em>, The Simpsons, Big Bang Theory";
            profile2.ProfileFilm = "Lord of the Rings";
            profile2.ProfileInterest = "Some descriptive words about hobbies and interests.";
            profile2.ProfileRomance = "Some descriptive words about love and romance " + System.Net.WebUtility.HtmlDecode("&#x1F497;") + ".";
            profile2.ProfileWork = "Some descriptive words about work and employment status.";
            profile2.ProfileEducation = "<ul class=\"listbullet\" style=\"list-style-type: circle;\"><li>1st stage of education</li><li>2nd stage of education</li><li>Massachusetts Institute of Technology <a href=\"http://web.mit.edu\" target=\"_blank\">http://web.mit.edu</a></li></ul>";
            profile2.ProfileSocialNetworks = "<a href=\"https://de-de.facebook.com/GameOfThrones\" target=\"_blank\">Facebook page of Games of Thrones</a>";
            profile2.ProfileHomepage = "http://sdfpazarlama.com";
            profile2.ProfileUsers = new List<FriendicaUser>();
            var prof2user1 = new FriendicaUserExtended(6, "Arya Stark", "https://friendica.server.text/profile/aryastark", "aryastark", "https://upload.wikimedia.org/wikipedia/en/3/39/Arya_Stark-Maisie_Williams.jpg", ContactTypes.Friends);
            var prof2user2 = new FriendicaUserExtended(10, "Catelyn Stark", "https://friendica.server.text/profile/catelyn", "catelyn", "https://upload.wikimedia.org/wikipedia/en/1/1b/Catelyn_Stark_S3.jpg", ContactTypes.Friends);
            var prof2user3 = new FriendicaUserExtended(7, "Ned Stark", "https://friendica.server.text/profile/nedstark", "nedstark", "https://upload.wikimedia.org/wikipedia/en/4/44/Ned_Stark_as_Portrayed_by_Sean_Bean_in_the_television_series_2011.jpg", ContactTypes.Friends);
            var prof2user4 = new FriendicaUserExtended(5, "Sansa Stark", "https://friendica.server.text/profile/sansa", "sansa", "https://upload.wikimedia.org/wikipedia/en/7/74/SophieTurnerasSansaStark.jpg", ContactTypes.Friends);
            profile2.ProfileUsers.Add(prof2user1.User);
            profile2.ProfileUsers.Add(prof2user2.User);
            profile2.ProfileUsers.Add(prof2user3.User);
            profile2.ProfileUsers.Add(prof2user4.User);

            var profile3 = new FriendicaProfile();
            profile3.ProfileName = "Lannisters";
            profile3.ProfileIsDefault = false;
            profile3.ProfileHideFriends = false;
            profile3.ProfilePhoto = "http://howmanyarethere.net/wp-content/uploads/2012/06/game-of-thrones-poster.jpg";
            profile3.ProfileThumb = "http://howmanyarethere.net/wp-content/uploads/2012/06/game-of-thrones-poster.jpg";
            profile3.ProfilePublish = true;
            profile3.ProfileNetPublish = true;
            profile3.ProfileDateOfBirth = "0000-02-28";
            profile3.ProfileRegion = "New York";
            profile3.ProfileCountry = "USA";
            profile3.ProfileGender = "Male";
            profile3.ProfileAbout = "Some different information shown only to Lennisters.";
            profile3.ProfileBook = "Clash of Kings, Feast for Crowns, Games of Thrones, Storm of Swords";
            profile3.ProfileInterest = "Different information about hobbies and interests shown only to Lennisters.";
            profile3.ProfileHomepage = "http://sdfpazarlama.com";
            profile3.ProfileUsers = new List<FriendicaUser>();
            var prof3user1 = new FriendicaUserExtended(2, "Cersei Lannister", "https://friendica.server.text/profile/cersei", "cersei", "https://upload.wikimedia.org/wikipedia/en/a/a7/Queencersei.jpg", ContactTypes.Friends);
            var prof3user2 = new FriendicaUserExtended(8, "Jaime Lannister", "https://friendica.server.text/profile/jaimeL", "jaimeL", "https://upload.wikimedia.org/wikipedia/en/b/b5/JaimeLannister.jpg", ContactTypes.Friends);
            var prof3user3 = new FriendicaUserExtended(1, "Tyrion Lannister", "https://friendica.server.test/profile/tyrion", "tyrion", "https://upload.wikimedia.org/wikipedia/en/5/50/Tyrion_Lannister-Peter_Dinklage.jpg", ContactTypes.Friends);
            profile3.ProfileUsers.Add(prof3user1.User);
            profile3.ProfileUsers.Add(prof3user2.User);
            profile3.ProfileUsers.Add(prof3user3.User);

            ProfileSamples.Add(profile1);
            ProfileSamples.Add(profile2);
            ProfileSamples.Add(profile3);
        }
    }
}
