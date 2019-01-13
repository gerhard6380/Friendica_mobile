using Newtonsoft.Json;
using System.Collections.Generic;

namespace Friendica_Mobile.Models
{
    public class JsonFriendicaActivities
    {
        private List<JsonFriendicaUser> _activitiesLike;
        private List<JsonFriendicaUser> _activitiesDislike;
        private List<JsonFriendicaUser> _activitiesAttendYes;
        private List<JsonFriendicaUser> _activitiesAttendNo;
        private List<JsonFriendicaUser> _activitiesAttendMaybe;


        [JsonProperty("like")]
        public List<JsonFriendicaUser> ActivitiesLike
        {
            get { return _activitiesLike; }
            set { _activitiesLike = value; }
        }

        [JsonProperty("dislike")]
        public List<JsonFriendicaUser> ActivitiesDislike
        {
            get { return _activitiesDislike; }
            set { _activitiesDislike = value; }
        }

        [JsonProperty("attendyes")]
        public List<JsonFriendicaUser> ActivitiesAttendYes
        {
            get { return _activitiesAttendYes; }
            set { _activitiesAttendYes = value; }
        }

        [JsonProperty("attendno")]
        public List<JsonFriendicaUser> ActivitiesAttendNo
        {
            get { return _activitiesAttendNo; }
            set { _activitiesAttendNo = value; }
        }

        [JsonProperty("attendmaybe")]
        public List<JsonFriendicaUser> ActivitiesAttendMaybe
        {
            get { return _activitiesAttendMaybe; }
            set { _activitiesAttendMaybe = value; }
        }


        public JsonFriendicaActivities()
        {
            ActivitiesLike = new List<JsonFriendicaUser>();
            ActivitiesDislike = new List<JsonFriendicaUser>();
            ActivitiesAttendYes = new List<JsonFriendicaUser>();
            ActivitiesAttendNo = new List<JsonFriendicaUser>();
            ActivitiesAttendMaybe = new List<JsonFriendicaUser>();
        }

    }
}
