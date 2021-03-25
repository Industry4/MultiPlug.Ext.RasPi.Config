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

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[4];

            Tasks[0] = ProcessRunner.GetProcessResultAsync(c_CatCommand, "/etc/hostname");
            Tasks[1] = ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 get country");
            Tasks[2] = GetSSIDs();
            Tasks[3] = ProcessRunner.GetProcessResultAsync(c_IFConfigCommand);

            ReadResult ReadResult = DHCPCD.Read();

            var Eth0 = ReadResult.Properties.FirstOrDefault(nic => nic.Id == c_Eth0);

            Eth0IPAddress = (Eth0 == null || Eth0.IPAddress == null) ? string.Empty : Eth0.IPAddress;
            Eth0IP6Address = (Eth0 == null || Eth0.IP6Address == null) ? string.Empty : Eth0.IP6Address;
            Eth0Routers = (Eth0 == null || Eth0.Routers == null) ? new string[0] : Eth0.Routers;
            Eth0DomainNameServers = (Eth0 == null || Eth0.DomainNameServers == null) ? new string[0] : Eth0.DomainNameServers;

            var Wlan0 = ReadResult.Properties.FirstOrDefault(nic => nic.Id == c_Wlan0);

            Wlan0IPAddress = (Wlan0 == null || Wlan0.IPAddress == null) ? string.Empty : Wlan0.IPAddress;
            Wlan0IP6Address = (Wlan0 == null || Wlan0.IP6Address == null) ? string.Empty : Wlan0.IP6Address;
            Wlan0Routers = (Wlan0 == null || Wlan0.Routers == null) ? new string[0] : Wlan0.Routers;
            Wlan0DomainNameServers = (Wlan0 == null || Wlan0.DomainNameServers == null) ? new string[0] : Wlan0.DomainNameServers;

            ReadResult = null;

            Task.WaitAll(Tasks);

            var WiFiCountryResult = Tasks[1].Result.GetOutput().TrimEnd();
            WiFiCountrySet = WiFiCountryResult != c_WiFiCountryNotSet;

            HostName = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().TrimEnd() : string.Empty;
            WiFiCountry = WiFiCountrySet ? WiFiCountryResult : string.Empty;
            SSIDs = ProcessSSIDs(Tasks[2]);
            Interfaces = Tasks[3].Result.Okay() ? IFCONFIG.Parse(Tasks[3].Result.GetOutput()) : new NICInterface[0];

            // Log only if errors have occured
            LoggingActions.LogTaskResult(Log, Tasks[0], EventLogEntryCodes.HostNameSettingsGetError);
            LoggingActions.LogTaskResult(Log, Tasks[1], EventLogEntryCodes.WiFiCountrySettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[2], EventLogEntryCodes.SSIDSettingsGetError);
            LoggingActions.LogTaskResult(Log, Tasks[3], EventLogEntryCodes.NICInterfacesGetError);
            return this;
        }

        internal void UpdateProperties(NetworkProperties theModel)
        {
            List<Task<ProcessResult>> Tasks = new List<Task<ProcessResult>>();
            bool AskToRestart = false;

            bool Eth0Changes = false;
            bool Wlan0Changes = false;

            NetworkProperties CurrentSettings = Core.Instance.Network.RepopulateAndGetProperties();

            if (theModel.Eth0Routers == null) { theModel.Eth0Routers = new string[0]; }
            if (theModel.Eth0DomainNameServers == null) { theModel.Eth0DomainNameServers = new string[0]; }
            if (theModel.Wlan0Routers == null) { theModel.Wlan0Routers = new string[0]; }
            if (theModel.Wlan0DomainNameServers == null) { theModel.Wlan0DomainNameServers = new string[0]; }

            if (CurrentSettings.Eth0IPAddress != theModel.Eth0IPAddress)
            {
                Eth0Changes = true;
                if (string.IsNullOrEmpty(theModel.Eth0IPAddress)) { Log?.Invoke(EventLogEntryCodes.NICIP4DynamicChanging, new string[] { c_Eth0 }); }
                else { Log?.Invoke(EventLogEntryCodes.NICIP4StaticChanging, new string[] { c_Eth0, theModel.Eth0IPAddress }); }
            }

            if (CurrentSettings.Eth0IP6Address != theModel.Eth0IP6Address)
            {
                Eth0Changes = true;
                if (string.IsNullOrEmpty(theModel.Eth0IP6Address)) { Log?.Invoke(EventLogEntryCodes.NICIP6DynamicChanging, new string[] { c_Eth0 }); }
                else { Log?.Invoke(EventLogEntryCodes.NICIP6StaticChanging, new string[] { c_Eth0, theModel.Eth0IP6Address }); }
            }

            if (!CurrentSettings.Eth0Routers.OrderBy(e => e).SequenceEqual(theModel.Eth0Routers.OrderBy(e => e)))
            {
                Eth0Changes = true;
                Log?.Invoke(EventLogEntryCodes.NICRoutersChanging, new string[] { c_Eth0, string.Join(" ", theModel.Eth0Routers) });
            }

            if (!CurrentSettings.Eth0DomainNameServers.OrderBy(e => e).SequenceEqual(theModel.Eth0DomainNameServers.OrderBy(e => e)))
            {
                Eth0Changes = true;
                Log?.Invoke(EventLogEntryCodes.NICDNSChanging, new string[] { c_Eth0, string.Join(" ", theModel.Eth0DomainNameServers) });
            }

            if (CurrentSettings.Wlan0IPAddress != theModel.Wlan0IPAddress)
            {
                Wlan0Changes = true;
                if (string.IsNullOrEmpty(theModel.Wlan0IPAddress)) { Log?.Invoke(EventLogEntryCodes.NICIP6DynamicChanging, new string[] { c_Wlan0 }); }
                else { Log?.Invoke(EventLogEntryCodes.NICIP6StaticChanging, new string[] { c_Wlan0, theModel.Wlan0IPAddress }); }
            }

            if (CurrentSettings.Wlan0IP6Address != theModel.Wlan0IP6Address)
            {
                Wlan0Changes = true;
                if (string.IsNullOrEmpty(theModel.Eth0IP6Address)) { Log?.Invoke(EventLogEntryCodes.NICIP6DynamicChanging, new string[] { c_Wlan0 }); }
                else { Log?.Invoke(EventLogEntryCodes.NICIP6StaticChanging, new string[] { c_Wlan0, theModel.Wlan0IP6Address }); }
            }

            if (!CurrentSettings.Wlan0Routers.OrderBy(e => e).SequenceEqual(theModel.Wlan0Routers.OrderBy(e => e)))
            {
                Wlan0Changes = true;
                Log?.Invoke(EventLogEntryCodes.NICDNSChanging, new string[] { c_Wlan0, string.Join(" ", theModel.Wlan0Routers) });
            }

            if (!CurrentSettings.Wlan0DomainNameServers.OrderBy(e => e).SequenceEqual(theModel.Wlan0DomainNameServers.OrderBy(e => e)))
            {
                Wlan0Changes = true;
                Log?.Invoke(EventLogEntryCodes.NICDNSChanging, new string[] { c_Wlan0, string.Join(" ", theModel.Wlan0DomainNameServers) });
            }

            Task<ProcessResult> IPAddressTask = null;
            if (Eth0Changes || Wlan0Changes)
            {
                NICProperties eth0 = new NICProperties(c_Eth0);
                eth0.IPAddress = theModel.Eth0IPAddress;
                eth0.IP6Address = theModel.Eth0IP6Address;
                eth0.Routers = theModel.Eth0Routers;
                eth0.DomainNameServers = theModel.Eth0DomainNameServers;

                NICProperties wlan0 = new NICProperties(c_Wlan0);
                wlan0.IPAddress = theModel.Wlan0IPAddress;
                wlan0.IP6Address = theModel.Wlan0IP6Address;
                wlan0.Routers = theModel.Wlan0Routers;
                wlan0.DomainNameServers = theModel.Wlan0DomainNameServers;

                IPAddressTask = FlushNetworkSequence(Eth0Changes, Wlan0Changes, eth0, wlan0);
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

        internal bool DeleteWiFi(string id)
        {
            // TODO All Results aren't being catched.

            Task<ProcessResult> ScanTask = ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 list_networks");

            ScanTask.Wait();

            bool Result = ScanTask.Result.Okay();

            if (Result)
            {
                string[] SSIDS = ScanTask.Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => line.Contains(id))
                    .Select(line => line.Substring(0, line.IndexOf("\t")))
                    .ToArray();

                if( SSIDS.Any())
                {
                    Task<Task<ProcessResult>> RemoveWiFi = ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 remove_network " + SSIDS[0])
                        .ContinueWith(async RemoveNetworkTask => await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 save_config"))
                        .ContinueWith(async SaveConfigTask => await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 reconfigure"));

                    RemoveWiFi.Wait();

                    Result = RemoveWiFi.Result.Result.Okay();
                }
                else
                {
                    Result = false;
                }
            }

            return Result;
        }

        internal static Task<ProcessResult> GetSSIDs()
        {
            return ProcessRunner.GetProcessResultAsync("iwgetid", "--raw");
        }


        internal static string[] ProcessSSIDs(Task<ProcessResult> theTask)
        {
            if (theTask.Result.Okay())
            {
                return theTask.Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                                                .Select(line => line.TrimEnd())
                                                                .ToArray();
            }

            return new string[0];
        }

        private Task<ProcessResult> SetWiFiSSIDPassphraseSequence(NetworkProperties theModel)
        {
            return Task.Run(async () =>
            {
                Log?.Invoke(EventLogEntryCodes.SSIDChanging, new string[] { theModel.NewSSID });

                var AddNetworkTask = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 add_network");

                var NewNetworkId = AddNetworkTask.GetOutput().Trim();

                ProcessResult SetSSID = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 set_network " + NewNetworkId + " ssid '\"" + theModel.NewSSID + "\"'");

                ProcessResult SetPassphrase;

                if (string.IsNullOrEmpty(theModel.NewPassphrase))
                {
                    SetPassphrase = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 set_network " + NewNetworkId + " key_mgmt NONE");
                }
                else
                {
                    SetPassphrase = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 set_network " + NewNetworkId + " psk '\"" + theModel.NewPassphrase + "\"'");
                }

                await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 enable_network " + NewNetworkId);
                await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 save_config");
                await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 reconfigure");

                return new ProcessResult(0, string.Empty, string.Empty);
            });
        }

        private Task<ProcessResult> FlushNetworkSequence( bool isEth0Changes, bool isWlan0Changes, NICProperties theEth0, NICProperties theWlan0)
        {
            return Task.Run(async () =>
            {
                await DHCPCD.UpdateResultAsync(new NICProperties[] { theEth0, theWlan0 });
                await ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "daemon-reload");
                await ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "stop dhcpcd.service");
                if (isEth0Changes)
                {
                    await ProcessRunner.GetProcessResultAsync(c_IPCommand, "addr flush dev eth0");
                }
                if (isWlan0Changes)
                {
                    await ProcessRunner.GetProcessResultAsync(c_IPCommand, "addr flush dev wlan0");
                }
                await ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "start dhcpcd.service");
                await ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "restart networking.service");

                if (isWlan0Changes)
                {
                    await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 reconfigure");
                }

                return new ProcessResult(0, string.Empty, string.Empty);
            });
        }
    }
}
