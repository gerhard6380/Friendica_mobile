using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Friendica_Mobile.UWP.Triggers
{
    public class ContinuumTrigger : StateTriggerBase
    {
        public string UIMode
        {
            get { return (string)GetValue(UIModeProperty);  }
            set { SetValue(UIModeProperty, value);  }
        }

        public static readonly DependencyProperty UIModeProperty = 
            DependencyProperty.Register("UIMode", typeof(string), typeof(ContinuumTrigger), new PropertyMetadata(""));

        public ContinuumTrigger()
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
                    var currentUIMode =
                    Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode.ToString();
                    App.Settings.UIMode = currentUIMode;
                    SetActive(currentUIMode == UIMode);
                };
                Window.Current.Activated += windowactivated;
                Window.Current.SizeChanged += Current_SizeChanged;
            }
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            var currentUIMode =
                Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode.ToString();
            App.Settings.UIMode = currentUIMode;
            SetActive(currentUIMode == UIMode);
        }
    }
}
