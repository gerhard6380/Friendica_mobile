using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Friendica_Mobile.Viewmodels
{
    // public enumeration lists to be used in whole PCL
    public enum ContactTypes { Friends, Forums, Groups }
    public enum PostTypes { UserGenerated, Newsfeed }
    public enum FriendicaActivity { like, dislike, unlike, undislike };
    public enum MessageErrors { OK, NoMailsAvailable, MessageIdNotSpecified, MessageSetToSeen, MessageIdOrParentUriNotSpecified, MessageDeleted, MessageIdNotInDatabase, ParentUriNotInDatabase, SearchstringNotSpecified, UnknownError };


    public abstract class BindableClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }


        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
