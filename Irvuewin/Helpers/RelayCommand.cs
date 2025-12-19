using System.Windows.Input;

namespace Irvuewin.Helpers;

    public class RelayCommand<T>(Action<object> execute, Predicate<object>? canExecute = null)
        : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly Action<object>? _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute?.Invoke(parameter);
        }
    }