using BackgroundTasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI.Xaml.Documents;

namespace Friendica_Mobile.Models
{
    public class FriendicaPhotoalbum : BindableClass
    {
        public event EventHandler PrintButtonClicked;

        // list of the photos in the album
        private ObservableCollection<FriendicaPhotoExtended> _photosInAlbum;
        public ObservableCollection<FriendicaPhotoExtended> PhotosInAlbum
        {
            get { return _photosInAlbum; }
            set { _photosInAlbum = value;
                OnPropertyChanged("PhotosInAlbum");
            }
        }

        // selected photo
        private FriendicaPhotoExtended _selectedPhoto;
        public FriendicaPhotoExtended SelectedPhoto
        {
            get { return _selectedPhoto; }
            set { _selectedPhoto = value;
                if (SelectedPhoto != null)
                    SelectedPhoto.PrintButtonClicked += SelectedPhoto_PrintButtonClicked;
                OnPropertyChanged("SelectedPhoto");
            }
        }


        // name of the album
        private string _albumname;
        public string Albumname
        {
            get { return _albumname; }
            set { _albumname = value;
                OnPropertyChanged("Albumname"); }
        }

        // has the changed albumname if user has edited the name in the view
        private string _albumnameNew;
        public string AlbumnameNew
        {
            get { return _albumnameNew; }
            set { _albumnameNew = value;
                OnPropertyChanged("AlbumnameNew"); }
        }


        // trigger display of a new album for interaction of user
        private bool _newAlbumVisible;
        public bool NewAlbumVisible
        {
            get { return _newAlbumVisible; }
            set { _newAlbumVisible = value; }
        }

        // trigger display of the 1st image in the "up to three image stack" of an album
        private bool _photo1Visible;
                
        public bool Photo1Visible
        {
            get { return _photo1Visible; }
            set { _photo1Visible = value;
                OnPropertyChanged("Photo1Visible");
            }
        }

        // trigger display of the 2nd image in the "up to three image stack" of an album
        private bool _photo2Visible;

        public bool Photo2Visible
        {
            get { return _photo2Visible; }
            set { _photo2Visible = value;
                OnPropertyChanged("Photo2Visible");
            }
        }

        // trigger display of the 3rd image in the "up to three image stack" of an album
        private bool _photo3Visible;

        public bool Photo3Visible
        {
            get { return _photo3Visible; }
            set { _photo3Visible = value;
                OnPropertyChanged("Photo3Visible");
            }
        }

        // image source for the 1st image in the "up to three image stack" of an album
        private FriendicaPhotoExtended _stackPhoto1;
        public FriendicaPhotoExtended StackPhoto1
        {
            get { return _stackPhoto1; }
            set { _stackPhoto1 = value;
                OnPropertyChanged("StackPhoto1");
            }
        }

        // image source for the 2nd image in the "up to three image stack" of an album
        private FriendicaPhotoExtended _stackPhoto2;
        public FriendicaPhotoExtended StackPhoto2
        {
            get { return _stackPhoto2; }
            set { _stackPhoto2 = value;
                OnPropertyChanged("StackPhoto2");
            }
        }

        // image source for the 3rd image in the "up to three image stack" of an album
        private FriendicaPhotoExtended _stackPhoto3;
        public FriendicaPhotoExtended StackPhoto3
        {
            get { return _stackPhoto3; }
            set { _stackPhoto3 = value;
                OnPropertyChanged("StackPhoto3");
            }
        }




        public FriendicaPhotoalbum()
        {
            PhotosInAlbum = new ObservableCollection<FriendicaPhotoExtended>();
        }

        private void SelectedPhoto_PrintButtonClicked(object sender, EventArgs e)
        {
            if (PrintButtonClicked != null)
                PrintButtonClicked.Invoke(this, EventArgs.Empty);
        }
    }
}
