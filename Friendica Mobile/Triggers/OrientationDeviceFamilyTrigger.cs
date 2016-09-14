using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Friendica_Mobile;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Controls;

namespace Friendica_Mobile.Triggers
{
    public class OrientationDeviceFamilyTrigger : StateTriggerBase
    {

        public OrientationDeviceFamily OrientationDevice
        {
            get { return ( OrientationDeviceFamily)GetValue(OrientationDeviceProperty); }
            set { SetValue(OrientationDeviceProperty, value); }
        }

        public static readonly DependencyProperty OrientationDeviceProperty =
            DependencyProperty.Register("OrientationDevice", typeof( OrientationDeviceFamily), typeof(OrientationDeviceFamilyTrigger), new PropertyMetadata(""));

        public OrientationDeviceFamilyTrigger()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                WindowActivatedEventHandler windowactivated = null;
                windowactivated = (s, e) =>
                {
                    Window.Current.Activated -= windowactivated;
                    Setter();
                };
                Window.Current.Activated += windowactivated;
                Window.Current.SizeChanged += Current_SizeChanged;
                App.NavigationCompleted += App_NavigationCompleted;
                App.Settings.PropertyChanged += Settings_PropertyChanged;
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShellWidth")
                Setter();
        }

        private void App_NavigationCompleted(object sender, EventArgs e)
        {
            Setter();
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Setter();
        }

        public void SetterManual()
        {
            Setter();
        }

        private void Setter()
        {
            var currentOrientation = ApplicationView.GetForCurrentView().Orientation;

            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
            string currentDeviceFamily = "";
            if (qualifiers.ContainsKey("DeviceFamily"))
                currentDeviceFamily = qualifiers["DeviceFamily"];

            OrientationDeviceFamily currentOrientationDevice;
            if (currentDeviceFamily == "Mobile" && currentOrientation == ApplicationViewOrientation.Landscape)
                currentOrientationDevice = OrientationDeviceFamily.MobileLandscape;
            else if (currentDeviceFamily == "Mobile" && currentOrientation == ApplicationViewOrientation.Portrait)
                currentOrientationDevice = OrientationDeviceFamily.MobilePortrait;
            else if (currentDeviceFamily == "Desktop" && currentOrientation == ApplicationViewOrientation.Landscape)
            {
                Views.Shell shell = (Views.Shell)Window.Current.Content;
                var actualwidth = shell.ActualWidth;
                if (actualwidth < 810)
                    currentOrientationDevice = OrientationDeviceFamily.DesktopPortrait;
                else
                    currentOrientationDevice = OrientationDeviceFamily.DesktopLandscape;
            }
            else
                currentOrientationDevice = OrientationDeviceFamily.DesktopPortrait;

            App.Settings.Orientation = currentOrientation;
            App.Settings.DeviceFamily = currentDeviceFamily;
            App.Settings.OrientationDevice = currentOrientationDevice;

            SetActive(currentOrientationDevice == OrientationDevice);
        }
    }


}
