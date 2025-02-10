/// <summary>
/// Uses process calls to raspi-config
/// https://github.com/RPi-Distro/raspi-config
/// MIT License
/// </summary>

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MultiPlug.Ext.RasPi.Config.Models.Components.Network;
using MultiPlug.Ext.RasPi.Config.Network.ConfigFiles;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;

namespace MultiPlug.Ext.RasPi.Config.Components.Network
{
    public class NetworkComponent : NetworkProperties
    {
        internal event Action<EventLogEntryCodes, string[]> Log;
        internal event Action RestartDue;

        internal NetworkProperties RepopulateAndGetProperties()
        {
            if (!Utils.Hardware.isRunningRaspberryPi) { return this; }

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[3];

            Tasks[0] = ProcessRunner.GetProcessResultAsync(c_CatCommand, "/etc/hostname");
            Tasks[1] = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint get_wifi_country");
            Tasks[2] = ProcessRunner.GetProcessResultAsync(c_IFConfigCommand);

            ReadResult ReadResult = DHCPCD.Read();

            var wlans = ReadResult.Properties.Where(nic => nic.Id.StartsWith("wlan", StringComparison.OrdinalIgnoreCase)).ToArray();

            foreach (var item in wlans)
            {
                if (item.IPAddress == null) { item.IPAddress = string.Empty; }
                if (item.IP6Address == null) { item.IP6Address = string.Empty; }
                if (item.Routers == null) { item.Routers = new string[0]; }
                if (item.DomainNameServers == null) { item.DomainNameServers = new string[0]; }
            }

            var eths = ReadResult.Properties.Where(nic => nic.Id.StartsWith("eth", StringComparison.OrdinalIgnoreCase)).ToArray();

            foreach(var item in eths)
            {
                if (item.IPAddress == null) { item.IPAddress = string.Empty; }
                if ( item.IP6Address == null){ item.IP6Address = string.Empty;}
                if (item.Routers == null) { item.Routers = new string[0]; }
                if (item.DomainNameServers == null) { item.DomainNameServers = new string[0]; }
            }

            Wlans = wlans;
            Eths = eths;

            ReadResult = null;

            Task.WaitAll(Tasks);

            var WiFiCountryResult = Tasks[1].Result.GetOutput().TrimEnd();
            WiFiCountrySet = WiFiCountryResult != c_WiFiCountryNotSet || WiFiCountryResult != string.Empty;

            HostName = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().TrimEnd() : string.Empty;
            WiFiCountry = WiFiCountrySet ? WiFiCountryResult : string.Empty;
            Interfaces = Tasks[2].Result.Okay() ? IFCONFIG.Parse(Tasks[2].Result.GetOutput()) : new NICInterface[0];

            // Add any new NICs to the option to set their IPs static
            var NICInterfaces = Interfaces.Where(i => i.Name.StartsWith("eth", StringComparison.OrdinalIgnoreCase)).ToArray();
            Eths = CheckForNewNICs(Eths, NICInterfaces);

            NICInterfaces = Interfaces.Where(i => i.Name.StartsWith("wlan", StringComparison.OrdinalIgnoreCase)).ToArray();
            Wlans = CheckForNewNICs(Wlans, NICInterfaces);

            WPASupplicant.ManageInterfaceCountChanges(NICInterfaces);

            Task<ProcessResult>[] SSIDTasks = new Task<ProcessResult>[NICInterfaces.Length];

            for (int i = 0; i < NICInterfaces.Length; i++)
            {
                SSIDTasks[i] = GetSSIDs(NICInterfaces[i].Name);
            }

            Task.WaitAll(SSIDTasks);

            List<ConnectedSSID> ConnectedSSIDs = new List<ConnectedSSID>();

            for (int i = 0; i < NICInterfaces.Length; i++)
            {
                LoggingActions.LogTaskResult(Log, SSIDTasks[i], EventLogEntryCodes.SSIDSettingsGetError);

                ConnectedSSIDs.AddRange(ProcessSSIDs(SSIDTasks[i], NICInterfaces[i].Name));

            }

            SSIDs = ConnectedSSIDs.ToArray();

            // Log only if errors have occured
            LoggingActions.LogTaskResult(Log, Tasks[0], EventLogEntryCodes.HostNameSettingsGetError);
            LoggingActions.LogTaskResult(Log, Tasks[1], EventLogEntryCodes.WiFiCountrySettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[2], EventLogEntryCodes.NICInterfacesGetError);
            return this;
        }

        private NICProperties[] CheckForNewNICs(NICProperties[] theCurrent, NICInterface[] theNICs)
        {
            var NICsProperties = theCurrent.ToList();
            foreach (var NICInterface in theNICs)
            {
                if (NICsProperties.FirstOrDefault(NIC => NIC.Id.Equals(NICInterface.Name, StringComparison.OrdinalIgnoreCase)) == null)
                {
                    NICProperties NICProperties = new NICProperties(NICInterface.Name);
                    NICProperties.IPAddress = string.Empty;
                    NICProperties.IP6Address = string.Empty;
                    NICProperties.Routers = new string[0];
                    NICProperties.DomainNameServers = new string[0];
                    NICsProperties.Add(NICProperties);
                }
            }
            return NICsProperties.ToArray();
        }


        private bool HasSettingsChanged(NICProperties[] theCurrent, NICProperties[] theNew)
        {
            if (theCurrent.Length != theNew.Length)
            {
                return true;
            }
            else if (!ContainsEqualNICIds(theCurrent.OrderBy(e => e.Id).ToArray(), theNew.OrderBy(e => e.Id).ToArray()))
            {
                return true;
            }
            else
            {
                NICProperties[] CurrentSettingsSorted = theCurrent.OrderBy(e => e.Id).ToArray();
                NICProperties[] NewSettingsSorted = theNew.OrderBy(e => e.Id).ToArray();

                for (int i = 0; i < CurrentSettingsSorted.Length; i++)
                {
                    if (CurrentSettingsSorted[i].IPAddress != NewSettingsSorted[i].IPAddress)
                    {
                        if (string.IsNullOrEmpty(NewSettingsSorted[i].IPAddress)) { Log?.Invoke(EventLogEntryCodes.NICIP4DynamicChanging, new string[] { NewSettingsSorted[i].Id }); }
                        else { Log?.Invoke(EventLogEntryCodes.NICIP4StaticChanging, new string[] { NewSettingsSorted[i].Id, NewSettingsSorted[i].IPAddress }); }
                        return true;
                    }
                    else if (CurrentSettingsSorted[i].IP6Address != NewSettingsSorted[i].IP6Address)
                    {
                        if (string.IsNullOrEmpty(NewSettingsSorted[i].IP6Address)) { Log?.Invoke(EventLogEntryCodes.NICIP6DynamicChanging, new string[] { NewSettingsSorted[i].Id }); }
                        else { Log?.Invoke(EventLogEntryCodes.NICIP6StaticChanging, new string[] { NewSettingsSorted[i].Id, NewSettingsSorted[i].IPAddress }); }
                        return true;
                    }
                    if (!CurrentSettingsSorted[i].Routers.OrderBy(e => e).SequenceEqual(NewSettingsSorted[i].Routers.OrderBy(e => e)))
                    {
                        Log?.Invoke(EventLogEntryCodes.NICRoutersChanging, new string[] { NewSettingsSorted[i].Id, string.Join(" ", NewSettingsSorted[i].Routers) });
                        return true;
                    }

                    if (!CurrentSettingsSorted[i].DomainNameServers.OrderBy(e => e).SequenceEqual(NewSettingsSorted[i].DomainNameServers.OrderBy(e => e)))
                    {
                        Log?.Invoke(EventLogEntryCodes.NICDNSChanging, new string[] { NewSettingsSorted[i].Id, string.Join(" ", NewSettingsSorted[i].DomainNameServers) });
                        return true;
                    }
                }
            }

            return false;
        }


        internal void UpdateProperties(NetworkProperties theModel)
        {
            List<Task<ProcessResult>> Tasks = new List<Task<ProcessResult>>();
            bool AskToRestart = false;

            bool EthsChanges = false;
            bool WlansChanges = false;

            NetworkProperties CurrentSettings = Core.Instance.Network.RepopulateAndGetProperties();

            if (theModel.Eths == null) { theModel.Eths = new NICProperties[0]; }
            if (theModel.Wlans == null) { theModel.Wlans = new NICProperties[0]; }

            // TODO Lots of Duplicate Code
            EthsChanges = HasSettingsChanged(CurrentSettings.Eths, theModel.Eths);
            WlansChanges = HasSettingsChanged(CurrentSettings.Wlans, theModel.Wlans);

            Task<ProcessResult> IPAddressTask = null;
            if (EthsChanges || WlansChanges)
            {
                IPAddressTask = FlushNetworkSequence(EthsChanges, WlansChanges, theModel.Eths, theModel.Wlans);
                Tasks.Add(IPAddressTask);
            }

            Task<ProcessResult> HostNameTask = null;
            if (CurrentSettings.HostName != theModel.HostName)
            {
                AskToRestart = true;
                HostNameTask = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_hostname " + theModel.HostName);
                Tasks.Add(HostNameTask);
                Log?.Invoke(EventLogEntryCodes.HostNameChanging, new string[] { theModel.HostName });
            }

            Task<ProcessResult> SetWiFiSSIDPassphrase = null;

            if (!string.IsNullOrEmpty(theModel.NewSSID))
            {
                SetWiFiSSIDPassphrase = SetWiFiSSIDPassphraseSequence(theModel);
                Tasks.Add(SetWiFiSSIDPassphrase);
            }

            if (AskToRestart) { RestartDue?.Invoke(); }

            Task.WaitAll(Tasks.ToArray());

            // Check if Tasks have completed Okay and Log result

            LoggingActions.LogTaskResult(Log, IPAddressTask, EventLogEntryCodes.NICIPChangesComplete, EventLogEntryCodes.NICIPChangesError);
            LoggingActions.LogTaskResult(Log, HostNameTask, EventLogEntryCodes.HostNameChanged, EventLogEntryCodes.HostNameChangeError);
            LoggingActions.LogTaskResult(Log, SetWiFiSSIDPassphrase, EventLogEntryCodes.SSIDChanged, EventLogEntryCodes.SSIDChangeError);
        }

        private bool ContainsEqualNICIds(NICProperties[] theNICProperties1, NICProperties[] theNICProperties2)
        {
            for(int i = 0; i < theNICProperties1.Length; i++)
            {
                if(theNICProperties1[i].Id != theNICProperties2[i].Id)
                {
                    return false;
                }
            }

            return true;
        }

        internal bool DeleteWiFi(string id, string theWlanId, string ssid)
        {
            // TODO All Results aren't being catched.

            bool Result;

            if (Utils.Hardware.isUsingNetworkManager == false)
            {
                var Task = ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theWlanId + " remove_network " + id);
                Task.Wait();
                if (!Task.Result.Okay()) { return false; }

                Task = ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theWlanId + " save_config");
                Task.Wait();
                if (!Task.Result.Okay()) { return false; }

                Task = ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theWlanId + " reconfigure");
                Task.Wait();
                Result = Task.Result.Okay();

            }
            else
            {
                Task<ProcessResult> ScanTask = ProcessRunner.GetProcessResultAsync("nmcli", "con delete " + ssid);

                ScanTask.Wait();

                Result = ScanTask.Result.Okay();
            }

            return Result;
        }

        internal static Task<ProcessResult> GetSSIDs(string theWlanId)
        {
           return ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theWlanId + " list_networks");
        }


        internal static ConnectedSSID[] ProcessSSIDs(Task<ProcessResult> theTask, string theWlan)
        {
            if (theTask.Result.Okay())
            {
                return theTask.Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1) // Skip the Heading
                    .Select( Row =>
                    {
                        var RowItems = Row.Split(new char[] { '\t' });
                        return new ConnectedSSID { Wlan = theWlan, SSID = RowItems[1], Id = RowItems[0] };
                    })
                    .ToArray();
            }

            return new ConnectedSSID[0];
        }

        private Task<ProcessResult> SetWiFiSSIDPassphraseSequence(NetworkProperties theModel)
        {
            return Task.Run(async () =>
            {
                Log?.Invoke(EventLogEntryCodes.SSIDChanging, new string[] { theModel.NewSSID });

                if (Utils.Hardware.isUsingNetworkManager == false)
                {
                    var AddNetworkTask = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theModel.NewWiFiNIC + " add_network");

                    var NewNetworkId = AddNetworkTask.GetOutput().Trim();

                    ProcessResult SetSSID = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theModel.NewWiFiNIC + " set_network " + NewNetworkId + " ssid '\"" + theModel.NewSSID + "\"'");

                    ProcessResult SetPassphrase;

                    if (string.IsNullOrEmpty(theModel.NewPassphrase))
                    {
                        SetPassphrase = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theModel.NewWiFiNIC + " set_network " + NewNetworkId + " key_mgmt NONE");
                    }
                    else
                    {
                        SetPassphrase = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theModel.NewWiFiNIC + " set_network " + NewNetworkId + " psk '\"" + theModel.NewPassphrase + "\"'");
                    }

                    await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theModel.NewWiFiNIC + " enable_network " + NewNetworkId);
                    await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theModel.NewWiFiNIC + " save_config");
                    await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + theModel.NewWiFiNIC + " reconfigure");

                    return new ProcessResult(0, string.Empty, string.Empty);
                }
                else
                {
                    return await ProcessRunner.GetProcessResultAsync(c_NetworkMonitorCliCommand , "device wifi connect " + theModel.NewSSID + " password " + theModel.NewPassphrase + " ifname " + theModel.NewWiFiNIC);
                }
            });
        }

        private Task<ProcessResult> FlushNetworkSequence( bool isAnyEthChanges, bool isAnyWlanChanges, NICProperties[] theEth, NICProperties[] theWlan)
        {
            return Task.Run(async () =>
            {
                await DHCPCD.UpdateResultAsync(theEth.Concat( theWlan ).ToArray());
                await ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "daemon-reload");
                await ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "stop dhcpcd.service");
                if (isAnyEthChanges)
                {
                    foreach(var Eth in theEth)
                    {
                        InterfacesD.Write(Eth);
                    }

                    foreach (var Eth in theEth)
                    {
                        await ProcessRunner.GetProcessResultAsync(c_IPCommand, "addr flush dev " + Eth.Id);
                    }
                }
                if (isAnyWlanChanges)
                {
                    foreach (var Wlan in theWlan)
                    {
                        InterfacesD.Write(Wlan);
                    }

                    foreach (var Wlan in theWlan)
                    {
                        await ProcessRunner.GetProcessResultAsync(c_IPCommand, "addr flush dev " + Wlan.Id);
                    }
                }
                await ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "start dhcpcd.service");
                await ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "restart networking.service");

                if (isAnyWlanChanges)
                {
                    foreach (var Wlan in theWlan)
                    {
                        await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i " + Wlan.Id + " reconfigure");
                    }
                }

                foreach (var Eth in theEth)
                {
                    await ProcessRunner.GetProcessResultAsync(c_IFUp, Eth.Id);
                }

                foreach (var Wlan in theWlan)
                {
                    await ProcessRunner.GetProcessResultAsync(c_IFUp, Wlan.Id);
                }

                return new ProcessResult(0, string.Empty, string.Empty);
            });
        }
    }
}
