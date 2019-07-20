using Newtonsoft.Json;
using System.Collections.Generic;

namespace Friendica_Mobile.Models
{
    public class FriendicaActivities
    {
        public enum FriendicaActivity { like, dislike, unlike, undislike };

        private List<FriendicaUser> _activitiesLike;
        private List<FriendicaUser> _activitiesDislike;
        private List<FriendicaUser> _activitiesAttendYes;
        private List<FriendicaUser> _activitiesAttendNo;
        private List<FriendicaUser> _activitiesAttendMaybe;


        [JsonProperty("like")]
        public List<FriendicaUser> ActivitiesLike
        {
            get { return _activitiesLike; }
            set { _activitiesLike = value; }
        }

        [JsonProperty("dislike")]
        public List<FriendicaUser> ActivitiesDislike
        {
            get { return _activitiesDislike; }
            set { _activitiesDislike = value; }
        }

        [JsonProperty("attendyes")]
        public List<FriendicaUser> ActivitiesAttendYes
        {
            get { return _activitiesAttendYes; }
            set { _activitiesAttendYes = value; }
        }

        [JsonProperty("attendno")]
        public List<FriendicaUser> ActivitiesAttendNo
        {
            get { return _activitiesAttendNo; }
            set { _activitiesAttendNo = value; }
        }

        [JsonProperty("attendmaybe")]
        public List<FriendicaUser> ActivitiesAttendMaybe
        {
            get { return _activitiesAttendMaybe; }
            set { _activitiesAttendMaybe = value; }
        }


        public FriendicaActivities()
        {
            ActivitiesLike = new List<FriendicaUser>();
            ActivitiesDislike = new List<FriendicaUser>();
            ActivitiesAttendYes = new List<FriendicaUser>();
            ActivitiesAttendNo = new List<FriendicaUser>();
            ActivitiesAttendMaybe = new List<FriendicaUser>();
        }

    }
}
