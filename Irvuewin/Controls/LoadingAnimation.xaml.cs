using System.Windows;
using System.Windows.Controls;

namespace Irvuewin.Controls;

public partial class LoadingAnimation : UserControl
{
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(LoadingAnimation),
            new PropertyMetadata(false));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public LoadingAnimation()
    {
        InitializeComponent();
    }
}