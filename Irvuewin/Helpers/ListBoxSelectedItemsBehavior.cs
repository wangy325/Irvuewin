using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Irvuewin.Helpers
{
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
                    listBox.SelectedItems.Clear();
                    foreach (var item in newList)
                    {
                        listBox.SelectedItems.Add(item);
                    }
                }
            }
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                var bindableSelectedItems = GetBindableSelectedItems(listBox);
                if (bindableSelectedItems == null) return;

                bindableSelectedItems.Clear();
                foreach (var item in listBox.SelectedItems)
                {
                    bindableSelectedItems.Add(item);
                }
            }
        }
    }
}