using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Friendica_Mobile.UWP.Triggers
{
    class VisualStateSelector
    {
        public VisualStateSelector(Control page)
        {
            switch (App.Settings.OrientationDevice)
            {
                case OrientationDeviceFamily.MobilePortrait:
                    VisualStateManager.GoToState(page, "MobilePortrait", true);
                    break;
                case OrientationDeviceFamily.MobileLandscape:
                    VisualStateManager.GoToState(page, "MobileLandscape", true);
                    break;
                default:
                    VisualStateManager.GoToState(page, "Desktop", true);
                    break;
            }
        }

        public void VisualStateSelectorPhotosFlipView(Control page)
        {
            switch (App.Settings.OrientationDevice)
            {
                case OrientationDeviceFamily.MobilePortrait:
                    VisualStateManager.GoToState(page, "PhotoCommandsHorizontal", true);
                    break;
                case OrientationDeviceFamily.DesktopPortrait:
                    VisualStateManager.GoToState(page, "PhotoCommandsHorizontal", true);
                    break;
                default:
                    VisualStateManager.GoToState(page, "PhotoCommandsVertical", true);
                    break;
            }
        }

    }
}
