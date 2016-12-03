using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Xaml;
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
        private Thickness _profileIsDefaultInt;
        public Thickness ProfileIsDefaultInt
        {
            get { return _profileIsDefaultInt; }
            set { _profileIsDefaultInt = value; }
        }

        private DateTimeOffset _profileDateOfBirthTransformed;
        public DateTimeOffset ProfileDateOfBirthTransformed
        {
            get { return _profileDateOfBirthTransformed; }
            set
            {
                _profileDateOfBirthTransformed = value;
                EditedProfileDateOfBirth = value;
            }
        }

        private string _profileCountryRegionTransformed;
        public string ProfileCountryRegionTransformed
        {
            get { return _profileCountryRegionTransformed; }
            set
            {
                _profileCountryRegionTransformed = value;
                EditedProfileCountryRegion = value;
            }
        }

        private string _profileMaritalTransformed;
        public string ProfileMaritalTransformed
        {
            get { return _profileMaritalTransformed; }
            set { _profileMaritalTransformed = value;
                EditedProfileMarital = value; }
        }

        private string _profileGenderTransformed;
        public string ProfileGenderTransformed
        {
            get { return _profileGenderTransformed; }
            set { _profileGenderTransformed = value;
                EditedProfileGender = value; }
        }

        private DateTimeOffset _profileMaritalSinceTransformed;
        public DateTimeOffset ProfileMaritalSinceTransformed
        {
            get { return _profileMaritalSinceTransformed; }
            set { _profileMaritalSinceTransformed = value;
                EditedProfileMaritalSince = value; }
        }

        private string _profileSexualTransformed;
        public string ProfileSexualTransformed
        {
            get { return _profileSexualTransformed; }
            set { _profileSexualTransformed = value;
                EditedProfileSexual = value; }
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

        private Paragraph _profileHomepageTransformed;
        public Paragraph ProfileHomepageTransformed
        {
            get { return _profileHomepageTransformed; }
            set { _profileHomepageTransformed = value; }
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
            ProfileIsDefault = (bool)CheckAttribute(jsonObject, profileIsDefaultKey, AttributeTypes.Boolean);
            ProfileHideFriends = (bool)CheckAttribute(jsonObject, profileHideFriendsKey, AttributeTypes.Boolean);
            ProfilePhoto = (string)CheckAttribute(jsonObject, profilePhotoKey, AttributeTypes.String);
            ProfileThumb = (string)CheckAttribute(jsonObject, profileThumbKey, AttributeTypes.String);
            ProfilePublish = (bool)CheckAttribute(jsonObject, profilePublishKey, AttributeTypes.Boolean);
            ProfileNetPublish = (bool)CheckAttribute(jsonObject, profileNetPublishKey, AttributeTypes.Boolean);
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
            ProfileReligion = (string)CheckAttribute(jsonObject, profileReligionKey, AttributeTypes.String);
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
                    {
                        string test = jsonObject.GetNamedString(key, "");
                        return jsonObject.GetNamedString(key, "");
                    }
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
                return null; //return new Paragraph();
        }


        public string ProfileId
        {
            get { return _profileId; }
            set { _profileId = value; }
        }

        public string ProfileName
        {
            get { return _profileName; }
            set
            {
                _profileName = value;
                EditedProfileName = value;
            }
        }

        public bool ProfileIsDefault
        {
            get { return _profileIsDefault; }
            set { _profileIsDefault = value;
                ProfileIsDefaultInt = (value) ? new Thickness(2.0) : new Thickness(0.0); }
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
            set
            {
                _profileDescription = value;
                EditedProfileDescription = value;
            }
        }

        public string ProfileDateOfBirth
        {
            get { return _profileDateOfBirth; }
            set
            {
                _profileDateOfBirth = value;
                if (value != "")
                {
                    try
                    {
                        var date = DateTime.ParseExact(value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        ProfileDateOfBirthTransformed = new DateTimeOffset(date);
                    }
                    catch (Exception ex)
                    {
                        if (value == "0000-00-00")
                            ProfileDateOfBirthTransformed = default(DateTimeOffset);
                        else if (value.StartsWith("0000-"))
                            ProfileDateOfBirthTransformed = new DateTimeOffset(DateTime.ParseExact(value.Replace("0000", "1900"), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));           
                    }
                }
            }
        }

        public string ProfileAddress
        {
            get { return _profileAddress; }
            set { _profileAddress = value;
                EditedProfileAddress = value; }
        }

        public string ProfileCity
        {
            get { return _profileCity; }
            set { _profileCity = value;
                EditedProfileCity = value; }
        }

        public string ProfileRegion
        {
            get { return _profileRegion; }
            set { _profileRegion = value;
                ProfileCountryRegionTransformed = "FriendicaRegion|" + ProfileCountry + "|" + value; }
        }

        public string ProfilePostalCode
        {
            get { return _profilePostalCode; }
            set { _profilePostalCode = value;
                EditedProfilePostalCode = value; }
        }

        public string ProfileCountry
        {
            get { return _profileCountry; }
            set { _profileCountry = value;
                ProfileCountryRegionTransformed = "FriendicaRegion|" + value + "|" + ProfileRegion; }
        }

        public string ProfileHometown
        {
            get { return _profileHometown; }
            set { _profileHometown = value;
                EditedProfileHometown = value; }
        }

        public string ProfileGender
        {
            get { return _profileGender; }
            set { _profileGender = value;
                ProfileGenderTransformed = "FriendicaGender|" + value; }
        }

        public string ProfileMarital
        {
            get { return _profileMarital; }
            set { _profileMarital = value;
                ProfileMaritalTransformed = "FriendicaMarital|" + value; }
        }

        public string ProfileMaritalWith
        {
            get { return _profileMaritalWith; }
            set { _profileMaritalWith = value;
                EditedProfileMaritalWith = value; }
        }

        public string ProfileMaritalSince
        {
            get { return _profileMaritalSince; }
            set { _profileMaritalSince = value;
                if (value != "")
                {
                    try
                    {
                        var date = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        ProfileMaritalSinceTransformed = new DateTimeOffset(date);
                    }
                    catch (Exception ex)
                    {
                        if (value == "0000-00-00 00:00:00")
                            ProfileMaritalSinceTransformed = default(DateTimeOffset);
                        else if (value.StartsWith("0000-"))
                            ProfileMaritalSinceTransformed = new DateTimeOffset(DateTime.ParseExact(value.Replace("0000", "1900"), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        public string ProfileSexual
        {
            get { return _profileSexual; }
            set { _profileSexual = value;
                ProfileSexualTransformed = "FriendicaSexual|" + value; }
        }

        public string ProfilePolitic
        {
            get { return _profilePolitic; }
            set { _profilePolitic = value;
                EditedProfilePolitic = value; }
        }

        public string ProfileReligion
        {
            get { return _profileReligion; }
            set { _profileReligion = value;
                EditedProfileReligion = value; }
        }

        public string ProfilePublicKeywords
        {
            get { return _profilePublicKeywords; }
            set
            {
                _profilePublicKeywords = value;
                EditedProfilePublicKeywords = value;
            }
        }

        public string ProfilePrivateKeywords
        {
            get { return _profilePrivateKeywords; }
            set
            {
                _profilePrivateKeywords = value;
                EditedProfilePrivateKeywords = value;
            }
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
            set { _profileHomepage = value;
                string homepage = "";
                if (value != "")
                    homepage = "<a href='" + value + "'>" + value + "</a>";
                ProfileHomepageTransformed = ConvertHtmlToParagraph(homepage); }
        }

        public List<FriendicaUser> ProfileUsers
        {
            get { return _profileUsers; }
            set { _profileUsers = value; }
        }


        // other functions
        public void RecreateParagraphFields()
        {
            ProfileLikesTransformed = ConvertHtmlToParagraph(ProfileLikes);
            ProfileDislikesTransformed = ConvertHtmlToParagraph(ProfileDislikes);
            ProfileAboutTransformed = ConvertHtmlToParagraph(ProfileAbout);
            ProfileMusicTransformed = ConvertHtmlToParagraph(ProfileMusic);
            ProfileBookTransformed = ConvertHtmlToParagraph(ProfileBook);
            ProfileTvTransformed = ConvertHtmlToParagraph(ProfileTv);
            ProfileFilmTransformed = ConvertHtmlToParagraph(ProfileFilm);
            ProfileInterestTransformed = ConvertHtmlToParagraph(ProfileInterest);
            ProfileRomanceTransformed = ConvertHtmlToParagraph(ProfileRomance);
            ProfileWorkTransformed = ConvertHtmlToParagraph(ProfileWork);
            ProfileEducationTransformed = ConvertHtmlToParagraph(ProfileEducation);
            ProfileSocialNetworksTransformed = ConvertHtmlToParagraph(ProfileSocialNetworks);
            string homepage = "";
            if (ProfileHomepage != "")
                homepage = "<a href='" + ProfileHomepage + "'>" + ProfileHomepage + "</a>";
            ProfileHomepageTransformed = ConvertHtmlToParagraph(homepage);
        }

        public void ClearOutParagraphFields()
        {
            ProfileLikesTransformed = ConvertHtmlToParagraph("");
            ProfileDislikesTransformed = ConvertHtmlToParagraph("");
            ProfileAboutTransformed = ConvertHtmlToParagraph("");
            ProfileMusicTransformed = ConvertHtmlToParagraph("");
            ProfileBookTransformed = ConvertHtmlToParagraph("");
            ProfileTvTransformed = ConvertHtmlToParagraph("");
            ProfileFilmTransformed = ConvertHtmlToParagraph("");
            ProfileInterestTransformed = ConvertHtmlToParagraph("");
            ProfileRomanceTransformed = ConvertHtmlToParagraph("");
            ProfileWorkTransformed = ConvertHtmlToParagraph("");
            ProfileEducationTransformed = ConvertHtmlToParagraph("");
            ProfileSocialNetworksTransformed = ConvertHtmlToParagraph("");
            ProfileHomepageTransformed = ConvertHtmlToParagraph("");
        }

        // edited data
        private string _editedProfileName;
        public string EditedProfileName
        {
            get { return _editedProfileName; }
            set { _editedProfileName = value; }
        }

        private string _editedProfileDescription;
        public string EditedProfileDescription
        {
            get { return _editedProfileDescription; }
            set { _editedProfileDescription = value; }
        }

        private string _editedProfileGender;
        public string EditedProfileGender
        {
            get { return _editedProfileGender; }
            set { _editedProfileGender = value; }
        }

        private string _editedProfilePublicKeywords;
        public string EditedProfilePublicKeywords
        {
            get { return _editedProfilePublicKeywords; }
            set { _editedProfilePublicKeywords = value; }
        }

        private string _editedProfilePrivateKeywords;
        public string EditedProfilePrivateKeywords
        {
            get { return _editedProfilePrivateKeywords; }
            set { _editedProfilePrivateKeywords = value; }
        }


        private DateTimeOffset _editedProfileDateOfBirth;
        public DateTimeOffset EditedProfileDateOfBirth
        {
            get { return _editedProfileDateOfBirth; }
            set { _editedProfileDateOfBirth = value; }
        }

        private string _editedProfileAddress;
        public string EditedProfileAddress
        {
            get { return _editedProfileAddress; }
            set { _editedProfileAddress = value; }
        }

        private string _editedProfileCity;
        public string EditedProfileCity
        {
            get { return _editedProfileCity; }
            set { _editedProfileCity = value; }
        }

        private string _editedProfilePostalCode;
        public string EditedProfilePostalCode
        {
            get { return _editedProfilePostalCode; }
            set { _editedProfilePostalCode = value; }
        }

        private string _editedProfileCountryRegion;
        public string EditedProfileCountryRegion
        {
            get { return _editedProfileCountryRegion; }
            set { _editedProfileCountryRegion = value;}
        }

        private string _editedProfileHometown;
        public string EditedProfileHometown
        {
            get { return _editedProfileHometown; }
            set { _editedProfileHometown = value; }
        }

        private string _editedProfileMarital;
        public string EditedProfileMarital
        {
            get { return _editedProfileMarital; }
            set { _editedProfileMarital = value; }
        }

        private string _editedProfileMaritalWith;
        public string EditedProfileMaritalWith
        {
            get { return _editedProfileMaritalWith; }
            set { _editedProfileMaritalWith = value; }
        }

        private DateTimeOffset _editedProfileMaritalSince;
        public DateTimeOffset EditedProfileMaritalSince
        {
            get { return _editedProfileMaritalSince; }
            set { _editedProfileMaritalSince = value; }
        }

        private string _editedProfileSexual;
        public string EditedProfileSexual
        {
            get { return _editedProfileSexual; }
            set { _editedProfileSexual = value; }
        }

        private string _editedProfilePolitic;
        public string EditedProfilePolitic
        {
            get { return _editedProfilePolitic; }
            set { _editedProfilePolitic = value; }
        }

        private string _editedProfileReligion;
        public string EditedProfileReligion
        {
            get { return _editedProfileReligion; }
            set { _editedProfileReligion = value; }
        }

    }
}
