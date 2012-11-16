using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Xml.Linq;

namespace ServiceWatcher.Backend
{
	/// <summary>
	/// Operations that can be used to obtain / act on services or processes
	/// </summary>
	public class Operations
	{
		private static ManagementObject GetProcess(string machineName, string processId)
		{
			var scope = new ManagementScope("\\\\" + machineName + "\\root\\cimv2");
			var getService = new ObjectQuery(
				"Select * from Win32_Process where ProcessId = '" + processId + "'");
			var search = new ManagementObjectSearcher(scope, getService);
			ManagementObjectCollection services = search.Get();
			foreach (ManagementObject service in services)
			{
				return service;
			}
			return null;
		}

		/// <summary>
		/// Gets the <see cref="ManagementObject"/> that represents this service.
		/// </summary>
		/// <param name="machineName">Name of the machine.</param>
		/// <param name="serviceName">Name of the service.</param>
		/// <returns></returns>
		public static ManagementObject GetService(string machineName, string serviceName)
		{
			var scope = new ManagementScope("\\\\" + machineName + "\\root\\cimv2");
			var getService =
				new ObjectQuery("Select Name, State, ProcessId from Win32_Service where Name = '" + serviceName + "'");
			var search = new ManagementObjectSearcher(scope, getService);
			ManagementObjectCollection services = search.Get();
			foreach (ManagementObject service in services)
			{
				return service;
			}
			return null;
		}

		/// <summary>
		/// Kills the process.
		/// </summary>
		/// <param name="machineName">Name of the machine.</param>
		/// <param name="serviceObj">The service obj.</param>
		/// <param name="observer">The observer.</param>
		public static void KillProcess(string machineName, ManagementObject serviceObj, ManagementOperationObserver observer)
		{
			string str = String.Empty;
			foreach (var prop in serviceObj.Properties)
			{
				str += String.Format("{0} - {1}\r\n", prop.Name, prop.Value);
			}
			var processObj = GetProcess(machineName, serviceObj["ProcessId"].ToString()); 
			processObj.InvokeMethod(observer, "Terminate", null);
		}

		/// <summary>
		/// Loads the servers from the config file.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Server> LoadServers()
		{
			XDocument doc = XDocument.Load("Config.xml");
			IEnumerable<Server> results = from server in doc.Descendants("Server")
			select
				new Server
				{
					Name = (string)server.Attribute("Name"),
					Services = new ObservableCollection<Service>(
						from service in server.Descendants("Service")
						select new Service((string)service.Attribute("DisplayName"), (string)service.Attribute("ShortName")))
				};
			var servers = results.ToList();
			RefreshServers(servers);
			return servers;
		}

		/// <summary>
		/// Refreshes the list of server information.
		/// </summary>
		/// <param name="servers">The servers.</param>
		public static void RefreshServers(IEnumerable<Server> servers)
		{
			foreach (Server server in servers)
			{
				string name = server.Name;

				foreach (Service service in server.Services)
				{
					service.ManagementObj = GetService(name, service.Name);
				}
			}
		}

		/// <summary>
		/// Starts the service.
		/// </summary>
		/// <param name="machineName">Name of the machine.</param>
		/// <param name="serviceObj">The service obj.</param>
		/// <param name="observer">The observer.</param>
		public static void StartService(ManagementObject serviceObj, ManagementOperationObserver observer)
		{
			serviceObj.InvokeMethod(observer, "StartService", null);
		}

		/// <summary>
		/// Stops the service.
		/// </summary>
		/// <param name="machineName">Name of the machine.</param>
		/// <param name="serviceObj">The service obj.</param>
		/// <param name="observer">The observer.</param>
		public static void StopService(ManagementObject serviceObj, ManagementOperationObserver observer)
		{
			serviceObj.InvokeMethod(observer, "StopService", null);
		}
	}
}