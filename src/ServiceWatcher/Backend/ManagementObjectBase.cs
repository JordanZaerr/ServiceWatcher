using System.ComponentModel;
using System.Management;

namespace ServiceWatcher.Backend
{
	public abstract class ManagementObjectBase : INotifyPropertyChanged
	{
		private ManagementObject myManagementObj;
		private string myName;

	    protected ManagementObjectBase()
		{
		}

	    protected ManagementObjectBase(string displayName, string name)
		{
			Name = name;
			DisplayName = displayName;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public string DisplayName { get; set; }
		public virtual bool IsRunning { get; private set; }

		public ManagementObject ManagementObj
		{
			get { return myManagementObj; }
			set
			{
				myManagementObj = value;
                RaisePropertyChanged(null);
			}
		}

		public string Name
		{
			get { return myName; }
			private set
			{
				myName = value;
				RaisePropertyChanged("Name");
			}
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