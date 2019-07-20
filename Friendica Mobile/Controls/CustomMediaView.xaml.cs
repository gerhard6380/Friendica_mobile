using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Friendica_Mobile;
using Plugin.MediaManager;
using Plugin.MediaManager.Abstractions.Enums;
using Plugin.MediaManager.Abstractions.Implementations;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    public partial class CustomMediaView : ContentView
    {
        // BindableProperty for the media url
        public static readonly BindableProperty MediaUrlProperty = BindableProperty.Create("MediaUrl",
                                                                    typeof(string), typeof(CustomMediaView),
                                                                    "", BindingMode.OneWay,
        propertyChanged: (bindable, value, newValue) =>
        {
            CrossMediaManager.Current.MediaQueue.Clear();
            // add media url to get data from this media file
            CrossMediaManager.Current.MediaQueue.Add(new MediaFile((string)newValue));
            ((CustomMediaView)bindable).GetMediaParameters();
        });
                          
        public string MediaUrl
        {
            get { return (string)GetValue(MediaUrlProperty); }
            set { SetValue(MediaUrlProperty, value); }
        }

        private MediaFileType _mediaType;
        public MediaFileType MediaType
        {
            get { return _mediaType; }
            set { SetProperty(ref _mediaType, value);
            IsAudioOnly = (value == MediaFileType.Audio);
            }
        }

        private bool _isShowingControls;
        public bool IsShowingControls
        {
            get { return _isShowingControls; }
            set { SetProperty(ref _isShowingControls, value); }
        }

        private bool _isAudioOnly;
        public bool IsAudioOnly
        {
            get { return _isAudioOnly; }
            set { SetProperty(ref _isAudioOnly, value);
                if (value)
                    IsShowingControls = true; }
        }

        // volume control currently not supported, therefore not used at the moment
        private bool _isVolumeMute;
        public bool IsVolumeMute
        {
            get { return _isVolumeMute; }
            set { SetProperty(ref _isVolumeMute, value); }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(ref _isPlaying, value); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set {
                if (_isLoading == value)
                    return;
                SetProperty(ref _isLoading, value);
                PerformRotating();
            }
        }

        private bool _isFailed;
        public bool IsFailed
        {
            get { return _isFailed; }
            set { SetProperty(ref _isFailed, value); }
        }

        private string _artist;
        public string Artist
        {
            get { return _artist; }
            set { SetProperty(ref _artist, value); }
        }

        private string _songTitle;
        public string SongTitle
        {
            get { return _songTitle; }
            set { SetProperty(ref _songTitle, value); }
        }

        private string _album;
        public string Album
        {
            get { return _album; }
            set { SetProperty(ref _album, value); }
        }

        private string _year;
        public string Year
        {
            get { return _year; }
            set { SetProperty(ref _year, value); }
        }

        private double _currentPosition;
        public double CurrentPosition
        {
            get { return _currentPosition; }
            set { SetProperty(ref _currentPosition, value); }
        }

        private TimeSpan _currentTime;
        public TimeSpan CurrentTime
        {
            get { return _currentTime; }
            set { SetProperty(ref _currentTime, value); }
        }

        private TimeSpan _totalTime;
        public TimeSpan TotalTime
        {
            get { return _totalTime; }
            set { SetProperty(ref _totalTime, value); }
        }


        public CustomMediaView()
        {
            InitializeComponent();

            // tap to show or hide the controls
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (sender, e) => 
            {
                IsShowingControls = !IsShowingControls;
            };
            GridContent.GestureRecognizers.Add(tapGesture);         
        }


        private void GetMediaParameters()
        {
            if (MediaUrl == null || MediaUrl == "")
                return;

            // get media type and set media player visibility
            MediaType = (MediaUrl.EndsWith("mp3")) ? MediaFileType.Audio : MediaFileType.Video;
            if (MediaType == MediaFileType.Video)
            {
                MediaPlayer.HeightRequest = App.ShellHeight / 3;
                MediaPlayer.IsVisible = true;
            }
            else
            {
                MediaPlayer.IsVisible = false;
                MediaPlayer.HeightRequest = 0;
            }
            CrossMediaManager.Current.MediaQueue.Current.Type = MediaType;

            // set title to filename, can be overridden if metadata is updated
            var uriSegments = new Uri(MediaUrl).Segments;
            SongTitle = uriSegments[uriSegments.Length - 1];
            Artist = new Uri(MediaUrl).Host;

            // load metadata from media file
            CrossMediaManager.Current.MediaQueue.Current.MetadataUpdated += (sender2, e2) =>
            {
                GetMetadata();
                TotalTime = CrossMediaManager.Current.Duration;
            };

            // react on status changes
            CrossMediaManager.Current.StatusChanged += (sender2, e2) =>
            {
                IsPlaying = (CrossMediaManager.Current.Status == MediaPlayerStatus.Playing);
                IsLoading = (CrossMediaManager.Current.Status == MediaPlayerStatus.Loading
                            || CrossMediaManager.Current.Status == MediaPlayerStatus.Buffering);
                IsFailed = (CrossMediaManager.Current.Status == MediaPlayerStatus.Failed);
                GetMetadata();
            };

            // update position data
            CrossMediaManager.Current.PlayingChanged += (sender2, e2) =>
            {
                // Android has a weird data structure, so we must react differently
                if (Device.RuntimePlatform == Device.Android)
                {
                    CurrentPosition = e2.Progress;
                    if (e2.Duration.Days > 0)
                    {
                        // wrong data returned, 3 Zeroes too much
                        TotalTime = ConvertWrongTime(e2.Duration);
                        CurrentTime = ConvertWrongTime(e2.Position);
                    }
                    else
                    {
                        TotalTime = e2.Duration;
                        var curr = TotalTime.Ticks * CurrentPosition;
                        CurrentTime = new TimeSpan((long)curr);
                    }
                }
                else
                {
                    CurrentTime = e2.Position;
                    TotalTime = e2.Duration;
                    if (TotalTime != TimeSpan.Zero)
                        CurrentPosition = CurrentTime.TotalSeconds / TotalTime.TotalSeconds;
                }
                // check metadata again, because some files are update later than others
                GetMetadata();
            };

            // play file and get metadata the first time if possible (not all platforms are firing MetadataUpdated
            CrossMediaManager.Current.Play(MediaUrl, MediaType);
            GetMetadata();
        }

        private TimeSpan ConvertWrongTime(TimeSpan time)
        {
            // remove last 3 digits if string is longer, this gives then the correct tick count
            var ticksString = time.Ticks.ToString();
            if (ticksString.Length > 3)
                ticksString = ticksString.Remove(ticksString.Length - 3, 3);
            return new TimeSpan(Convert.ToInt64(ticksString));
        }

        private void GetMetadata()
        {
            Artist = CrossMediaManager.Current.MediaQueue.Current.Metadata.Artist;

            // display filename instead of Unknown if there is no title available
            var title = CrossMediaManager.Current.MediaQueue.Current.Metadata.Title;
            if (title != "Unknown" && title != "")
                SongTitle = title;

            Album = CrossMediaManager.Current.MediaQueue.Current.Metadata.Album;

            // only display year with brackets if there is a relevant number available
            var year = CrossMediaManager.Current.MediaQueue.Current.Metadata.Year;
            if (year > 1900)
                Year = "(" + year.ToString() + ")";
        }

        private async void PerformRotating()
        {
            // when property is true, we start to rotate the gearing wheel by 45° each 1/4 second, we repeat this until property is false
            if (IsLoading)
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    await LoadingSymbol.RotateTo(360, 4000);
                }
                else
                {
                    var rotation = LoadingSymbol.Rotation + 45;
                    await LoadingSymbol.RotateTo(rotation, 250);
                    PerformRotating();
                }
            }
        }


        void SkipBack_IconButtonClicked(object sender, System.EventArgs e)
        {
            if (CurrentTime > TimeSpan.Zero)
            {
                if (CurrentTime > TimeSpan.FromSeconds(10))
                    CrossMediaManager.Current.Seek(CurrentTime.Subtract(TimeSpan.FromSeconds(10)));
            }
        }

        void PlayPause_IconButtonClicked(object sender, System.EventArgs e)
        {
            if (CrossMediaManager.Current.Status == MediaPlayerStatus.Playing)
                CrossMediaManager.Current.Pause();
            else if (CrossMediaManager.Current.Status == MediaPlayerStatus.Paused)
            {
                // platforms react different on pause function
                if (Device.RuntimePlatform == Device.UWP)
                    CrossMediaManager.Current.Pause();
                else
                    CrossMediaManager.Current.Play();
            }
            else
                CrossMediaManager.Current.Play();        
        }

        void Stop_IconButtonClicked(object sender, System.EventArgs e)
        {
            CrossMediaManager.Current.Stop();
        }

        void SkipForward_IconButtonClicked(object sender, System.EventArgs e)
        {
            if (CurrentTime > TimeSpan.Zero)
            {
                if (CurrentTime < TotalTime.Subtract(TimeSpan.FromSeconds(30)))
                    CrossMediaManager.Current.Seek(CurrentTime.Add(TimeSpan.FromSeconds(30)));
            }
        }
        
        private void VolumeMinus_IconButtonClicked(object sender, EventArgs e)
        {
            // not working currently
            CrossMediaManager.Current.VolumeManager.CurrentVolume = CrossMediaManager.Current.VolumeManager.CurrentVolume - 1;
        }

        private void Mute_IconButtonClicked(object sender, EventArgs e)
        {
            // not working currently
            CrossMediaManager.Current.VolumeManager.Mute = !CrossMediaManager.Current.VolumeManager.Mute;
            IsVolumeMute = CrossMediaManager.Current.VolumeManager.Mute;
        }

        private void VolumePlus_IconButtonClicked(object sender, EventArgs e)
        {
            // not working currently
            CrossMediaManager.Current.VolumeManager.CurrentVolume = CrossMediaManager.Current.VolumeManager.CurrentVolume + 1;
        }



        #region INotifyPropertyChanged
        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

}
