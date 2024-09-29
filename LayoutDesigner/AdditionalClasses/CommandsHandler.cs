using System.Windows.Input;

namespace LayoutDesigner.AdditionalClasses
{
    public sealed class CommandsHandler : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public CommandsHandler(Action<object> execute, Func<object, bool> canExecute1 = null)
        {
            this._execute = execute;
            this._canExecute = canExecute1;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        { 
            _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
