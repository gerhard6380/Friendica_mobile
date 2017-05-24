using Friendica_Mobile.HttpRequests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

namespace Friendica_Mobile.Models
{
    class FriendicaPhotoalbumSamples
    {
        public ObservableCollection<FriendicaPhotoalbum> PhotoalbumSamples;

        public FriendicaPhotoalbumSamples()
        {
            PhotoalbumSamples = new ObservableCollection<FriendicaPhotoalbum>();
            FillSampleData();
        }

        private void FillSampleData()
        {
            var album1 = new FriendicaPhotoalbum()
            {
                Albumname = "Oil on canvas",
            };

            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ap/mobile-large/ap1980.360.12.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "American Eagle on Red Scroll (W. H. Bean)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Oil on canvas",
                    PhotoComments = new List<FriendicaPostExtended>()
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ap/mobile-large/ap1980.360.12.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ap/web-large/ap1980.360.12.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ap/original/ap1980.360.12.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true,
                IsConversationStarted = true
            });

            var sample = "{\"text\":\"Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:54 +0000 2015\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"Friendica\",\"id\":4,\"id_str\":\"4\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":{\"type\":\"Point\",\"coordinates\":[40.7127837,-74.0059413]},\"location\":\"New York\",\"favorited\":false,\"user\":{\"id\":1,\"id_str\":\"1\",\"name\":\"Testuser #1\",\"screen_name\":\"test1\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":2,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":1,\"network\":\"dfrn\"},\"statusnet_html\":\"<img src=\\\"http://images.metmuseum.org/CRDImages/ap/web-large/ap1980.360.12.jpg\\\" alt =\\\"Bild/Foto\\\">Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\",\"statusnet_conversation_id\":\"4\"}";
            var postExtended1 = new FriendicaPostExtended(sample);
            album1.PhotosInAlbum[0].Photo.PhotoComments.Add(postExtended1);

            sample = "{\"text\":\"Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:45 +0000 2015\",\"in_reply_to_status_id\":1,\"in_reply_to_status_id_str\":\"1\",\"source\":\"Friendica\",\"id\":2,\"id_str\":\"2\",\"in_reply_to_user_id\":1,\"in_reply_to_user_id_str\":\"1\",\"in_reply_to_screen_name\":\"test1\",\"geo\":null,\"location\":\"\",\"favorited\":false,\"user\":{\"id\":4,\"id_str\":\"4\",\"name\":\"Testuser #4\",\"screen_name\":\"test4\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":1,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":4,\"network\":\"dfrn\"},\"statusnet_html\":\"Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\",\"statusnet_conversation_id\":\"1\"}";
            var postExtended2 = new FriendicaPostExtended(sample);
            album1.PhotosInAlbum[0].Photo.PhotoComments.Add(postExtended2);
            var userArya = new FriendicaUserExtended(6, "Arya Stark", "https://friendica.server.text/profile/aryastark", "aryastark", "https://upload.wikimedia.org/wikipedia/en/3/39/Arya_Stark-Maisie_Williams.jpg", ContactTypes.Friends);
            var userCatelyn = new FriendicaUserExtended(10, "Catelyn Stark", "https://friendica.server.text/profile/catelyn", "catelyn", "https://upload.wikimedia.org/wikipedia/en/1/1b/Catelyn_Stark_S3.jpg", ContactTypes.Friends);
            var userSansa = new FriendicaUserExtended(5, "Sansa Stark", "https://friendica.server.text/profile/sansa", "sansa", "https://upload.wikimedia.org/wikipedia/en/7/74/SophieTurnerasSansaStark.jpg", ContactTypes.Friends);
            var userDaenerys = new FriendicaUserExtended(3, "Daenerys Targaryen", "https://friendica.server.text/profile/daenerys", "daenerys", "https://upload.wikimedia.org/wikipedia/en/0/0d/Daenerys_Targaryen_with_Dragon-Emilia_Clarke.jpg", ContactTypes.Friends);
            var userMyself = new FriendicaUserExtended();
            userMyself.IsAuthenticatedUser = true;
            AddSampleUsers(postExtended2, userArya, PostFriendicaActivities.FriendicaActivity.like);
            AddSampleUsers(postExtended2, userCatelyn, PostFriendicaActivities.FriendicaActivity.like);
            AddSampleUsers(postExtended2, userSansa, PostFriendicaActivities.FriendicaActivity.like);
            AddSampleUsers(postExtended2, userDaenerys, PostFriendicaActivities.FriendicaActivity.dislike);
            AddSampleUsers(postExtended2, userMyself, PostFriendicaActivities.FriendicaActivity.like);


            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ma/mobile-large/DT7011.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Woman in Red Blouse (Chaim Soutine)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Oil on canvas"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ma/mobile-large/DT7011.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ma/web-large/DT7011.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ma/original/DT7011.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ep/mobile-large/77J_045R2M.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Mont Sainte-Victoire (Paul Sezanne)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Oil on canvas"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/mobile-large/77J_045R2M.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/web-large/77J_045R2M.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/original/77J_045R2M.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ep/mobile-large/DP316906.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "The Outer Harbor of Brest (Henri Joseph van Blarenberghe)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Oil on canvas"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/mobile-large/DP316906.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/web-large/DP316906.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/original/DP316906.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ep/mobile-large/DT7098.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Bouquet of Flowers in a Vase (Vincent van Gogh)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Oil on canvas"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/mobile-large/DT7098.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/web-large/DT7098.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/original/DT7098.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album1.SetPhotosForAlbumView();

            var album2 = new FriendicaPhotoalbum()
            {
                Albumname = "Watercolor on paper",
            };
            album2.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ap/mobile-large/ap67.55.156.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "The Towers of St. Martin, Tours (Joseph Pennell)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Watercolor on paper"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ap/mobile-large/ap67.55.156.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ap/web-large/ap67.55.156.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ap/original/ap67.55.156.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album2.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ep/mobile-large/DP123836.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Peonies (Édouard Manet)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Watercolor on paper"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/mobile-large/DP123836.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/web-large/DP123836.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/original/DP123836.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album2.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ma/mobile-large/DT7783.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Composition with the Yellow Half-Moon and the Y (Paul Klee)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Watercolor on paper"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ma/mobile-large/DT7783.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ma/web-large/DT7783.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ma/original/DT7783.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album2.SetPhotosForAlbumView();

            var album3 = new FriendicaPhotoalbum()
            {
                Albumname = "Oil on wood",
            };
            album3.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ep/mobile-large/DT2019.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Bacchante by the sea (Camille Corot)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Oil on wood"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/mobile-large/DT2019.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/web-large/DT2019.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/original/DT2019.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album3.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ep/mobile-large/DT373.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "The Conquest of Naples (Master of Charles of Durazzo)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Oil on wood"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/mobile-large/DT373.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/web-large/DT373.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/original/DT373.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album3.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ma/mobile-large/DT220060.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "The Harbor (Josef Presser)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Oil on wood"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ma/mobile-large/DT220060.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ma/web-large/DT220060.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ma/original/DT220060.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album3.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/ep/mobile-large/DP375107.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Fish market (Joachim Beuckelaer)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Oil on wood"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/mobile-large/DP375107.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/web-large/DP375107.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/ep/original/DP375107.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album3.SetPhotosForAlbumView();

            var album4 = new FriendicaPhotoalbum()
            {
                Albumname = "Asian art",
            };
            album4.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/as/mobile-large/DP211813.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Monkeys (Kawanabe Kyōsai)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Asian art"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/as/mobile-large/DP211813.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/as/web-large/DP211813.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/as/original/DP211813.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album4.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/as/mobile-large/DP227750.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Landscapes of the Four Seasons (Xie Shichen)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Asian art"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/as/mobile-large/DP227750.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/as/web-large/DP227750.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/as/original/DP227750.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album4.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                PhotolistThumb = "http://images.metmuseum.org/CRDImages/as/mobile-large/DP120312.jpg",
                Photo = new FriendicaPhoto()
                {
                    PhotoDesc = "Winter Landscape and Flowers (Qian Weicheng)",
                    PhotoEdited = "2017-05-05 22:23:25",
                    PhotoAlbum = "Asian art"
                },
                ThumbSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/as/mobile-large/DP120312.jpg")),
                MediumSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/as/web-large/DP120312.jpg")),
                MediumSizeLoaded = true,
                FullSizeData = new BitmapImage(new Uri("http://images.metmuseum.org/CRDImages/as/original/DP120312.jpg")),
                FullSizeLoaded = true,
                IsPubliclyVisible = true,
                IsFriendicaPhoto = true
            });
            album4.SetPhotosForAlbumView();

            PhotoalbumSamples.Add(album1);
            PhotoalbumSamples.Add(album2);
            PhotoalbumSamples.Add(album3);
            PhotoalbumSamples.Add(album4);
        }

        private void AddSampleUsers(FriendicaPostExtended post, FriendicaUserExtended user, PostFriendicaActivities.FriendicaActivity activity)
        {
            if (post.Post.PostFriendicaActivities == null)
                post.Post.PostFriendicaActivities = new FriendicaActivities();

            if (user != null)
            {
                if (activity == PostFriendicaActivities.FriendicaActivity.like)
                {
                    post.Post.PostFriendicaActivities.ActivitiesLike.Add(user);
                }
                else if (activity == PostFriendicaActivities.FriendicaActivity.dislike)
                {
                    post.Post.PostFriendicaActivities.ActivitiesDislike.Add(user);
                }
            }

            post.SetActivitiesParameters();
        }

    }
}
