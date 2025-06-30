using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Irvuewin.Helpers
{
    // Multiple ListBox item selected binding tools
    public static class ListBoxSelectedItemsBehavior
    {
        public static readonly DependencyProperty BindableSelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "BindableSelectedItems",
                typeof(IList),
                typeof(ListBoxSelectedItemsBehavior),
                new PropertyMetadata(null, OnBindableSelectedItemsChanged));

        public static void SetBindableSelectedItems(DependencyObject element, IList value)
        {
            element.SetValue(BindableSelectedItemsProperty, value);
        }

        public static IList GetBindableSelectedItems(DependencyObject element)
        {
            return (IList)element.GetValue(BindableSelectedItemsProperty);
        }

        private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                listBox.SelectionChanged -= ListBox_SelectionChanged;
                listBox.SelectionChanged += ListBox_SelectionChanged;

                if (e.NewValue is IList newList)
                {
                    /*listBox.SelectedItems.Clear();
                    foreach (var item in newList)
                    {
                        listBox.SelectedItems.Add(item);
                    }*/
                    // 创建快照副本
                    var snapshot = newList.Cast<object?>().ToList();

                    listBox.SelectedItems.Clear();
                    foreach (var item in snapshot)
                    {
                        listBox.SelectedItems.Add(item);
                    }
                }
            }
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox) return;
            var bindableSelectedItems = GetBindableSelectedItems(listBox);

            bindableSelectedItems.Clear();
            foreach (var item in listBox.SelectedItems)
            {
                bindableSelectedItems.Add(item);
            }
        }
    }
}