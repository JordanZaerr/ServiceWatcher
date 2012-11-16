using System.Windows;
using ServiceWatcher.Backend;

namespace ServiceWatcher
{
	public partial class Main : Window
	{
		public Main()
		{
			InitializeComponent();
			uxServers.SelectedItemChanged += (x, y) => uxServers.Items.MoveCurrentTo(y.OldValue);
            ViewModel.Dispatcher = this.Dispatcher;
		}

        public MainViewModel ViewModel { get { return (MainViewModel)this.DataContext; } }

        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var service = e.NewValue as Service;
            if (service != null)
            {
                ViewModel.SelectedService = service;
            }
        }
	}
}