using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Friendica_Mobile.Styles
{
    public partial class CustomCommandBar : ContentView
    {
        List<CustomCommandBarItem> _listPrimary = new List<CustomCommandBarItem>();
        List<CustomCommandBarItem> _listOverflowPrimaries = new List<CustomCommandBarItem>();
        List<CustomCommandBarItem> _listSecondary = new List<CustomCommandBarItem>();
        public enum BarPositions { Top, Bottom, Default }


        // prepare Bindable Property for the Bar position 
        public static readonly BindableProperty CommandBarPositionProperty = BindableProperty.Create("CommandBarPosition",
                                                            typeof(BarPositions), typeof(CustomCommandBar),
                                                            SetDefaultPosition(), BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                (bindable as CustomCommandBar).OnCommandBarPositionChanged?.Invoke(bindable, EventArgs.Empty);
                                                            });

        // prepare Bindable Property for the command items 
        public static readonly BindableProperty CommandListProperty = BindableProperty.Create("CommandList",
                                                            typeof(ObservableCollection<CustomCommandBarItem>), typeof(CustomCommandBar),
                                                            null, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                (bindable as CustomCommandBar).SortCommandList();
                                                                (bindable as CustomCommandBar).DisplayPrimaryCommands();
                                                            });


        // properties to be used in XAML
        public BarPositions CommandBarPosition
        {
            get { return (BarPositions)GetValue(CommandBarPositionProperty); }
            set { SetValue(CommandBarPositionProperty, value); }
        }
        public ObservableCollection<CustomCommandBarItem> CommandList
        {
            get { return (ObservableCollection<CustomCommandBarItem>)GetValue(CommandListProperty); }
            set { SetValue(CommandListProperty, value); }
        }


        // events of the command bar
        public event EventHandler OnCommandBarPositionChanged;
        public event EventHandler MoreOptionsButtonClicked;




        public CustomCommandBar()
        {
            InitializeComponent();
            CommandList = new ObservableCollection<CustomCommandBarItem>();
            App.ShellSizeChanged += (object sender, EventArgs e) =>
            {
                SortCommandList();
                DisplayPrimaryCommands();
            };
        }


        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == "IsVisible")
                OnCommandBarPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SortCommandList()
        {
            if (CommandList != null && CommandList.Count > 0)
            {
                _listPrimary = new List<CustomCommandBarItem>();
                _listOverflowPrimaries = new List<CustomCommandBarItem>();
                _listSecondary = new List<CustomCommandBarItem>();

                // how many commands can we display in available space?
                int maxPrimary = 0;
                if (App.ShellWidth != 0)
                    maxPrimary = Convert.ToInt32(Math.Floor((App.ShellWidth - 48) / 72));
                else
                    maxPrimary = 4;

                // separate primary buttons from secondaries
                foreach (var item in CommandList)
                {
                    if (item.Level == CustomCommandBarItem.CommandLevel.Primary && item.IsVisible)
                    {
                        item.IsHorizontal = false;
                        _listPrimary.Add(item);
                    }
                    else
                    {
                        item.IsHorizontal = true;
                        _listSecondary.Add(item);
                    }
                }

                // remove lower priority buttons if there is not enough space
                while (_listPrimary != null && _listPrimary.Count > maxPrimary)
                {
                    var lowestPrio = _listPrimary.Where(x => x.Priority == _listPrimary.Min(i => i.Priority)).ToList();
                    foreach (var item in lowestPrio)
                        _listOverflowPrimaries.Add(item);
                    _listPrimary = _listPrimary.Where(x => x.Priority != _listPrimary.Min(i => i.Priority)).ToList();
                }

                // set the overflowing elements to horizontal display of text
                foreach (var item in _listOverflowPrimaries)
                    item.IsHorizontal = true;
            }
        }

        private void DisplayPrimaryCommands()
        {
            // change to right side on phone devices in portrait mode
            if (App.DeviceType == Plugin.DeviceInfo.Abstractions.Idiom.Phone)
            {
                if (Settings.NavigationOnRightSide && App.ShellWidth < App.ShellHeight)
                    StackLayoutPrimaryCommandsContainer.HorizontalOptions = LayoutOptions.End;
                else
                    StackLayoutPrimaryCommandsContainer.HorizontalOptions = LayoutOptions.Start;
            }

            // add the buttons to the primary item container
            StackLayoutPrimaryCommandsContainer.Children.Clear();
            foreach (var item in _listPrimary)
            {
                item.IsHorizontal = false;
                StackLayoutPrimaryCommandsContainer.Children.Add(item);
            }
        }

        public List<CustomCommandBarItem> GetSecondaryElements()
        {
            return _listOverflowPrimaries.Concat(_listSecondary).ToList();
        }


        private static BarPositions SetDefaultPosition()
        {
            return (App.DeviceType == Plugin.DeviceInfo.Abstractions.Idiom.Phone) ? CustomCommandBar.BarPositions.Bottom : CustomCommandBar.BarPositions.Top;
        }


        void ButtonMoreOptions_Clicked(object sender, System.EventArgs e)
        {
            // fire event to change the available space in BaseContentPage
            MoreOptionsButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
