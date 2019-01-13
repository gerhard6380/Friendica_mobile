namespace Friendica_Mobile.PCL
{
    public static class CheckSettings
    {
        // check if we have a defined server url otherwise we are in samples mode
        public static bool ServerSettingsAvailable()
        {
            if (Settings.FriendicaServer == "" || Settings.FriendicaServer == "http://"
                    || Settings.FriendicaServer == "https://" || Settings.FriendicaServer == null
                    || Settings.FriendicaUsername == null || Settings.FriendicaPassword == null
                    || Settings.FriendicaUsername == "" || Settings.FriendicaPassword == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
