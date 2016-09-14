using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Friendica_Mobile.Models
{
    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public class FriendicaServer
    {
        private const string friendicaPlatformKey = "FRIENDICA_PLATFORM";
        private const string friendicaVersionKey = "FRIENDICA_VERSION";
        private const string dfrnProtocolVersionKey = "DFRN_PROTOCOL_VERSION";
        private const string dbUpdateVersionKey = "DB_UPDATE_VERSION";

        private string _friendicaPlatform;
        private string _friendicaVersion;
        private string _dfrnProtocolVersion;
        private double _dbUpdateVersion;

        public FriendicaServer()
        {
            FriendicaPlatform = "";
            FriendicaVersion = "";
            DfrnProtocolVersion = "";
            DbUpdateVersion = 0;
        }

        public FriendicaServer(JsonObject jsonObject)
        {
            FriendicaPlatform = jsonObject.GetNamedString(friendicaPlatformKey);
            FriendicaVersion = jsonObject.GetNamedString(friendicaVersionKey);
            DfrnProtocolVersion = jsonObject.GetNamedString(dfrnProtocolVersionKey);
            DbUpdateVersion = jsonObject.GetNamedNumber(dbUpdateVersionKey);
        }

        public JsonObject ToJsonObject()
        {
            JsonObject friendicaServerObject = new JsonObject();
            friendicaServerObject.SetNamedValue(friendicaPlatformKey, JsonValue.CreateStringValue(FriendicaPlatform));
            friendicaServerObject.SetNamedValue(friendicaVersionKey, JsonValue.CreateStringValue(FriendicaVersion));
            friendicaServerObject.SetNamedValue(dfrnProtocolVersionKey, JsonValue.CreateStringValue(DfrnProtocolVersion));
            friendicaServerObject.SetNamedValue(dbUpdateVersionKey, JsonValue.CreateNumberValue(DbUpdateVersion));

            return friendicaServerObject;
        }

        public string FriendicaPlatform
        {
            get
            {
                return _friendicaPlatform;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _friendicaPlatform = value;
            }
        }

        public string FriendicaVersion
        {
            get
            {
                return _friendicaVersion;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _friendicaVersion = value;
            }
        }

        public string DfrnProtocolVersion
        {
            get
            {
                return _dfrnProtocolVersion;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _dfrnProtocolVersion = value;
            }
        }

        public double DbUpdateVersion
        {
            get
            {
                return _dbUpdateVersion;
            }
            set
            {
                _dbUpdateVersion = value;
            }
        }

    }
}
