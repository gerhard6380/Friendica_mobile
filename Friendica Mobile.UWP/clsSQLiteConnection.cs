using Friendica_Mobile.UWP.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.UWP
{
    public class clsSQLiteConnection
    {
        private static SQLiteConnection _conn;
        public enum SQLTypes { String, Integer, Boolean };


        public clsSQLiteConnection()
        {
            // initializing sqlite database
            try
            {
                _conn = new SQLiteConnection("friendicaMobile.db");
            }
            catch (Exception ex)
            {
                MessageOnFailure(ex);
            }

            // create Table "tblAllKnownUsers"
            string sql = @"CREATE TABLE IF NOT EXISTS
                                tblAllKnownUsers  (Id                       INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                                UserId                      VARCHAR(10),
                                                UserIdStr                   VARCHAR(10),
                                                UserName                    VARCHAR(50), 
                                                UserScreenName              VARCHAR(50),
                                                UserLocation                VARCHAR(50),
                                                UserDescription             VARCHAR(255),
                                                UserProfileImageUrl         VARCHAR(255),
                                                UserProfileImageUrlHttps    VARCHAR(255),
                                                UserUrl                     VARCHAR(255),
                                                UserProtected               INT,
                                                UserFollowersCount          INT,
                                                UserFriendsCount            INT,
                                                UserCreatedAt               VARCHAR(50),
                                                UserFavouritesCount         INT,
                                                UserUtcOffset               INT,
                                                UserTimeZone                VARCHAR(50),
                                                UserStatusesCount           INT,
                                                UserFollowing               BOOLEAN,
                                                UserVerified                BOOLEAN,
                                                UserStatusnetBlocking       BOOLEAN,
                                                UserNotifications           BOOLEAN,
                                                UserStatusnetProfileUrl     VARCHAR(255),
                                                UserCid                     INT,
                                                UserNetwork                 VARCHAR(50)
                                                );";

            var isSuccess = ExecuteStatement(sql);

            // weitere Tabellen hier einfügen
        }

        private async void MessageOnFailure(Exception ex)
        {
            var errorMsg = String.Format("Error on creating sql database for Friendica Mobile\n\nMessage:\n{0}\n\nSource:\n{1}\n\nStack Trace:\n{2}", ex.Message, ex.Source, ex.StackTrace);
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }

        private bool ExecuteStatement(string sql)
        {
            // execute statements without returns like CREATE TABLE, 
            using (var statement = _conn.Prepare(sql))
            {
                try
                {
                    if (statement.Step() == SQLiteResult.DONE)
                        return true;
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
        }


        public FriendicaUser SelectSingleFromtblAllKnownUsers(string url)
        {
            try
            {
                using (var stmtSelect = _conn.Prepare("SELECT * FROM tblAllKnownUsers WHERE UserUrl = ?"))
                {
                    stmtSelect.Bind(1, url);
                    var friendicaUser = new FriendicaUser();

                    while (stmtSelect.Step() == SQLiteResult.ROW)
                    {
                        //friendicaUser.UserName = stmtSelect.GetText("UserName");

                        TransferAllKnownUsersToFriendicaUser(stmtSelect, friendicaUser);
                        return friendicaUser;
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private FriendicaUser TransferAllKnownUsersToFriendicaUser(ISQLiteStatement stmtSelect, FriendicaUser friendicaUser)
        {
            friendicaUser.UserId = (string)TransformFromSQLite(stmtSelect, "UserId", SQLTypes.String);
            friendicaUser.UserIdStr = (string)TransformFromSQLite(stmtSelect, "UserIdStr", SQLTypes.String);
            friendicaUser.UserName = (string)TransformFromSQLite(stmtSelect, "UserName", SQLTypes.String);
            friendicaUser.UserScreenName = (string)TransformFromSQLite(stmtSelect, "UserScreenName", SQLTypes.String);
            friendicaUser.UserLocation = (string)TransformFromSQLite(stmtSelect, "UserLocation", SQLTypes.String);
            friendicaUser.UserDescription = (string)TransformFromSQLite(stmtSelect, "UserDescription", SQLTypes.String);
            friendicaUser.UserProfileImageUrl = (string)TransformFromSQLite(stmtSelect, "UserProfileImageUrl", SQLTypes.String);
            friendicaUser.UserProfileImageUrlHttps = (string)TransformFromSQLite(stmtSelect, "UserProfileImageUrlHttps", SQLTypes.String);
            friendicaUser.UserUrl = (string)TransformFromSQLite(stmtSelect, "UserUrl", SQLTypes.String);
            friendicaUser.UserProtected = (bool)TransformFromSQLite(stmtSelect, "UserProtected", SQLTypes.Boolean);
            friendicaUser.UserFollowersCount = (long)TransformFromSQLite(stmtSelect, "UserFollowersCount", SQLTypes.Integer);
            friendicaUser.UserFriendsCount = (long)TransformFromSQLite(stmtSelect, "UserFriendsCount", SQLTypes.Integer);
            friendicaUser.UserCreatedAt = (string)TransformFromSQLite(stmtSelect, "UserCreatedAt", SQLTypes.String);
            friendicaUser.UserFavouritesCount = (long)TransformFromSQLite(stmtSelect, "UserFavouritesCount", SQLTypes.Integer);
            friendicaUser.UserUtcOffset = (long)TransformFromSQLite(stmtSelect, "UserUtcOffset", SQLTypes.Integer);
            friendicaUser.UserTimeZone = (string)TransformFromSQLite(stmtSelect, "UserTimeZone", SQLTypes.String);
            friendicaUser.UserStatusesCount = (long)TransformFromSQLite(stmtSelect, "UserStatusesCount", SQLTypes.Integer);
            friendicaUser.UserFollowing = (bool)TransformFromSQLite(stmtSelect, "UserFollowing", SQLTypes.Boolean);
            friendicaUser.UserVerified = (bool)TransformFromSQLite(stmtSelect, "UserVerified", SQLTypes.Boolean);
            friendicaUser.UserStatusnetBlocking = (bool)TransformFromSQLite(stmtSelect, "UserStatusnetBlocking", SQLTypes.Boolean);
            friendicaUser.UserNotifications = (bool)TransformFromSQLite(stmtSelect, "UserNotifications", SQLTypes.Boolean);
            friendicaUser.UserStatusnetProfileUrl = (string)TransformFromSQLite(stmtSelect, "UserStatusnetProfileUrl", SQLTypes.String);
            friendicaUser.UserCid = (long)TransformFromSQLite(stmtSelect, "UserCid", SQLTypes.Integer);
            friendicaUser.UserNetwork = (string)TransformFromSQLite(stmtSelect, "UserNetwork", SQLTypes.String);
            return friendicaUser;
        }

        public List<FriendicaUser> SelectAllFromtblAllKnownUsers()
        {
            List<FriendicaUser> userList = new List<FriendicaUser>();
            using (var stmtSelect = _conn.Prepare("SELECT * FROM tblAllKnownUsers"))
            {
                while (stmtSelect.Step() == SQLiteResult.ROW)
                {
                    var friendicaUser = new FriendicaUser();

                    friendicaUser.UserName = stmtSelect.GetText("UserName");

                    TransferAllKnownUsersToFriendicaUser(stmtSelect, friendicaUser);
                    userList.Add(friendicaUser);
                }
                return userList;
            }
        }

        public bool InsertIntotblAllKnownUsers(FriendicaUser friendicaUser)
        {
            try
            {
                using (var stmtInsert = _conn.Prepare(@"INSERT INTO tblAllKnownUsers (UserId, UserIdStr, UserName, UserScreenName, 
                                        UserLocation, UserDescription, UserProfileImageUrl, UserProfileImageUrlHttps, UserUrl,
                                        UserProtected, UserFollowersCount, UserFriendsCount, UserCreatedAt, UserFavouritesCount,
                                        UserUtcOffset, UserTimeZone, UserStatusesCount, UserFollowing, UserVerified, 
                                        UserStatusnetBlocking, UserNotifications, UserStatusnetProfileUrl, UserCid, UserNetwork) 
                                        VALUES (?, ?, ?, ?, 
                                        ?, ?, ?, ?, ?,
                                        ?, ?, ?, ?, ?,
                                        ?, ?, ?, ?, ?,
                                        ?, ?, ?, ?, ?)"))
                {
                    stmtInsert.Bind(1, TransformToSQLite(friendicaUser.UserId));
                    stmtInsert.Bind(2, TransformToSQLite(friendicaUser.UserIdStr));
                    stmtInsert.Bind(3, TransformToSQLite(friendicaUser.UserName));
                    stmtInsert.Bind(4, TransformToSQLite(friendicaUser.UserScreenName));
                    stmtInsert.Bind(5, TransformToSQLite(friendicaUser.UserLocation));
                    stmtInsert.Bind(6, TransformToSQLite(friendicaUser.UserDescription));
                    stmtInsert.Bind(7, TransformToSQLite(friendicaUser.UserProfileImageUrl));
                    stmtInsert.Bind(8, TransformToSQLite(friendicaUser.UserProfileImageUrlHttps));
                    stmtInsert.Bind(9, TransformToSQLite(friendicaUser.UserUrl));
                    stmtInsert.Bind(10, TransformToSQLite(friendicaUser.UserProtected));
                    stmtInsert.Bind(11, TransformToSQLite(friendicaUser.UserFollowersCount));
                    stmtInsert.Bind(12, TransformToSQLite(friendicaUser.UserFriendsCount));
                    stmtInsert.Bind(13, TransformToSQLite(friendicaUser.UserCreatedAt));
                    stmtInsert.Bind(14, TransformToSQLite(friendicaUser.UserFavouritesCount));
                    stmtInsert.Bind(15, TransformToSQLite(friendicaUser.UserUtcOffset));
                    stmtInsert.Bind(16, TransformToSQLite(friendicaUser.UserTimeZone));
                    stmtInsert.Bind(17, TransformToSQLite(friendicaUser.UserStatusesCount));
                    stmtInsert.Bind(18, TransformToSQLite(friendicaUser.UserFollowing));
                    stmtInsert.Bind(19, TransformToSQLite(friendicaUser.UserVerified));
                    stmtInsert.Bind(20, TransformToSQLite(friendicaUser.UserStatusnetBlocking));
                    stmtInsert.Bind(21, TransformToSQLite(friendicaUser.UserNotifications));
                    stmtInsert.Bind(22, TransformToSQLite(friendicaUser.UserStatusnetProfileUrl));
                    stmtInsert.Bind(23, TransformToSQLite(friendicaUser.UserCid));
                    stmtInsert.Bind(24, TransformToSQLite(friendicaUser.UserNetwork));
                    stmtInsert.Step();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TruncatetblAllKnownUser()
        {
            // initializing sqlite database
            _conn = new SQLiteConnection("friendicaMobile.db");

            // delete Table "tblAllKnownUsers"
            string sql = @"DELETE FROM tblAllKnownUsers;";
            var isSuccess = ExecuteStatement(sql);
            sql = @"VACUUM;";
            isSuccess = ExecuteStatement(sql);

            return isSuccess;
        }


        private object TransformToSQLite(object value)
        {
            if (value == null)
                return null;

            var type = value.GetType();
            
            if (type == typeof(bool))
            {
                bool valueBool = (bool)value;
                int valueInt;
                if (valueBool)
                    valueInt = 1;
                else
                    valueInt = 0;
                return valueInt;
            }
            else
                return value;
        }

        private object TransformFromSQLite(ISQLiteStatement stmtSelect, string column, SQLTypes type)
        {
            switch (type)
            {
                case SQLTypes.Boolean:
                    try
                    {
                        var value = stmtSelect.GetInteger(column);
                        if ((int)value == 1)
                            return true;
                        else
                            return false;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                case SQLTypes.Integer:
                    try
                    {
                        var value = stmtSelect.GetInteger(column);
                        return value;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                case SQLTypes.String:
                    try
                    {
                        var value = stmtSelect.GetText(column);
                        return value;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }
    }
}
