using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Friendica_Mobile.Strings;
using Friendica_Mobile.Views;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Friendica_Mobile.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        // property for the title of the view - shown in title bar
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        
        // property for the number of unread elements (don't forget the property and binding in Shell.xaml and ShellViewModel)
        private int _counter;
        public int Counter
        {
            get { return _counter; }
            set
            {
                SetProperty(ref _counter, value);
                SetCounter();
            }
        }

		// property to show an activity indicator (progress ring)
        private bool _activityIndicatorVisible;
        public bool ActivityIndicatorVisible
        {
            get { return _activityIndicatorVisible; }
            set
            {
                SetProperty(ref _activityIndicatorVisible, value);
            }
        }

        // property with text to be shown in activity indicator (progress ring)
        private string _activityIndicatorText;
        public string ActivityIndicatorText
        {
            get { return _activityIndicatorText; }
            set { SetProperty(ref _activityIndicatorText, value); }
        }

        // indicator if a server activity has failed (if a page needs more than one activity failed item, the 2nd needs to be declared in page viewmodel)
        private bool _serverActivityFailed = false;
        public bool ServerActivityFailed
        {
            get { return _serverActivityFailed; }
            set { SetProperty(ref _serverActivityFailed, value); }
        }

        // error message to be shown if a server activity has been failed  
        private string _labelServerActivityFailed;
        public string LabelServerActivityFailed
        {
            get { return _labelServerActivityFailed; }
            set { SetProperty(ref _labelServerActivityFailed, value); }
        }

        // mediaUrl for MediaPlayer
        private string _mediaUrlVM;
        public string MediaUrlVM
        {
            get { return _mediaUrlVM; }
            set { SetProperty(ref _mediaUrlVM, value); }
        }

        // indicator to show the media player
        private bool _isMediaPlayerVisible;
        public bool IsMediaPlayerVisible
        {
            get { return _isMediaPlayerVisible; }
            set { SetProperty(ref _isMediaPlayerVisible, value); }
        }

        // indicator to show the button for fullscreen media player
        private bool _isAudioOnly;
        public bool IsAudioOnly
        {
            get { return _isAudioOnly;  }
            set { SetProperty(ref _isAudioOnly, value); }
        }

        // content for code fullscreen
        private View _codeFullscreenContent;
        public View CodeFullscreenContent
        {
            get { return _codeFullscreenContent; }
            set { SetProperty(ref _codeFullscreenContent, value); }
        }

        // indicator to show the fullscreen for code
        private bool _isCodeFullscreenVisible;
        public bool IsCodeFullscreenVisible
        {
            get { return _isCodeFullscreenVisible; }
            set { SetProperty(ref _isCodeFullscreenVisible, value); }
        }


        public void NavigateTo(ContentPage page)
        {
            // get shell environment and load the new page
            var nav = Application.Current.MainPage as NavigationPage;
            var shell = nav.RootPage as Views.CustomShell;
            var vm = shell.BindingContext as ShellViewModel;
            vm.Detail = page;
        }

        public void NavigateBack()
        {
            var nav = Application.Current.MainPage as NavigationPage;
            var shell = nav.RootPage as Views.CustomShell;
            var vm = shell.BindingContext as ShellViewModel;
            vm.PopAsync();
        }


        private void SetCounter()
        {
            // get shell environment and set counter to new value
            // dont't forget to add a xyzCounter property in ShellViewModel and the binding
            // to this property in the Shell.xaml
            var nav = Application.Current.MainPage as NavigationPage;
            var shell = nav.RootPage as Views.CustomShell;

            var page = shell.Detail.GetType().Name;

            var vm = shell.BindingContext as ShellViewModel;
            typeof(ShellViewModel).GetProperty(page + "Counter")?.SetValue(vm, Counter);
        }

        internal void SetIsNavigationRunning(bool isRunning)
        {
            // used to start spinning the Settings gearing wheel when a long-lasting activity is expected
            // can be used parallel to the server activity indicator on the page (useful for navigation events
            // or when background activities started and probably could slow the app)
            // true = start spinning, false = stop spinning
            var nav = Application.Current.MainPage as NavigationPage;
            var shell = nav.RootPage as Views.CustomShell;
            var vm = shell.BindingContext as ShellViewModel;
            vm.IsNavigationRunning = isRunning;
        }

        internal void SetNavigationAllowed(bool navAllowed)
        {
            var nav = Application.Current.MainPage as NavigationPage;
            var shell = nav.RootPage as Views.CustomShell;
            var vm = shell.BindingContext as ShellViewModel;
            vm.SideContentVisible = navAllowed;
            vm.NavigationAllowed = navAllowed;
        }



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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
