using Friendica_Mobile.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Friendica_Mobile
{
    public static class StaticSQLiteConn
    {
        private static SQLiteAsyncConnection _conn;
        public enum SQLTypes { String, Integer, Boolean };


        public static async void InitializeDatabase(SQLiteAsyncConnection connection)
        {
            // initializing sqlite database
            try
            {
                _conn = connection;
            }
            catch (Exception ex)
            {
                var errorMsg = String.Format("Error on creating sql database for Friendica Mobile\n\nMessage:\n{0}\n\nSource:\n{1}\n\nStack Trace:\n{2}", ex.Message, ex.Source, ex.StackTrace);
                await StaticMessageDialog.ShowDialogAsync("SQLite database", errorMsg, "OK");
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

            var isSuccess = await ExecuteStatementAsync(sql);

            // weitere Tabellen hier einfügen
        }


        private static async Task<bool> ExecuteStatementAsync(string sql)
        {
            // execute statements without returns like CREATE TABLE, 
            var result = await _conn.ExecuteAsync(sql);
            if (result == 1)
                return true;
            else
                return false;
        }


        public static async Task<List<JsonFriendicaUser>> SelectSingleFromtblAllKnownUsersAsync(string url)
        {
            try
            {
                var result = await _conn.QueryAsync<JsonFriendicaUser>(String.Format("SELECT * FROM tblAllKnownUsers WHERE UserUrl = {0}", url));
                return result;
            }
            catch
            {
                return null;
            }
        }


        public static async Task<List<JsonFriendicaUser>> SelectAllFromtblAllKnownUsersAsync()
        {
            List<FriendicaUser> userList = new List<FriendicaUser>();
            var result = await _conn.QueryAsync<JsonFriendicaUser>("SELECT * FROM tblAllKnownUsers");
            return result;
        }


        public static async Task<bool> InsertIntotblAllKnownUsersAsync(JsonFriendicaUser friendicaUser)
        {
            try
            {
                var result = await _conn.InsertAsync(friendicaUser);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static async Task<bool> TruncatetblAllKnownUserAsync()
        {
            // delete Table "tblAllKnownUsers"
            string sql = @"DELETE FROM tblAllKnownUsers;";
            var isSuccess = await _conn.ExecuteAsync(sql);
            sql = @"VACUUM;";
            isSuccess = await _conn.ExecuteAsync(sql);

            if (isSuccess == 1)
                return true;
            else
                return false;
        }


    }
}
