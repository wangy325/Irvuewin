using System.Windows.Input;
using Irvuewin.Helpers;

namespace Irvuewin.ViewModels;

public class TrayViewModel
{
    public ICommand TrayMenuOpened { set; get; } = new RelayCommand<object>(CheckDisplayCommand);

    private static void CheckDisplayCommand(object param)
    {
        TrayMenuHelper.CheckPointer(param);
    }

    public ICommand LoadCachedSequence { get; } = new RelayCommand<object>(OnLoadCachedSequence);
    public ICommand SaveCachedSequence { get; } = new RelayCommand<object>(OnSaveCachedSequence);
    
    private static void OnLoadCachedSequence(object obj)
    {
        // Load Cached resources
        TrayMenuHelper.LoadCachedSequence();
        Properties.Settings.Default.Save();
    }

    private static void OnSaveCachedSequence(object obj)
    {
        // Save Cache
        TrayMenuHelper.SaveCachedSequence();
        Properties.Settings.Default.Save();
    }

}