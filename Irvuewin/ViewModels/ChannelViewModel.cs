using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.ViewModels;

///<summary>
///Author: wangy325
///Date: 2020/01/01 18:18:18
///Desc: 
///</summary>
public class ChannelViewModel : UnsplashChannel, INotifyPropertyChanged
{
    private bool _isSelected;
    public bool IsSelected { get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                if (_isSelected)
                {
                    // TODO 通知其他channel取消选择
                        
                }
            }
        }
    } 
    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand ItemSelected { set; get; }


    private void OnTrayChannelSelected(object param)
    {
        
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}