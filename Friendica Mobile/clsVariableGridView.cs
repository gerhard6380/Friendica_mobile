using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Friendica_Mobile
{
    public class clsVariableGridView : GridView
    {
        // change standard GridView to include RowSpan property from UserControl and update GridView on each size change
        public bool IsUpdating = false;

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            dynamic model = item;
            try
            {
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.ColumnSpanProperty, model.ColSpan);
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.RowSpanProperty, model.RowSpan);
            }
            catch
            {
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.ColumnSpanProperty, 1);
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.RowSpanProperty, 1);
            }
            finally
            {
                element.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
                element.SetValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                base.PrepareContainerForItemOverride(element, item);
            }
        }

        // refresh the variablesizedwrapgrid layout, called on size changes of each usercontrol and after loading data from server
        public void Update()
        {
            if ((this.ItemsPanelRoot is VariableSizedWrapGrid))
            {    //throw new ArgumentException("ItemsPanel is not VariableSizedWrapGrid");
                IsUpdating = true;
                foreach (var container in this.ItemsPanelRoot.Children.Cast<GridViewItem>())
                {
                    dynamic data = container.Content;
                    // calculate the number of rows to be used in displaying information (row height = 12), +24 = include margins
                    int _rowSpan = (int)Math.Ceiling((data.Content.Content.ActualHeight + 24) / 12);
                    int _columnSpan = (int)Math.Ceiling((data.Content.ActualWidth) / 12);
                    VariableSizedWrapGrid.SetRowSpan(container, _rowSpan);
                    VariableSizedWrapGrid.SetColumnSpan(container, _columnSpan);
                }

                this.ItemsPanelRoot.InvalidateMeasure();
                this.ItemsPanelRoot.LayoutUpdated += ItemsPanelRoot_LayoutUpdated;
            }
        }

        private void ItemsPanelRoot_LayoutUpdated(object sender, object e)
        {
            IsUpdating = false;
        }
    }
}
