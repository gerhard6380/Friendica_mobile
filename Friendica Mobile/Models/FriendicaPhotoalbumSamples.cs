using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Albumname = "Test1",
                Photo1Visible = true,
                Photo2Visible = true,
                Photo3Visible = true,
                StackPhoto1 = new FriendicaPhotoExtended()
                {
                    PhotolistThumb = "http://www.save-celle.de/wp-content/uploads/2012/09/testbild_1297949508.jpg"
                },
                StackPhoto2 = new FriendicaPhotoExtended()
                {
                    PhotolistThumb = "http://images2.fanpop.com/images/photos/6300000/The-Simpsons-the-simpsons-6345058-1024-768.jpg"
                },
                StackPhoto3 = new FriendicaPhotoExtended()
                {
                    PhotolistThumb = "http://www.chicagonerds.com/wp-content/uploads/2013/07/the-big-bang-05.jpg"
                }
            };
            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                SourcePath = "http://www.save-celle.de/wp-content/uploads/2012/09/testbild_1297949508.jpg",
                Photo = new FriendicaPhoto() { PhotoDesc = "Das ist ein Testbild." },
                IsPubliclyVisible = true,
                IsConversationStarted = true
            });
            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                SourcePath = "http://images2.fanpop.com/images/photos/6300000/The-Simpsons-the-simpsons-6345058-1024-768.jpg",
                Photo = new FriendicaPhoto() { PhotoDesc = "Und nun hier ein etwas längerer Beschreibungstext um zu sehen wie das mit dem Umbrechen funktioniert." },
                IsPubliclyVisible = false,
                IsConversationStarted = false
            });
            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                SourcePath = "http://www.chicagonerds.com/wp-content/uploads/2013/07/the-big-bang-05.jpg",
                Photo = new FriendicaPhoto() { PhotoDesc = "" },
                IsPubliclyVisible = true,
                IsConversationStarted = false
            });
            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                SourcePath = "http://www.nibis.de/~niff/material/bild/tiere/original/katze.jpg",
                Photo = new FriendicaPhoto() { PhotoDesc = "Das ist eine Katze im Hochformat." },
                IsPubliclyVisible = false,
                IsConversationStarted = true
            });
            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                SourcePath = "https://putzlowitsch.de/wp-content/uploads/2011/06/katze-im-gras-1600.jpg",
                Photo = new FriendicaPhoto() { PhotoDesc = "" },
                IsPubliclyVisible = true,
                IsConversationStarted = true
            });
            album1.PhotosInAlbum.Add(new FriendicaPhotoExtended()
            {
                SourcePath = "http://www.fotos.sc/img2/u/mysite/h/Katze__Katzen__Kater__Ktzchen__niedlich__Samtpfote.jpg",
                Photo = new FriendicaPhoto() { PhotoDesc = "" },
                IsPubliclyVisible = false
            });
            var album2 = new FriendicaPhotoalbum()
            {
                Albumname = "Test2",
                Photo1Visible = true,
                Photo2Visible = false,
                Photo3Visible = false,
                StackPhoto1 = new FriendicaPhotoExtended() { PhotolistThumb = "http://www.save-celle.de/wp-content/uploads/2012/09/testbild_1297949508.jpg" },
            };
            var album3 = new FriendicaPhotoalbum()
            {
                Albumname = "Test3",
                Photo1Visible = true,
                Photo2Visible = true,
                Photo3Visible = false,
                StackPhoto1 = new FriendicaPhotoExtended()
                {
                    PhotolistThumb = "http://www.save-celle.de/wp-content/uploads/2012/09/testbild_1297949508.jpg"
                },
                StackPhoto2 = new FriendicaPhotoExtended()
                {
                    PhotolistThumb = "http://images2.fanpop.com/images/photos/6300000/The-Simpsons-the-simpsons-6345058-1024-768.jpg"
                },
            };
            //var album4 = new FriendicaPhotoalbum()
            //{
            //    Albumname = "Neu",
            //    NewAlbumVisible = true,
            //    Photo1Visible = false,
            //    Photo2Visible = false,
            //    Photo3Visible = false
            //};
            //PhotoalbumSamples.Add(album4);
            PhotoalbumSamples.Add(album1);
            PhotoalbumSamples.Add(album2);
            PhotoalbumSamples.Add(album3);
        }
    }
}
