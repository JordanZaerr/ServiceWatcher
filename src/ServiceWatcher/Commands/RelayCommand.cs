using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ServiceWatcher.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> myCanExecute;
        private readonly Action myAction;

        public RelayCommand(Action action)
            : this(action, null)
        {
        }

        public RelayCommand(Action action, Predicate<object> canExecute)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            myAction = action;
            myCanExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return myCanExecute == null ? true : myCanExecute(parameter);
        }

		[DebuggerStepThrough]
        public void Execute(object parameter)
        {
            myAction();
        }

		protected virtual void OnCanExecuteChanged(EventArgs e)
		{
			var canExecuteChanged = CanExecuteChanged;
			if (canExecuteChanged != null)
			{
				canExecuteChanged(this, e);
			}
		}
		public void RaiseCanExecuteChanged()
		{
			OnCanExecuteChanged(EventArgs.Empty);
		}
    }
}