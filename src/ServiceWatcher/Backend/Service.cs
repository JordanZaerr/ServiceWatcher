namespace ServiceWatcher.Backend
{
	public class Service : ManagementObjectBase
	{
		public Service()
		{
			
		}

		public Service(string displayName, string name)
			: base(displayName, name)
		{

		}

		public override bool IsRunning
		{
			get
			{
				if (ManagementObj != null)
				{
					return ManagementObj["State"].ToString() == "Running";
				}
				return false;
			}
		}
	}
}