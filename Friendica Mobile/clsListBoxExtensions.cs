using Friendica_Mobile.Models;
using System;
using System.Collections;
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

namespace Friendica_Mobile
{
    public class clsListBoxExtensions : DependencyObject
    {
        // class needed to add the functionality to RichTextBlock that we can bind the Content property to the DataContext
        // after loading data we prepare the Content within the ViewModel into a paragraph with runs which will be added as the RichtextBlock-Content

        public static readonly DependencyProperty SelectedItemListProperty =
                DependencyProperty.RegisterAttached("SelectedItemList", typeof(IList),
    typeof(clsListBoxExtensions), new PropertyMetadata(new List<FriendicaUser>(), OnSelectedItemListChanged));

        
        public static IList GetSelectedItemList(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemListProperty);
        }

        public static void SetSelectedItemList(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemListProperty, value);
        }

        private static void OnSelectedItemListChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var gridview = sender as GridView;
            if (gridview != null)
            {
                gridview.SelectedItems.Clear();
                var selectedItems = e.NewValue as IList;
                if (selectedItems != null)
                {
                    foreach (var item in selectedItems)
                        gridview.SelectedItems.Add(item);
                }
                gridview.UpdateLayout();
            }
        }
    }
}
