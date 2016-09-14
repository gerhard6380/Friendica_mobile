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
    public class clsListBoxExtensionsContacts : DependencyObject
    {
        // class needed to add the functionality to ListBox that we can bind the SelectedItems property to the DataContext

        public static readonly DependencyProperty SelectedItemListProperty =
                DependencyProperty.RegisterAttached("SelectedItemList", typeof(IList),
    typeof(clsListBoxExtensionsContacts), new PropertyMetadata(new List<FriendicaUserExtended>(), OnSelectedItemListChanged));

        
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
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                listBox.SelectedItems.Clear();
                var selectedItems = e.NewValue as IList;
                if (selectedItems != null)
                {
                    foreach (var item in selectedItems)
                        listBox.SelectedItems.Add(item);
                }
            }
        }
    }
}
