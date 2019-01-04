using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Friendica_Mobile.UWP
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class PreviewUnavailable : Page
    {
        public PreviewUnavailable()
        {
            this.InitializeComponent();
        }

        public PreviewUnavailable(Size paperSize, Size printableSize) : this()
        {
            Page.Width = paperSize.Width;
            Page.Height = paperSize.Height;
            PrintablePage.Width = printableSize.Width;
            PrintablePage.Height = PrintablePage.Height;
        }

    }
}
