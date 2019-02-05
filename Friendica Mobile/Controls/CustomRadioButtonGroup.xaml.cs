using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    public partial class CustomRadioButtonGroup : ContentView
    {
        // prepare Bindable Property for "IsBordered" 
        public static readonly BindableProperty IsBorderedProperty = BindableProperty.Create("IsBordered",
                                                            typeof(bool), typeof(CustomRadioButtonGroup),
                                                            true, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
        {
            (bindable as CustomRadioButtonGroup).FrameRadioButtonGroup.OutlineColor = ((bool)newValue) ? Color.LightGray : Color.Transparent;
            (bindable as CustomRadioButtonGroup).FrameRadioButtonGroup.HasShadow = (bool)newValue;
        });

        // prepare Bindable Property for "Orientation" 
        public static readonly BindableProperty OrientationProperty = BindableProperty.Create("Orientation",
                                                            typeof(StackOrientation), typeof(CustomRadioButtonGroup),
                                                            StackOrientation.Vertical, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
        {
            (bindable as CustomRadioButtonGroup).StackRadioButtons.Orientation = (StackOrientation)newValue;
            (bindable as CustomRadioButtonGroup).StackRadioButtons.Spacing = ((StackOrientation)newValue == StackOrientation.Vertical) ? 0 : 12;
        });

        // prepare Bindable Property for "GroupName" 
        public static readonly BindableProperty GroupNameProperty = BindableProperty.Create("GroupName",
                                                            typeof(string), typeof(CustomRadioButtonGroup),
                                                            "", BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                            });

        // prepare Bindable Property for "CanDeselectAll" 
        public static readonly BindableProperty CanDeselectAllProperty = BindableProperty.Create("CanDeselectAll",
                                                            typeof(bool), typeof(CustomRadioButtonGroup),
                                                            false, BindingMode.OneWay);

        // prepare Bindable Property for "RadioButtons" 
        public static readonly BindableProperty RadioButtonsProperty = BindableProperty.Create("RadioButtons",
                                                            typeof(ObservableCollection<CustomRadioButton>), typeof(CustomRadioButtonGroup),
                                                            null, BindingMode.OneWay);                                                         

        // prepare Bindable Property for "SelectedIndex" 
        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create("SelectedIndex",
                                                            typeof(int), typeof(CustomRadioButtonGroup),
                                                            -2, BindingMode.TwoWay,
                                                            propertyChanged: (bindable, value, newValue) =>
        {
            // avoid illegal index settings
            var group = (bindable as CustomRadioButtonGroup);
            try
            {
                CustomRadioButton item;
                if ((int)newValue != -1)
                    item = group.RadioButtons[(int)newValue];
            }
            catch { return; }
            if (group.RadioButtons.Count < (int)newValue)
                return;

            // avoid changing things if there is no change
            if (value != newValue)
            {
                if ((int)newValue == -1)
                    group.SetInactiveButtons();
                else
                    group.SetActiveButtons((int)newValue);
            }
        });

        // prepare Bindable Property for "SelectedItem" 
        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create("SelectedItem",
                                                            typeof(CustomRadioButton), typeof(CustomRadioButtonGroup),
                                                                                               null, BindingMode.OneWay,
                                                            propertyChanged: (bindable, value, newValue) =>
        {
        });



        public bool IsBordered
        {
            get { return (bool)GetValue(IsBorderedProperty); }
            set { SetValue(IsBorderedProperty, value); }
        }

        public StackOrientation Orientation
        {
            get { return (StackOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // holding a language neutral reference name for complete group, if needed
        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public bool CanDeselectAll
        {
            get { return (bool)GetValue(CanDeselectAllProperty); }
            set { SetValue(CanDeselectAllProperty, value); }
        }

        public ObservableCollection<CustomRadioButton> RadioButtons
        {
            get { return (ObservableCollection<CustomRadioButton>)GetValue(RadioButtonsProperty); }
            set { SetValue(RadioButtonsProperty, value); }
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public CustomRadioButton SelectedItem
        {
            get { return (CustomRadioButton)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }





        public CustomRadioButtonGroup()
        {
            InitializeComponent();
            // initialize the collection of items and listen to changes
            RadioButtons = new ObservableCollection<CustomRadioButton>();
            RadioButtons.CollectionChanged += RadioButtons_CollectionChanged;
        }


        void RadioButtons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // rebuild the collection each time the item list is changed
            var list = sender as ObservableCollection<CustomRadioButton>;
            if (StackRadioButtons.Children.Count > 0)
                this.StackRadioButtons.Children.Clear();

            foreach (var item in list)
            {
                // remove existing click event handlers on the items before adding to avoid multiple event firings
                item.RadioButtonClicked -= Item_TabHeaderClicked;
                item.RadioButtonClicked += Item_TabHeaderClicked;
                this.StackRadioButtons.Children.Add(item);
            }
        }

        private int GetItemId(CustomRadioButton button)
        {
            return RadioButtons.IndexOf(button);
        }

        void Item_TabHeaderClicked(object sender, EventArgs e)
        {
            var clickedItem = sender as CustomRadioButton;
            var index = GetItemId(clickedItem);
            if (SelectedIndex == index)
            {
                if (clickedItem.IsChecked)
                    SetInactiveButtons();
                else
                    SetActiveButtons(index);
            }
            else
                SelectedIndex = index;
        }

        void SetActiveButtons(int index)
        {
            SelectedItem = RadioButtons[SelectedIndex];

            foreach (var button in RadioButtons)
            {
                button.IsChecked = (RadioButtons.IndexOf(button) == index);
            }
        }

        void SetInactiveButtons()
        {
            if (CanDeselectAll)
            {
                foreach (var button in RadioButtons)
                    button.IsChecked = false;
                SelectedIndex = -1;
            }
            else
            {
                foreach (var button in RadioButtons)
                {
                    if (button.IsEnabled)
                    {
                        button.IsChecked = true;
                        break;
                    }
                }
            }
        }
       
    }
}

