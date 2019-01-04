using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class FlowDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // return the righttoleft on mobiledevices when user is right-handed and lefttoright on others

            // sometimes on loading app (especially in sample mode) it can happen that the DeviceFamily is not yet set, so define it here before continuing
            if (App.Settings.DeviceFamily == null)
            {
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily"))
                    App.Settings.DeviceFamily = qualifiers["DeviceFamily"];
            }

            if (App.Settings.DeviceFamily == "Mobile")
            {
                if (App.Settings.NavigationSide == "RightToLeft")
                    return FlowDirection.RightToLeft;
                else
                    return FlowDirection.LeftToRight;
            }
            else
                return FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
