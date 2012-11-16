using System;
using System.Windows.Input;

namespace ServiceWatcher.Commands
{
	public class RelayCommand<T> : ICommand
	{
		private readonly Predicate<T> myCanExecute;
        private readonly Action<T> myAction;

		public RelayCommand(Action<T> action)
			: this(action, null)
		{
		}

		public RelayCommand(Action<T> action, Predicate<T> canExecute)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}

			myAction = action;
			myCanExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
		    if (parameter != null)
		    {
				var param = GetTypedParameter(parameter);
                if (param != null)
                {
                    return myCanExecute == null ? true : myCanExecute(param);
                }
		    }
			return false;
		}

		public virtual void Execute(object parameter)
		{
			var param = GetTypedParameter(parameter);
			if (param != null)
			{
				myAction(param);
			}
			else
			{
				throw new InvalidOperationException(String.Format("Invalid parameter type for Command, Parameter type was {0}, expected type is {1}", parameter.GetType().FullName, typeof(T).FullName));
			}
		}

		private T GetTypedParameter(object parameter)
		{
			var param = parameter as IConvertible;
			if (param != null)
			{
				//ChangeType allows string parameters to be cast to the correct type.
				return (T)Convert.ChangeType(parameter, typeof(T), null);
			}
			return (T)parameter;
		}

		public event EventHandler CanExecuteChanged;

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