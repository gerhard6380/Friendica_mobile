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
    public class FriendicaSite
    {
        private const string siteNameKey = "name";
        private const string siteServerKey = "server";
        private const string siteThemeKey = "theme";
        private const string sitePathKey = "path";
        private const string siteLogoKey = "logo";
        private const string siteFancyKey = "fancy";
        private const string siteLanguageKey = "language";
        private const string siteEmailKey = "email";
        private const string siteBroughtbyKey = "broughtby";
        private const string siteBroughtbyurlKey = "brougthbyurl";
        private const string siteTimezoneKey = "timezone";
        private const string siteClosedKey = "closed";
        private const string siteInviteonlyKey = "inviteonly";
        private const string sitePrivateKey = "private";
        private const string siteTextlimitKey = "textlimit";
        private const string siteSslserverKey = "sslserver";
        private const string siteSslKey = "ssl";
        private const string siteShorturllengthKey = "shorturllength";
        private const string siteFriendicaKey = "friendica";

        private string _siteName;
        private string _siteServer;
        private string _siteTheme;
        private string _sitePath;
        private string _siteLogo;
        private bool _siteFancy;
        private string _siteLanguage;
        private string _siteEmail;
        private string _siteBroughtby;
        private string _siteBroughtbyurl;
        private string _siteTimezone;
        private string _siteClosed;
        private bool _siteInviteonly;
        private string _sitePrivate;
        private string _siteTextlimit;
        private string _siteSslserver;
        private string _siteSsl;
        private string _siteShorturllength;
        private FriendicaServer _siteFriendica;

        public FriendicaSite()
        {
            SiteName = "";
            SiteServer = "";
            SiteTheme = "";
            SitePath = "";
            SiteLogo = "";
            SiteFancy = false;
            SiteLanguage = "";
            SiteEmail = "";
            SiteBroughtby = "";
            SiteBroughtbyurl = "";
            SiteTimezone = "";
            SiteClosed = "";
            SiteInviteonly = false;
            SitePrivate = "";
            SiteTextlimit = "";
            SiteSslserver = "";
            SiteSsl = "";
            SiteShorturllength = "";
            SiteFriendica = new FriendicaServer();
        }


        public FriendicaSite(JsonObject jsonObject) : this()
        {
            SiteName = jsonObject.GetNamedString(siteNameKey, "");
            SiteServer = jsonObject.GetNamedString(siteServerKey, "");
            SiteTheme = jsonObject.GetNamedString(siteThemeKey, "");
            SitePath = jsonObject.GetNamedString(sitePathKey, "");
            SiteLogo = jsonObject.GetNamedString(siteLogoKey, "");
            SiteFancy = jsonObject.GetNamedBoolean(siteFancyKey, false);
            SiteLanguage = jsonObject.GetNamedString(siteLanguageKey, "");
            SiteEmail = jsonObject.GetNamedString(siteEmailKey, "");
            SiteBroughtby = jsonObject.GetNamedString(siteBroughtbyKey, "");
            SiteBroughtbyurl = jsonObject.GetNamedString(siteBroughtbyurlKey, "");
            SiteTimezone = jsonObject.GetNamedString(siteTimezoneKey, "");
            SiteClosed = jsonObject.GetNamedString(siteClosedKey, "");
            SiteInviteonly = jsonObject.GetNamedBoolean(siteInviteonlyKey, false);
            SitePrivate = jsonObject.GetNamedString(sitePrivateKey, "");
            SiteTextlimit = jsonObject.GetNamedString(siteTextlimitKey, "");
            SiteSslserver = jsonObject.GetNamedString(siteSslserverKey, "");
            SiteSsl = jsonObject.GetNamedString(siteSslKey, "");
            SiteShorturllength = jsonObject.GetNamedString(siteShorturllengthKey, "");
            SiteFriendica = new FriendicaServer(jsonObject.GetNamedObject(siteFriendicaKey));
        }

        public string Stringify()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject[siteNameKey] = JsonValue.CreateStringValue(SiteName);
            jsonObject[siteServerKey] = JsonValue.CreateStringValue(SiteServer);
            jsonObject[siteThemeKey] = JsonValue.CreateStringValue(SiteTheme);
            jsonObject[sitePathKey] = JsonValue.CreateStringValue(SitePath);
            jsonObject[siteLogoKey] = JsonValue.CreateStringValue(SiteLogo);
            jsonObject[siteFancyKey] = JsonValue.CreateBooleanValue(SiteFancy);
            jsonObject[siteLanguageKey] = JsonValue.CreateStringValue(SiteLanguage);
            jsonObject[siteEmailKey] = JsonValue.CreateStringValue(SiteEmail);
            jsonObject[siteBroughtbyKey] = JsonValue.CreateStringValue(SiteBroughtby);
            jsonObject[siteBroughtbyurlKey] = JsonValue.CreateStringValue(SiteBroughtbyurl);
            jsonObject[siteTimezoneKey] = JsonValue.CreateStringValue(SiteTimezone);
            jsonObject[siteClosedKey] = JsonValue.CreateStringValue(SiteClosed);
            jsonObject[siteInviteonlyKey] = JsonValue.CreateBooleanValue(SiteInviteonly);
            jsonObject[sitePrivateKey] = JsonValue.CreateStringValue(SitePrivate);
            jsonObject[siteTextlimitKey] = JsonValue.CreateStringValue(SiteTextlimit);
            jsonObject[siteSslserverKey] = JsonValue.CreateStringValue(SiteSslserver);
            jsonObject[siteSslKey] = JsonValue.CreateStringValue(SiteSsl);
            jsonObject[siteShorturllengthKey] = JsonValue.CreateStringValue(SiteShorturllength);
            jsonObject[siteFriendicaKey] = SiteFriendica.ToJsonObject();

            return jsonObject.Stringify();
        }

        public JsonObject ToJsonObject()
        {
            JsonObject friendicaSiteObject = new JsonObject();
            friendicaSiteObject.SetNamedValue(siteNameKey, JsonValue.CreateStringValue(SiteName));
            friendicaSiteObject.SetNamedValue(siteServerKey, JsonValue.CreateStringValue(SiteServer));
            friendicaSiteObject.SetNamedValue(siteThemeKey, JsonValue.CreateStringValue(SiteTheme));
            friendicaSiteObject.SetNamedValue(sitePathKey, JsonValue.CreateStringValue(SitePath));
            friendicaSiteObject.SetNamedValue(siteLogoKey, JsonValue.CreateStringValue(SiteLogo));
            friendicaSiteObject.SetNamedValue(siteFancyKey, JsonValue.CreateBooleanValue(SiteFancy));
            friendicaSiteObject.SetNamedValue(siteLanguageKey, JsonValue.CreateStringValue(SiteLanguage));
            friendicaSiteObject.SetNamedValue(siteEmailKey, JsonValue.CreateStringValue(SiteEmail));
            friendicaSiteObject.SetNamedValue(siteBroughtbyKey, JsonValue.CreateStringValue(SiteBroughtby));
            friendicaSiteObject.SetNamedValue(siteBroughtbyurlKey, JsonValue.CreateStringValue(SiteBroughtbyurl));
            friendicaSiteObject.SetNamedValue(siteTimezoneKey, JsonValue.CreateStringValue(SiteTimezone));
            friendicaSiteObject.SetNamedValue(siteClosedKey, JsonValue.CreateStringValue(SiteClosed));
            friendicaSiteObject.SetNamedValue(siteInviteonlyKey, JsonValue.CreateBooleanValue(SiteInviteonly));
            friendicaSiteObject.SetNamedValue(sitePrivateKey, JsonValue.CreateStringValue(SitePrivate));
            friendicaSiteObject.SetNamedValue(siteTextlimitKey, JsonValue.CreateStringValue(SiteTextlimit));
            friendicaSiteObject.SetNamedValue(siteSslserverKey, JsonValue.CreateStringValue(SiteSslserver));
            friendicaSiteObject.SetNamedValue(siteSslKey, JsonValue.CreateStringValue(SiteSsl));
            friendicaSiteObject.SetNamedValue(siteShorturllengthKey, JsonValue.CreateStringValue(SiteShorturllength));
            FriendicaServer friendicaServer = new FriendicaServer();
            friendicaSiteObject.SetNamedValue(siteFriendicaKey, friendicaServer.ToJsonObject());
            return friendicaSiteObject;
        }

        public string SiteName
        {
            get { return _siteName; }
            set { _siteName = value; }
        }

        public string SiteServer
        {
            get { return _siteServer; }
            set { _siteServer = value; }
        }

        public string SiteTheme
        {
            get { return _siteTheme; }
            set { _siteTheme = value; }
        }

        public string SitePath
        {
            get { return _sitePath; }
            set { _sitePath = value; }
        }

        public string SiteLogo
        {
            get { return _siteLogo; }
            set { _siteLogo = value; }
        }

        public bool SiteFancy
        {
            get { return _siteFancy; }
            set { _siteFancy = value; }
        }

        public string SiteLanguage
        {
            get { return _siteLanguage; }
            set { _siteLanguage = value; }
        }

        public string SiteEmail
        {
            get { return _siteEmail; }
            set { _siteEmail = value; }
        }

        public string SiteBroughtby
        {
            get { return _siteBroughtby; }
            set { _siteBroughtby = value; }
        }

        public string SiteBroughtbyurl
        {
            get { return _siteBroughtbyurl; }
            set { _siteBroughtbyurl = value; }
        }

        public string SiteTimezone
        {
            get { return _siteTimezone; }
            set { _siteTimezone = value; }
        }

        public string SiteClosed
        {
            get { return _siteClosed; }
            set { _siteClosed = value; }
        }

        public bool SiteInviteonly
        {
            get { return _siteInviteonly; }
            set { _siteInviteonly = value; }
        }

        public string SitePrivate
        {
            get { return _sitePrivate; }
            set { _sitePrivate = value; }
        }

        public string SiteTextlimit
        {
            get { return _siteTextlimit; }
            set { _siteTextlimit = value; }
        }

        public string SiteSslserver
        {
            get { return _siteSslserver; }
            set { _siteSslserver = value; }
        }

        public string SiteSsl
        {
            get { return _siteSsl; }
            set { _siteSsl = value; }
        }

        public string SiteShorturllength
        {
            get { return _siteShorturllength; }
            set { _siteShorturllength = value; }
        }

        public FriendicaServer SiteFriendica
        {
            get { return _siteFriendica; }
            set { _siteFriendica = value; }
        }
    }
}
