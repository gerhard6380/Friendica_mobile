using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Friendica_Mobile.Styles
{
    // class responsible for managing the navigation incl. backstack management
    public class MasterDetailControlViewModel : INotifyPropertyChanged, INavigation
    {
        // property for the title of the view - shown in title bar
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public event EventHandler IsNavigationRunningChanged;
        // property for indicating a current long during navigation activity (spinning settings icon)
        private bool _isNavigationRunning;
        public bool IsNavigationRunning
        {
            get { return _isNavigationRunning; }
            set
            {
                if (value != _isNavigationRunning)
                {
                    SetProperty(ref _isNavigationRunning, value);
                    IsNavigationRunningChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // property for removing side content if navigation is not allowed
        private bool _sideContentVisible = true;
        public bool SideContentVisible
        {
            get { return _sideContentVisible; }
            set
            {
                if (value != _sideContentVisible)
                {
                    SetProperty(ref _sideContentVisible, value);
                }
            }
        }

        // event raised when a button is clicked to fade-out the flyout for more options if still open
        public event EventHandler ButtonClicked;
        // base navigation method firing the buttonClicked event and set Detail to the new ContentPage to be displayed
        internal void NavigateTo(ContentPage page)
        {
            ButtonClicked?.Invoke(this, EventArgs.Empty);
            Detail = page;
        }


        private INavigation _navigation;
        private Stack<Page> _pages = new Stack<Page>();

        private Page _detail;
        public Page Detail
        {
            get { return _detail; }
            set
            {
                if (_detail != value)
                {
                    _pages.Push(Detail);
                    _detail = value;
                    NavigationAllowed = IsBackButtonEnabled();
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // event to trigger if back button is enabled/disabled
        public event EventHandler NavigationAllowedChanged;

        private bool _navigationAllowed = true;
        public bool NavigationAllowed
        {
            get { return _navigationAllowed; }
            set
            {
                _navigationAllowed = value;
                NavigationAllowedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsBackButtonEnabled()
        {
            // check if the backbutton can be enabled or disabled depending on the stack for backnavigation
            if (SideContentVisible && _pages.Count > 0 && _pages.ElementAt(0) != null)
                return true;
            else
                return false;
        }


        public IReadOnlyList<Page> ModalStack { get { return _navigation.ModalStack; } }

        public IReadOnlyList<Page> NavigationStack
        {
            get
            {
                if (_pages.Count == 0)
                {
                    return _navigation.NavigationStack;
                }
                var implPages = _navigation.NavigationStack;
                MasterDetailControl master = null;
                var beforeMaster = implPages.TakeWhile(d =>
                {
                    master = d as MasterDetailControl;
                    return master != null || d.GetType() == typeof(MasterDetailControl);
                }).ToList();
                beforeMaster.AddRange(_pages);
                beforeMaster.AddRange(implPages.Where(d => !beforeMaster.Contains(d)
                    && d != master));
                return new ReadOnlyCollection<Page>(_navigation.NavigationStack.ToList());
            }
        }


        public void SetNavigation(INavigation navigation)
        {
            _navigation = navigation;
        }

        public void InsertPageBefore(Page page, Page before)
        {
            if (_pages.Contains(before))
            {
                var list = _pages.ToList();
                var indexOfBefore = list.IndexOf(before);
                list.Insert(indexOfBefore, page);
                _pages = new Stack<Page>(list);
            }
            else
            {
                _navigation.InsertPageBefore(page, before);
            }
        }

        public Task<Page> PopAsync()
        {
            Page page = null;
            if (_pages.Count > 0 && _pages.ElementAt(0) != null)
            {
                page = _pages.Pop();
                _detail = page;
                OnPropertyChanged("Detail");
            }
            NavigationAllowed = IsBackButtonEnabled();
            return page != null ? Task.FromResult(page) : _navigation.PopAsync();
        }

        public Task<Page> PopAsync(bool animated)
        {
            Page page = null;
            if (_pages.Count > 0 && _pages.ElementAt(0) != null)
            {
                page = _pages.Pop();
                _detail = page;
                OnPropertyChanged("Detail");
            }
            NavigationAllowed = IsBackButtonEnabled();
            return page != null ? Task.FromResult(page) : _navigation.PopAsync(animated);
        }

        public Task<Page> PopModalAsync()
        {
            return _navigation.PopModalAsync();
        }

        public Task<Page> PopModalAsync(bool animated)
        {
            return _navigation.PopModalAsync(animated);
        }

        public Task PopToRootAsync()
        {
            var firstPage = _navigation.NavigationStack[0];
            if (firstPage is MasterDetailControl
                || firstPage.GetType() == typeof(MasterDetailControl))
            {
                _pages = new Stack<Page>(new[] { _pages.FirstOrDefault() });
                return Task.FromResult(firstPage);
            }
            return _navigation.PopToRootAsync();
        }

        public Task PopToRootAsync(bool animated)
        {
            var firstPage = _navigation.NavigationStack[0];
            if (firstPage is MasterDetailControl
                || firstPage.GetType() == typeof(MasterDetailControl))
            {
                _pages = new Stack<Page>(new[] { _pages.FirstOrDefault() });
                return Task.FromResult(firstPage);
            }
            return _navigation.PopToRootAsync(animated);
        }

        public Task PushAsync(Page page)
        {
            Detail = page;
            return Task.FromResult(page);
        }

        public Task PushAsync(Page page, bool animated)
        {
            Detail = page;
            return Task.FromResult(page);
        }

        public Task PushModalAsync(Page page)
        {
            return _navigation.PushModalAsync(page);
        }

        public Task PushModalAsync(Page page, bool animated)
        {
            return _navigation.PushModalAsync(page, animated);
        }

        public void RemovePage(Page page)
        {
            if (_pages.Contains(page))
            {
                var list = _pages.ToList();
                list.Remove(page);
                _pages = new Stack<Page>(list);
            }
            _navigation.RemovePage(page);
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }
}