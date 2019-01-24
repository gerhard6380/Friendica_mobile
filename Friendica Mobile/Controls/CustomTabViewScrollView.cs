using System;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    /// <summary>
    /// separate derived ScrollView to remove the scroll bars on Android only in the CustomTabView
    /// see ScrollbarDisabledRenderer in Android platform project, same for UWP (Windows 10 Mobile)
    /// </summary>
    public class CustomTabViewScrollView : ScrollView
    {
    }
}
