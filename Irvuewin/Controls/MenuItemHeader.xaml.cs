using System.Windows;
using System.Windows.Controls;

namespace Irvuewin.Controls;

public partial class MenuItemHeader : UserControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(HeaderName),
            typeof(string),
            typeof(MenuItemHeader),
            new PropertyMetadata(string.Empty));
    
    public string HeaderName
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public MenuItemHeader()
    {
        InitializeComponent();
    }
}