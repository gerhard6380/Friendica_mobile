
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Friendica_Mobile.ViewModel;
using Friendica_Mobile.Styles;
using Xamarin.Forms;

namespace Friendica_Mobile.Styles
{
    public partial class MasterDetailControl
    {
        private double width;
        private double height;

        public View SideContent
        {
            get { return (View)GetValue(SideContentProperty); }
            set { SetValue(SideContentProperty, value); }
        }

        public View MoreOptions
        {
            get { return (View)GetValue(MoreOptionsProperty); }
            set { SetValue(MoreOptionsProperty, value); }
        }

        public ContentPage Detail
        {
            get { return (ContentPage)GetValue(DetailProperty); }
            set
            {
                SetValue(DetailProperty, value);
            }
        }


        public MasterDetailControl()
        {
            InitializeComponent();    
            SetBinding(DetailProperty, new Binding("Detail", BindingMode.OneWay));

            SetGridFlyoutBaseTap();
            SetGridTapGestureTap();
            Settings.NavigationSideChanged += (sender, e) => { SetNavigationSide(); };
            SetNavigationSide();
        }


        // Used to bind to the definitions of the menu contents in Shell.xaml
        public static readonly BindableProperty SideContentProperty = BindableProperty.Create("SideContent",
            typeof(View), typeof(MasterDetailControl), null, propertyChanged: (bindable, value, newValue) =>
            {
                var control = (MasterDetailControl)bindable;              
                control.SideContentContainer.Children.Clear();
            control.SideContentContainer.Children.Add(control.SideContent);
            });

        // Used to bind to the definitions of the flyout contents in Shell.xaml
        public static readonly BindableProperty MoreOptionsProperty = BindableProperty.Create("MoreOptions",
            typeof(View), typeof(MasterDetailControl), null, propertyChanged: (bindable, value, newValue) =>
            {
                var control = (MasterDetailControl)bindable;
                control.MoreOptionsContainer.Children.Clear();
                control.MoreOptionsContainer.Children.Add(control.MoreOptions);
            });


        // used to bind to the pages to be displayed in DetailContainer
        public readonly BindableProperty DetailProperty = BindableProperty.Create("Detail",
            typeof(ContentPage), typeof(MasterDetailControl),
            propertyChanged: (bindable, value, newValue) =>
            {
                var masterPage = (MasterDetailControl)bindable;
                masterPage.DetailContainer.Content = newValue != null ?
                ((ContentPage)newValue).Content : null;

                // after setting detail to a new page we need to reestablish the local BindingContext (otherwise we have ShellViewModel as total context)
                var context = ((ContentPage)newValue).BindingContext;
                if (context != null)
                    masterPage.DetailContainer.BindingContext = ((ContentPage)newValue).BindingContext;

                // close navigationpane if needed
                if (masterPage.GridNavigationPane.WidthRequest == 240)
                    masterPage.CloseMenu();

                // set activePage on newValue
                var button = newValue.GetType();
                var sl = masterPage.SideContentContainer.Children[0] as StackLayout;
                foreach (CustomNavigationButton element in sl.Children)
                {
                    element.IsActivePage = (element.ClassName == button.Name);
                }
                var slMore = masterPage.MoreOptionsContainer.Children[0] as StackLayout;
                foreach (CustomNavigationButton element in slMore.Children)
                {
                    element.IsActivePage = (element.ClassName == button.Name); 
                }

                // load Title from page into navigationbar
                // TODO: remove newValue bezug wenn alles passt
                var vm = masterPage.DetailContainer.BindingContext as BaseViewModel;
                if (Device.RuntimePlatform != Device.macOS)
                {
                    if (vm == null)
                        masterPage.Title = (newValue != null) ? ((ContentPage)newValue).Title : "";
                    else
                        masterPage.Title = (newValue != null) ? vm.Title : "";
                }

                masterPage.ToolbarItems.Clear();
                foreach (var item in ((ContentPage)newValue).ToolbarItems)
                    masterPage.ToolbarItems.Add(item);

                masterPage.OnPropertyChanged("SideContentVisible");
            });


        // method used in App.xaml.cs on initialization to create the NavigationPage with the correct menu from shell file
        public static Page Create<TView, TViewModel>() where TView : MasterDetailControl, new()
            where TViewModel : MasterDetailControlViewModel, new()
        {
            return Create<TView, TViewModel>(new TViewModel());
        }

        public static Page Create<TView, TViewModel>(TViewModel viewModel) where TView : MasterDetailControl, new()
            where TViewModel : MasterDetailControlViewModel
        {
            // creates a new navigationPage with the View defined in Shell and bind to ShellViewModel
            var masterDetail = new TView();
            var navigationPage = new NavigationPage(masterDetail);
            viewModel.SetNavigation(navigationPage.Navigation);
            masterDetail.BindingContext = viewModel;
            viewModel.NavigationAllowedChanged += Shell_NavigationAllowedChanged;
            viewModel.IsNavigationRunningChanged += Shell_IsNavigationRunningChanged;
            viewModel.ButtonClicked += Shell_ButtonClicked;
            return navigationPage;
        }

        static void Shell_NavigationAllowedChanged(object sender, EventArgs e)
        {
            // each time the property in viewmodel of the current page is changed, we take the new value and apply it to local property
            var vm = sender as MasterDetailControlViewModel;
            var nav = Application.Current.MainPage as NavigationPage;
            var main = nav.CurrentPage as MasterDetailControl;
            // enable or disable the backbutton if navigation is not allowed
            main.ButtonBackNavigation.IsEnabled = vm.IsBackButtonEnabled();
            main.ButtonBackNavigationLabel.IsEnabled = vm.IsBackButtonEnabled();
        }

        static void Shell_IsNavigationRunningChanged(object sender, EventArgs e)
        {
            var vm = sender as ShellViewModel;
            var nav = Application.Current.MainPage as NavigationPage;
            var main = nav.CurrentPage as MasterDetailControl;
            main.ButtonMoreOptions.IsRotating = vm.IsNavigationRunning;
        }

        static void Shell_ButtonClicked(object sender, EventArgs e)
        {
            // make flyout invisible after user has clicked one of the buttons
            var nav = Application.Current.MainPage as NavigationPage;
            var main = nav.CurrentPage as MasterDetailControl;
            if (main.GridFlyoutBase.IsVisible)
                main.GridFlyoutBase.IsVisible = false;
        }


        protected override bool OnBackButtonPressed()
        {
            var viewModel = BindingContext as MasterDetailControlViewModel;
            if (viewModel != null)
            {
                var navigation = (INavigation)viewModel;
                navigation.PopAsync();
                return true;
            }
            return base.OnBackButtonPressed();
        }

        void ButtonBackNavigation_Clicked(object sender, System.EventArgs e)
        {
            OnBackButtonPressed();
        }

        void ButtonHamburgerSymbol_Clicked(object sender, System.EventArgs e)
        {
            if (GridNavigationPane.WidthRequest == 48)
            {
                GridNavigationPane.WidthRequest = 240;
                GridTapGesture.IsVisible = true;
            }
            else
            {
                CloseMenu();
            }
        }

        private void CloseMenu()
        {
            GridNavigationPane.WidthRequest = 48;
            GridTapGesture.IsVisible = false;
        }

        private void SetGridTapGestureTap()
        {
            // when user taps anything around the buttons we want to close the flyout
            TapGestureRecognizer gridTap = new TapGestureRecognizer();
            gridTap.Tapped += (s,e) => { CloseMenu(); };
            GridTapGesture.GestureRecognizers.Add(gridTap);
        }

        void ButtonNavScrollUp_Clicked(object sender, System.EventArgs e)
        {
            var newPos = (ScrollViewNavigation.ScrollY - 48 > 0) ? ScrollViewNavigation.ScrollY - 48 : 0;
            ScrollViewNavigation.ScrollToAsync(0, newPos, true);
        }

        void ButtonNavScrollDown_Clicked(object sender, System.EventArgs e)
        {
            var bottom = ScrollViewNavigation.ContentSize.Height - ScrollViewNavigation.Bounds.Height;
            var newPos = (ScrollViewNavigation.ScrollY + 48 < bottom) ? ScrollViewNavigation.ScrollY + 48 : bottom;
            ScrollViewNavigation.ScrollToAsync(0, newPos, true);
        }

        void ScrollViewNavigation_Scrolled(object sender, Xamarin.Forms.ScrolledEventArgs e)
        {
            var scroll = sender as ScrollView;
            NavScrollUp.IsEnabled = (scroll.ScrollY > 48);
            var bottom = ScrollViewNavigation.ContentSize.Height - ScrollViewNavigation.Bounds.Height;
            NavScrollDown.IsEnabled = (scroll.ScrollY < bottom - 48);
        }

        void ButtonSettingsFlyout_Clicked(object sender, System.EventArgs e)
        {
            // fade in flyout when gear wheel button is clicked
            SetGridFlyoutBaseTap();
            GridFlyout.Opacity = 0;
            GridFlyoutBase.IsVisible = true;
            GridFlyout.FadeTo(1, 500);
        }

        private void SetGridFlyoutBaseTap()
        {
            // when user taps anything around the buttons we want to close the flyout
            TapGestureRecognizer gridTap = new TapGestureRecognizer();
            gridTap.Tapped += GridTap_Tapped;
            GridFlyoutBase.GestureRecognizers.Add(gridTap);
        }

        async void GridTap_Tapped(object sender, EventArgs e)
        {
            // fade out slowly before making the item invisible (necessary to avoid further taps)
            await GridFlyout.FadeTo(0, 500);
            GridFlyoutBase.IsVisible = false;
        }

        private void SetNavigationSide()
        {
            if (App.DeviceType == Xamarin.Essentials.DeviceIdiom.Phone)
            {
                if (Settings.NavigationOnRightSide && width < height)
                {
                    GridDetailContainer.Margin = new Thickness(0, 0, 48, 0);
                    GridNavigationPane.HorizontalOptions = LayoutOptions.End;
                    GridFlyout.HorizontalOptions = LayoutOptions.End;
                    GridFlyout.TranslationX = -24;
                }
                else
                {
                    GridDetailContainer.Margin = new Thickness(48, 0, 0, 0);
                    GridNavigationPane.HorizontalOptions = LayoutOptions.Start;
                    GridFlyout.HorizontalOptions = LayoutOptions.Start;
                    GridFlyout.TranslationX = 24;
                }
            }
        }



        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != this.width || height != this.height)
            {
                this.width = width;
                this.height = height;
                App.ShellWidth = width;
                App.ShellHeight = height;
                SetNavigationSide();
            }
        }
    }
}
