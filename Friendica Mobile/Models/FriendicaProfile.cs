using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Xaml.Documents;

namespace Friendica_Mobile.Models
{
    public class FriendicaProfile
    {
        public enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser, FriendicaGeo };

        // API interface properties
        private const string profileIdKey = "profile_id";
        private const string profileNameKey = "profile_name";
        private const string profileIsDefaultKey = "is_default";
        private const string profileHideFriendsKey = "hide_friends";
        private const string profilePhotoKey = "profile_photo";
        private const string profileThumbKey = "profile_thumb";
        private const string profilePublishKey = "publish";
        private const string profileNetPublishKey = "net_publish";
        private const string profileDescriptionKey = "description";
        private const string profileDateOfBirthKey = "date_of_birth";
        private const string profileAddressKey = "address";
        private const string profileCityKey = "city";
        private const string profileRegionKey = "region";
        private const string profilePostalCodeKey = "postal_code";
        private const string profileCountryKey = "country";
        private const string profileHometownKey = "hometown";
        private const string profileGenderKey = "gender";
        private const string profileMaritalKey = "marital";
        private const string profileMaritalWithKey = "marital_with";
        private const string profileMaritalSinceKey = "marital_since";
        private const string profileSexualKey = "sexual";
        private const string profilePoliticKey = "politic";
        private const string profileReligionKey = "religion";
        private const string profilePublicKeywordsKey = "public_keywords";
        private const string profilePrivateKeywordsKey = "private_keywords";
        private const string profileLikesKey = "likes";
        private const string profileDislikesKey = "dislikes";
        private const string profileAboutKey = "about";
        private const string profileMusicKey = "music";
        private const string profileBookKey = "book";
        private const string profileTvKey = "tv";
        private const string profileFilmKey = "film";
        private const string profileInterestKey = "interest";
        private const string profileRomanceKey = "romance";
        private const string profileWorkKey = "work";
        private const string profileEducationKey = "education";
        private const string profileSocialNetworksKey = "social_networks";
        private const string profileHomepageKey = "homepage";
        private const string profileUsersKey = "users";

        private string _profileId;
        private string _profileName;
        private bool _profileIsDefault;
        private bool _profileHideFriends;
        private string _profilePhoto;
        private string _profileThumb;
        private bool _profilePublish;
        private bool _profileNetPublish;
        private string _profileDescription;
        private string _profileDateOfBirth;
        private string _profileAddress;
        private string _profileCity;
        private string _profileRegion;
        private string _profilePostalCode;
        private string _profileCountry;
        private string _profileHometown;
        private string _profileGender;
        private string _profileMarital;
        private string _profileMaritalWith;
        private string _profileMaritalSince;
        private string _profileSexual;
        private string _profilePolitic;
        private string _profileReligion;
        private string _profilePublicKeywords;
        private string _profilePrivateKeywords;
        private string _profileLikes;
        private string _profileDislikes;
        private string _profileAbout;
        private string _profileMusic;
        private string _profileBook;
        private string _profileTv;
        private string _profileFilm;
        private string _profileInterest;
        private string _profileRomance;
        private string _profileWork;
        private string _profileEducation;
        private string _profileSocialNetworks;
        private string _profileHomepage;
        private List<FriendicaUser> _profileUsers;


        // additional derived properties
        private DateTime _profileDateOfBirthDateTime;
        public DateTime ProfileDateOfBirthDateTime
        {
            get { return _profileDateOfBirthDateTime; }
            set { _profileDateOfBirthDateTime = value; }
        }

        private DateTime _profileMaritalSinceDateTime;
        public DateTime ProfileMaritalSinceDateTime
        {
            get { return _profileMaritalSinceDateTime; }
            set { _profileMaritalSinceDateTime = value; }
        }

        private Paragraph _profileLikesTransformed;
        public Paragraph ProfileLikesTransformed
        {
            get { return _profileLikesTransformed; }
            set { _profileLikesTransformed = value; }
        }

        private Paragraph _profileDislikesTransformed;
        public Paragraph ProfileDislikesTransformed
        {
            get { return _profileDislikesTransformed; }
            set { _profileDislikesTransformed = value; }
        }

        private Paragraph _profileAboutTransformed;
        public Paragraph ProfileAboutTransformed
        {
            get { return _profileAboutTransformed; }
            set { _profileAboutTransformed = value; }
        }

        private Paragraph _profileMusicTransformed;
        public Paragraph ProfileMusicTransformed
        {
            get { return _profileMusicTransformed; }
            set { _profileMusicTransformed = value; }
        }

        private Paragraph _profileBookTransformed;
        public Paragraph ProfileBookTransformed
        {
            get { return _profileBookTransformed; }
            set { _profileBookTransformed = value; }
        }

        private Paragraph _profileTvTransformed;
        public Paragraph ProfileTvTransformed
        {
            get { return _profileTvTransformed; }
            set { _profileTvTransformed = value; }
        }

        private Paragraph _profileFilmTransformed;
        public Paragraph ProfileFilmTransformed
        {
            get { return _profileFilmTransformed; }
            set { _profileFilmTransformed = value; }
        }

        private Paragraph _profileInterestTransformed;
        public Paragraph ProfileInterestTransformed
        {
            get { return _profileInterestTransformed; }
            set { _profileInterestTransformed = value; }
        }

        private Paragraph _profileRomanceTransformed;
        public Paragraph ProfileRomanceTransformed
        {
            get { return _profileRomanceTransformed; }
            set { _profileRomanceTransformed = value; }
        }

        private Paragraph _profileWorkTransformed;
        public Paragraph ProfileWorkTransformed
        {
            get { return _profileWorkTransformed; }
            set { _profileWorkTransformed = value; }
        }

        private Paragraph _profileEducationTransformed;
        public Paragraph ProfileEducationTransformed
        {
            get { return _profileEducationTransformed; }
            set { _profileEducationTransformed = value; }
        }

        private Paragraph _profileSocialNetworksTransformed;
        public Paragraph ProfileSocialNetworksTransformed
        {
            get { return _profileSocialNetworksTransformed; }
            set { _profileSocialNetworksTransformed = value; }
        }


        public FriendicaProfile()
        {
            ProfileId = "";
            ProfileName = "";
            ProfileIsDefault = false;
            ProfileHideFriends = true;
            ProfilePhoto = "";
            ProfileThumb = "";
            ProfilePublish = false;
            ProfileNetPublish = false;
            ProfileDescription = "";
            ProfileDateOfBirth = "";
            ProfileAddress = "";
            ProfileCity = "";
            ProfileRegion = "";
            ProfilePostalCode = "";
            ProfileCountry = "";
            ProfileHometown = "";
            ProfileGender = "";
            ProfileMarital = "";
            ProfileMaritalWith = "";
            ProfileMaritalSince = "";
            ProfileSexual = "";
            ProfilePolitic = "";
            ProfileReligion = "";
            ProfilePublicKeywords = "";
            ProfilePrivateKeywords = "";
            ProfileLikes = "";
            ProfileDislikes = "";
            ProfileAbout = "";
            ProfileMusic = "";
            ProfileBook = "";
            ProfileTv = "";
            ProfileFilm = "";
            ProfileInterest = "";
            ProfileRomance = "";
            ProfileWork = "";
            ProfileEducation = "";
            ProfileSocialNetworks = "";
            ProfileHomepage = "";
            ProfileUsers = new List<FriendicaUser>();
        }


    public FriendicaProfile(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            ProfileId = (string)CheckAttribute(jsonObject, profileIdKey, AttributeTypes.String);
            ProfileName = (string)CheckAttribute(jsonObject, profileNameKey, AttributeTypes.String);
            ProfileIsDefault = false;
            ProfileHideFriends = true;
            ProfilePhoto = (string)CheckAttribute(jsonObject, profilePhotoKey, AttributeTypes.String);
            ProfileThumb = (string)CheckAttribute(jsonObject, profileThumbKey, AttributeTypes.String);
            ProfilePublish = false;
            ProfileNetPublish = false;
            ProfileDescription = (string)CheckAttribute(jsonObject, profileDescriptionKey, AttributeTypes.String);
            ProfileDateOfBirth = (string)CheckAttribute(jsonObject, profileDateOfBirthKey, AttributeTypes.String);
            ProfileAddress = (string)CheckAttribute(jsonObject, profileAddressKey, AttributeTypes.String);
            ProfileCity = (string)CheckAttribute(jsonObject, profileCityKey, AttributeTypes.String);
            ProfileRegion = (string)CheckAttribute(jsonObject, profileRegionKey, AttributeTypes.String);
            ProfilePostalCode = (string)CheckAttribute(jsonObject, profilePostalCodeKey, AttributeTypes.String);
            ProfileCountry = (string)CheckAttribute(jsonObject, profileCountryKey, AttributeTypes.String);
            ProfileHometown = (string)CheckAttribute(jsonObject, profileHometownKey, AttributeTypes.String);
            ProfileGender = (string)CheckAttribute(jsonObject, profileGenderKey, AttributeTypes.String);
            ProfileMarital = (string)CheckAttribute(jsonObject, profileMaritalKey, AttributeTypes.String);
            ProfileMaritalWith = (string)CheckAttribute(jsonObject, profileMaritalWithKey, AttributeTypes.String);
            ProfileMaritalSince = (string)CheckAttribute(jsonObject, profileMaritalSinceKey, AttributeTypes.String);
            ProfileSexual = (string)CheckAttribute(jsonObject, profileSexualKey, AttributeTypes.String);
            ProfilePolitic = (string)CheckAttribute(jsonObject, profilePoliticKey, AttributeTypes.String);
            ProfileReligion = (string)CheckAttribute(jsonObject, profileRegionKey, AttributeTypes.String);
            ProfilePublicKeywords = (string)CheckAttribute(jsonObject, profilePublicKeywordsKey, AttributeTypes.String);
            ProfilePrivateKeywords = (string)CheckAttribute(jsonObject, profilePrivateKeywordsKey, AttributeTypes.String);
            ProfileLikes = (string)CheckAttribute(jsonObject, profileLikesKey, AttributeTypes.String);
            ProfileDislikes = (string)CheckAttribute(jsonObject, profileDislikesKey, AttributeTypes.String);
            ProfileAbout = (string)CheckAttribute(jsonObject, profileAboutKey, AttributeTypes.String);
            ProfileMusic = (string)CheckAttribute(jsonObject, profileMusicKey, AttributeTypes.String);
            ProfileBook = (string)CheckAttribute(jsonObject, profileBookKey, AttributeTypes.String);
            ProfileTv = (string)CheckAttribute(jsonObject, profileTvKey, AttributeTypes.String);
            ProfileFilm = (string)CheckAttribute(jsonObject, profileFilmKey, AttributeTypes.String);
            ProfileInterest = (string)CheckAttribute(jsonObject, profileInterestKey, AttributeTypes.String);
            ProfileRomance = (string)CheckAttribute(jsonObject, profileRomanceKey, AttributeTypes.String);
            ProfileWork = (string)CheckAttribute(jsonObject, profileWorkKey, AttributeTypes.String);
            ProfileEducation = (string)CheckAttribute(jsonObject, profileEducationKey, AttributeTypes.String);
            ProfileSocialNetworks = (string)CheckAttribute(jsonObject, profileSocialNetworksKey, AttributeTypes.String);
            ProfileHomepage = (string)CheckAttribute(jsonObject, profileHomepageKey, AttributeTypes.String);
            JsonArray array = jsonObject.GetNamedArray(profileUsersKey);
            foreach (var element in array)
                ProfileUsers.Add(new FriendicaUser(element.GetObject()));
        }

        private object CheckAttribute(JsonObject jsonObject, string key, AttributeTypes type)
        {
            IJsonValue value;
            jsonObject.TryGetValue(key, out value);
            switch (type)
            {
                case AttributeTypes.String:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return jsonObject.GetNamedString(key, "");
                case AttributeTypes.Number:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return 0.0;
                    else
                        return jsonObject.GetNamedNumber(key, 0);
                case AttributeTypes.Boolean:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return jsonObject.GetNamedBoolean(key, false);
                case AttributeTypes.FriendicaPost:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaPost(jsonObject.GetNamedObject(key));
                case AttributeTypes.FriendicaGeo:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaGeo(jsonObject.GetNamedObject(key));
                case AttributeTypes.FriendicaUser:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaUser(jsonObject.GetNamedObject(key));
                default:
                    return null;
            } 
        }


        public Paragraph ConvertHtmlToParagraph(string text)
        {
            // placeholder text shown if there is no text in message (very rare case)
            if (text != "")
            {
                var htmlToRichTextBlock = new clsHtmlToRichTextBlock(text);
                return htmlToRichTextBlock.ApplyHtmlToParagraph();
            }
            else
                return new Paragraph();
        }


        public string ProfileId
        {
            get { return _profileId; }
            set { _profileId = value; }
        }

        public string ProfileName
        {
            get { return _profileName; }
            set { _profileName = value; }
        }

        public bool ProfileIsDefault
        {
            get { return _profileIsDefault; }
            set { _profileIsDefault = value; }
        }

        public bool ProfileHideFriends
        {
            get { return _profileHideFriends; }
            set { _profileHideFriends = value; }
        }

        public string ProfilePhoto
        {
            get { return _profilePhoto; }
            set { _profilePhoto = value; }
        }

        public string ProfileThumb
        {
            get { return _profileThumb; }
            set { _profileThumb = value; }
        }

        public bool ProfilePublish
        {
            get { return _profilePublish; }
            set { _profilePublish = value; }
        }

        public bool ProfileNetPublish
        {
            get { return _profileNetPublish; }
            set { _profileNetPublish = value; }
        }

        public string ProfileDescription
        {
            get { return _profileDescription; }
            set { _profileDescription = value; }
        }

        public string ProfileDateOfBirth
        {
            get { return _profileDateOfBirth; }
            set { _profileDateOfBirth = value;
                if (value != "")
                    ProfileDateOfBirthDateTime = DateTime.ParseExact(value, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

        public string ProfileAddress
        {
            get { return _profileAddress; }
            set { _profileAddress = value; }
        }

        public string ProfileCity
        {
            get { return _profileCity; }
            set { _profileCity = value; }
        }

        public string ProfileRegion
        {
            get { return _profileRegion; }
            set { _profileRegion = value; }
        }

        public string ProfilePostalCode
        {
            get { return _profilePostalCode; }
            set { _profilePostalCode = value; }
        }

        public string ProfileCountry
        {
            get { return _profileCountry; }
            set { _profileCountry = value; }
        }

        public string ProfileHometown
        {
            get { return _profileHometown; }
            set { _profileHometown = value; }
        }

        public string ProfileGender
        {
            get { return _profileGender; }
            set { _profileGender = value; }
        }

        public string ProfileMarital
        {
            get { return _profileMarital; }
            set { _profileMarital = value; }
        }

        public string ProfileMaritalWith
        {
            get { return _profileMaritalWith; }
            set { _profileMaritalWith = value; }
        }

        public string ProfileMaritalSince
        {
            get { return _profileMaritalSince; }
            set { _profileMaritalSince = value;
                if (value != "")
                    ProfileMaritalSinceDateTime = DateTime.ParseExact(value, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public string ProfileSexual
        {
            get { return _profileSexual; }
            set { _profileSexual = value; }
        }

        public string ProfilePolitic
        {
            get { return _profilePolitic; }
            set { _profilePolitic = value; }
        }

        public string ProfileReligion
        {
            get { return _profileReligion; }
            set { _profileReligion = value; }
        }

        public string ProfilePublicKeywords
        {
            get { return _profilePublicKeywords; }
            set { _profilePublicKeywords = value; }
        }

        public string ProfilePrivateKeywords
        {
            get { return _profilePrivateKeywords; }
            set { _profilePrivateKeywords = value; }
        }

        public string ProfileLikes
        {
            get { return _profileLikes; }
            set { _profileLikes = value;
                ProfileLikesTransformed = ConvertHtmlToParagraph(value); }
        }

        public string ProfileDislikes
        {
            get { return _profileDislikes; }
            set
            {
                _profileDislikes = value;
                ProfileDislikesTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileAbout
        {
            get { return _profileAbout; }
            set
            {
                _profileAbout = value;
                ProfileAboutTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileMusic
        {
            get { return _profileMusic; }
            set
            {
                _profileMusic = value;
                ProfileMusicTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileBook
        {
            get { return _profileBook; }
            set
            {
                _profileBook = value;
                ProfileBookTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileTv
        {
            get { return _profileTv; }
            set
            {
                _profileTv = value;
                ProfileTvTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileFilm
        {
            get { return _profileFilm; }
            set
            {
                _profileFilm = value;
                ProfileFilmTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileInterest
        {
            get { return _profileInterest; }
            set
            {
                _profileInterest = value;
                ProfileInterestTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileRomance
        {
            get { return _profileRomance; }
            set
            {
                _profileRomance = value;
                ProfileRomanceTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileWork
        {
            get { return _profileWork; }
            set
            {
                _profileWork = value;
                ProfileWorkTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileEducation
        {
            get { return _profileEducation; }
            set
            {
                _profileEducation = value;
                ProfileEducationTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileSocialNetworks
        {
            get { return _profileSocialNetworks; }
            set
            {
                _profileSocialNetworks = value;
                ProfileSocialNetworksTransformed = ConvertHtmlToParagraph(value);
            }
        }

        public string ProfileHomepage
        {
            get { return _profileHomepage; }
            set { _profileHomepage = value; }
        }

        public List<FriendicaUser> ProfileUsers
        {
            get { return _profileUsers; }
            set { _profileUsers = value; }
        }

    }
}
