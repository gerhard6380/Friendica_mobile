using BackgroundTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI.Xaml.Documents;

namespace Friendica_Mobile.Models
{
    public class FriendicaActivities : FriendicaBaseModel
    {
        // API interface properties
        private const string activitiesLikeKey = "like";
        private const string activitiesDislikeKey = "dislike";
        private const string activitiesAttendYesKey = "attendyes";
        private const string activitiesAttendNoKey = "attendno";
        private const string activitiesAttendMaybeKey = "attendmaybe";

        private List<FriendicaUserExtended> _activitiesLike;
        private List<FriendicaUserExtended> _activitiesDislike;
        private List<FriendicaUserExtended> _activitiesAttendYes;
        private List<FriendicaUserExtended> _activitiesAttendNo;
        private List<FriendicaUserExtended> _activitiesAttendMaybe;

        public List<FriendicaUserExtended> ActivitiesLike
        {
            get { return _activitiesLike; }
            set { _activitiesLike = value; }
        }

        public List<FriendicaUserExtended> ActivitiesDislike
        {
            get { return _activitiesDislike; }
            set { _activitiesDislike = value; }
        }

        public List<FriendicaUserExtended> ActivitiesAttendYes
        {
            get { return _activitiesAttendYes; }
            set { _activitiesAttendYes = value; }
        }

        public List<FriendicaUserExtended> ActivitiesAttendNo
        {
            get { return _activitiesAttendNo; }
            set { _activitiesAttendNo = value; }
        }

        public List<FriendicaUserExtended> ActivitiesAttendMaybe
        {
            get { return _activitiesAttendMaybe; }
            set { _activitiesAttendMaybe = value; }
        }


        public FriendicaActivities()
        {
            ActivitiesLike = new List<FriendicaUserExtended>();
            ActivitiesDislike = new List<FriendicaUserExtended>();
            ActivitiesAttendYes = new List<FriendicaUserExtended>();
            ActivitiesAttendNo = new List<FriendicaUserExtended>();
            ActivitiesAttendMaybe = new List<FriendicaUserExtended>();
        }


        public FriendicaActivities(JsonObject jsonObject) : this()
        {
            ActivitiesLike = ConvertAndSort(jsonObject, activitiesLikeKey);
            ActivitiesDislike = ConvertAndSort(jsonObject, activitiesDislikeKey);
            ActivitiesAttendYes = ConvertAndSort(jsonObject, activitiesAttendYesKey);
            ActivitiesAttendNo = ConvertAndSort(jsonObject, activitiesAttendNoKey);
            ActivitiesAttendMaybe = ConvertAndSort(jsonObject, activitiesAttendMaybeKey);
        }

        private List<FriendicaUserExtended> ConvertAndSort(JsonObject jsonObject, string key)
        {
            List<FriendicaUserExtended> users = new List<FriendicaUserExtended>();
            try
            { users = ConvertJsonToObjects(new FriendicaUserExtended(), jsonObject.GetNamedArray(key, null)); }
            catch
            { return null; }
            var usersOrdered = users.OrderBy(a => a.User.UserScreenName);
            var list = new List<FriendicaUserExtended>();
            // we need to loop through the ordered list as types are incompatible
            foreach (var user in usersOrdered)
                list.Add(user);
            return list;
        }
        
    }
}
