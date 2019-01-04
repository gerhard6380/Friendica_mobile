using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Friendica_Mobile.UWP
{
    public class RichTextBlockHelper : DependencyObject
    {
        // class needed to add the functionality to RichTextBlock that we can bind the Content property to the DataContext
        // after loading data we prepare the Content within the ViewModel into a paragraph with runs which will be added as the RichtextBlock-Content

        public static Paragraph GetContent(DependencyObject obj)
        {
            return (Paragraph)obj.GetValue(ContentProperty);
        }

        public static void SetContent(DependencyObject obj, Paragraph value)
        {
            obj.SetValue(ContentProperty, value);
        }


        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.RegisterAttached("Content", typeof(Paragraph),
                typeof(RichTextBlockHelper), new PropertyMetadata(new Paragraph(), OnContentChanged));


        private static void OnContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as RichTextBlock;
            if (control != null)
            {
                Paragraph value = e.NewValue as Paragraph;
                control.Blocks.Clear();
                if (value.Inlines.Count != 0)
                    control.Blocks.Add(value);
            }
        }
    }
}
