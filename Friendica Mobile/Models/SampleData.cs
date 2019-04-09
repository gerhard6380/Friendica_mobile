using System;
using System.Collections.Generic;
using System.Linq;

namespace Friendica_Mobile.Models
{
    public static class SampleData
    {
        public static List<FriendicaUser> Friends;
        public static List<FriendicaUser> Forums;
        public static List<FriendicaGroup> Groups;

        public static List<FriendicaUser> ContactsFriendsSamples()
        {
            if (Friends == null)
                CreateFriends();
            return Friends;
        }

        public static List<FriendicaUser> ContactsForumsSamples()
        {
            if (Forums == null)
                CreateForums();
            return Forums;
        }

        public static List<FriendicaGroup> ContactsGroupsSamples()
        {
            if (Friends == null)
                CreateFriends();
            if (Forums == null)
                CreateForums();
            if (Groups == null)
                CreateGroups();
            return Groups;
        }

        private static void CreateFriends()
        {
            Friends = new List<FriendicaUser>();

            Friends.Add(new FriendicaUser()
            {
                UserCid = 1,
                UserName = "Tyrion Lannister",
                UserUrl = "https://friendica.server.test/profile/tyrion",
                UserScreenName = "tyrion",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/5/50/Tyrion_Lannister-Peter_Dinklage.jpg"
            });

            Friends.Add(new FriendicaUser()
            {
                UserCid = 2,
                UserName = "Cersei Lannister",
                UserUrl = "https://friendica.server.test/profile/cersei",
                UserScreenName = "cersei",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/9/94/Cersei_Lannister-Lena_Headey.jpg"
            });

            Friends.Add(new FriendicaUser()
            {
                UserCid = 3,
                UserName = "Daenerys Targaryen",
                UserUrl = "https://friendica.server.test/profile/daenerys",
                UserScreenName = "daenerys",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/0/0d/Daenerys_Targaryen_with_Dragon-Emilia_Clarke.jpg"
            });

            Friends.Add(new FriendicaUser()
            {
                UserCid = 4,
                UserName = "Jon Snow",
                UserUrl = "https://friendica.server.test/profile/jon_snow",
                UserScreenName = "jon_snow",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/f/f0/Jon_Snow-Kit_Harington.jpg"
            });

            Friends.Add(new FriendicaUser()
            {
                UserCid = 5,
                UserName = "Sansa Stark",
                UserUrl = "https://friendica.server.test/profile/sansa",
                UserScreenName = "sansa",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/7/74/SophieTurnerasSansaStark.jpg"
            });

            Friends.Add(new FriendicaUser()
            {
                UserCid = 6,
                UserName = "Arya Stark",
                UserUrl = "https://friendica.server.test/profile/aryastark",
                UserScreenName = "aryastark",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/3/39/Arya_Stark-Maisie_Williams.jpg"
            });

            Friends.Add(new FriendicaUser()
            {
                UserCid = 7,
                UserName = "Ned Stark",
                UserUrl = "https://friendica.server.test/profile/nedstark",
                UserScreenName = "nedstark",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/5/52/Ned_Stark-Sean_Bean.jpg"
            });


            Friends.Add(new FriendicaUser()
            {
                UserCid = 8,
                UserName = "Jaime Lannister",
                UserUrl = "https://friendica.server.test/profile/jaimeL",
                UserScreenName = "jaimeL",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/b/b4/Jaime_Lannister-Nikolaj_Coster-Waldau.jpg"
            });


            Friends.Add(new FriendicaUser()
            {
                UserCid = 9,
                UserName = "Petyr Baelish",
                UserUrl = "https://friendica.server.test/profile/petyr",
                UserScreenName = "petyr",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/d/d5/Aidan_Gillen_playing_Petyr_Baelish.jpg"
            });

            Friends.Add(new FriendicaUser()
            {
                UserCid = 10,
                UserName = "Catelyn Stark",
                UserUrl = "https://friendica.server.test/profile/catelyn",
                UserScreenName = "catelyn",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/2/25/Catelyn_Stark-Michelle_Fairley_S3.jpg"
            });
        }

        private static void CreateForums()
        {
            Forums = new List<FriendicaUser>();

            Forums.Add(new FriendicaUser()
            {
                UserCid = 500,
                UserName = "Games of Thrones",
                UserUrl = "https://friendica.server.test/profiles/thrones",
                UserScreenName = "thrones",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/9/93/AGameOfThrones.jpg"
            });

            Forums.Add(new FriendicaUser()
            {
                UserCid = 501,
                UserName = "Clash of Kings",
                UserUrl = "https://friendica.server.test/profiles/kings",
                UserScreenName = "kings",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/3/39/AClashOfKings.jpg"
            });

            Forums.Add(new FriendicaUser()
            {
                UserCid = 502,
                UserName = "Storm of Swords",
                UserUrl = "https://friendica.server.test/profiles/swords",
                UserScreenName = "swords",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/2/24/AStormOfSwords.jpg"
            });

            Forums.Add(new FriendicaUser()
            {
                UserCid = 503,
                UserName = "Feast for Crowns",
                UserUrl = "https://friendica.server.test/profiles/crowns",
                UserScreenName = "crowns",
                UserProfileImageUrlHttps = "https://upload.wikimedia.org/wikipedia/en/a/a3/AFeastForCrows.jpg"
            });
        }

        private static void CreateGroups()
        {
            Groups = new List<FriendicaGroup>();

            Groups.Add(new FriendicaGroup()
            {
                GroupGid = 1,
                GroupName = "Stark",
                GroupUser = Friends.FindAll(u => u.UserName.Contains("Stark")).OrderBy(u => u.UserName).ToList()
            });

            Groups.Add(new FriendicaGroup()
            {
                GroupGid = 2,
                GroupName = "Lannister",
                GroupUser = Friends.FindAll(u => u.UserName.Contains("Lannister")).OrderBy(u => u.UserName).ToList()
            });

            Groups.Add(new FriendicaGroup()
            {
                GroupGid = 3,
                GroupName = "Others",
                GroupUser = Friends.FindAll(u => !u.UserName.Contains("Stark") && !u.UserName.Contains("Lannister")).OrderBy(u => u.UserName).ToList()
            });
        }
    }
}
