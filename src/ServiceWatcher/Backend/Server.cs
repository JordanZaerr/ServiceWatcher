using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ServiceWatcher.Backend
{
	/// <summary>
	/// Represents a server and its services and process that should be monitored
	/// </summary>
	public class Server : INotifyPropertyChanged
	{
		private ObservableCollection<Service> myServices;
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets or sets the management objects for this server.
		/// </summary>
		/// <value>The management objects.</value>
		public ObservableCollection<ManagementObjectBase> ManagementObjects
		{
			get
			{
				return
					new ObservableCollection<ManagementObjectBase>(
						Services.Cast<ManagementObjectBase>());
			}
		}

		/// <summary>
		/// Gets or sets the name of the server.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the services.
		/// </summary>
		/// <value>The services.</value>
		public ObservableCollection<Service> Services
		{
			get { return myServices; }
			set
			{
				myServices = value;
				if (myServices != null)
				{
					foreach (Service svc in myServices)
					{
						svc.PropertyChanged += OnServiceObjectChanged;
					} 
				}
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("ServerName: " + Name + "\r\n");
			foreach (ManagementObjectBase svc in ManagementObjects)
			{
				sb.AppendLine(svc.Name + " is running: " + svc.IsRunning);
			}
			return sb.ToString();
		}

		private void OnServiceObjectChanged(object sender, PropertyChangedEventArgs e)
		{
			RaisePropertyChanged("Services");
			RaisePropertyChanged("ManagementObjects");
		}

		private void RaisePropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}