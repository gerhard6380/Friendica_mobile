using System.Collections.Generic;

namespace Friendica_Mobile.PCL.Models
{
    public class FriendicaActivities
    {
        private List<FriendicaUser> _activitiesLike;
        private List<FriendicaUser> _activitiesDislike;
        private List<FriendicaUser> _activitiesAttendYes;
        private List<FriendicaUser> _activitiesAttendNo;
        private List<FriendicaUser> _activitiesAttendMaybe;


        public List<FriendicaUser> ActivitiesLike
        {
            get { return _activitiesLike; }
            set { _activitiesLike = value; }
        }

        public List<FriendicaUser> ActivitiesDislike
        {
            get { return _activitiesDislike; }
            set { _activitiesDislike = value; }
        }

        public List<FriendicaUser> ActivitiesAttendYes
        {
            get { return _activitiesAttendYes; }
            set { _activitiesAttendYes = value; }
        }

        public List<FriendicaUser> ActivitiesAttendNo
        {
            get { return _activitiesAttendNo; }
            set { _activitiesAttendNo = value; }
        }

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

        public FriendicaActivities(JsonFriendicaActivities activities)
        {
            ActivitiesLike = new List<FriendicaUser>();
            ActivitiesDislike = new List<FriendicaUser>();
            ActivitiesAttendYes = new List<FriendicaUser>();
            ActivitiesAttendNo = new List<FriendicaUser>();
            ActivitiesAttendMaybe = new List<FriendicaUser>();

            foreach (var user in activities.ActivitiesLike)
                ActivitiesLike.Add(new FriendicaUser(user));

            foreach (var user in activities.ActivitiesDislike)
                ActivitiesDislike.Add(new FriendicaUser(user));

            foreach (var user in activities.ActivitiesAttendYes)
                ActivitiesAttendYes.Add(new FriendicaUser(user));

            foreach (var user in activities.ActivitiesAttendNo)
                ActivitiesAttendNo.Add(new FriendicaUser(user));

            foreach (var user in activities.ActivitiesAttendMaybe)
                ActivitiesAttendMaybe.Add(new FriendicaUser(user));
        }

    }
}
