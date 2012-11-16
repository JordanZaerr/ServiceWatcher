using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ServiceWatcher.Backend;
using ServiceWatcher.Commands;

namespace ServiceWatcher
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            StartCommand = new RelayCommand(Start);
            StopCommand = new RelayCommand(Stop);
            KillCommand = new RelayCommand(Kill);
            RefreshCommand = new RelayCommand(Refresh);
            Task.Factory.StartNew(() =>
            {
                Servers = new ObservableCollection<Server>(Operations.LoadServers());
                foreach (Server server in Servers)
                {
                    server.Services.CollectionChanged += (s, e) => RaisePropertyChanged("Servers");
                }
                IsLoaded = true;
                RaisePropertyChanged("Servers");
                RaisePropertyChanged("IsLoaded");
            });
        }

        public bool IsLoaded { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public Dispatcher Dispatcher { get; set; }

        public ICommand KillCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public ObservableCollection<Server> Servers { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand StopCommand { get; set; }

        public Service SelectedService { get; set; }

        private string GetServerName(ManagementObjectBase obj)
        {
            string serverName =
                Servers.First(x => x.Services.Select(s => s.Name).Contains(obj.Name)).Name;
            return serverName;
        }

        private void Kill()
        {
            if (SelectedService != null)
            {
                Task.Factory.StartNew(() =>
                {
                    string machineName = GetServerName(SelectedService);
                    var observer = new ManagementOperationObserver();
                    observer.Completed += RaiseRefreshApps;
                    Operations.KillProcess(machineName, SelectedService.ManagementObj, observer);
                })
                .ContinueWith(t => DisplayMessage(t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(t => Refresh());
            }
        }

        private void Refresh()
        {
            Task.Factory.StartNew(() => Operations.RefreshServers(Servers))
                .ContinueWith(t => Dispatcher.Invoke(new Action<string>(RaisePropertyChanged), "Servers"));
        }

        private void Start()
        {
            if (SelectedService != null && !SelectedService.IsRunning)
            {
                Task.Factory.StartNew(() =>
                {
                    var observer = new ManagementOperationObserver();
                    observer.Completed += RaiseRefreshApps;
                    Operations.StartService(SelectedService.ManagementObj, observer);
                })
                .ContinueWith(t => DisplayMessage(t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(t => Refresh());
            }
        }

        private void Stop()
        {
            if (SelectedService != null && SelectedService.IsRunning)
            {
                Task.Factory.StartNew(() =>
                {
                    var observer = new ManagementOperationObserver();
                    observer.Completed += RaiseRefreshApps;
                    Operations.StopService(SelectedService.ManagementObj, observer);
                })
                .ContinueWith(t => DisplayMessage(t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(t => Refresh());
            }
        }

        private void DisplayMessage(string message)
        {
            Dispatcher.Invoke(new Func<string, MessageBoxResult>(MessageBox.Show), message);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void RaiseRefreshApps(object sender, CompletedEventArgs e)
        {
            var observer = sender as ManagementOperationObserver;
            if (observer != null)
            {
                observer.Completed -= RaiseRefreshApps;
            }
            Refresh();
        }
    }
}