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

        private const string c_Eth0 = "eth0";
        private const string c_Wlan0 = "wlan0";
        private const string c_LinuxRaspconfigCommand = "raspi-config";

        internal NetworkProperties RepopulateAndGetProperties()
        {
            if (!RunningRaspberryPi) { return this; }

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[4];

            Tasks[0] = ProcessRunner.GetProcessResultAsync("cat", "/etc/hostname");
            Tasks[1] = ProcessRunner.GetProcessResultAsync("wpa_cli", "-i wlan0 get country");
            Tasks[2] = ProcessRunner.GetProcessResultAsync("iwgetid", "--raw");
            Tasks[3] = ProcessRunner.GetProcessResultAsync("ifconfig");

            ReadResult ReadResult = DHCPCD.Read();

            var Eth0 = ReadResult.Properties.FirstOrDefault(nic => nic.Id == "eth0");

            Eth0IPAddress           = (Eth0 == null) ? string.Empty : Eth0.IPAddress;
            Eth0IP6Address          = (Eth0 == null) ? string.Empty : Eth0.IP6Address;
            Eth0Routers             = (Eth0 == null) ? new string[0] : (Eth0.Routers == null) ? new string[0] : Eth0.Routers;
            Eth0DomainNameServers   = (Eth0 == null) ? new string[0] : (Eth0.DomainNameServers == null) ? new string[0] : Eth0.DomainNameServers;

            var Wlan0 = ReadResult.Properties.FirstOrDefault(nic => nic.Id == "wlan0");

            Wlan0IPAddress          = (Wlan0 == null) ? string.Empty : Wlan0.IPAddress;
            Wlan0IP6Address         = (Wlan0 == null) ? string.Empty : Wlan0.IP6Address;
            Wlan0Routers            = (Wlan0 == null) ? new string[0] : (Wlan0.Routers == null) ? new string[0] : Wlan0.Routers;
            Wlan0DomainNameServers  = (Wlan0 == null) ? new string[0] : (Wlan0.DomainNameServers == null) ? new string[0] : Wlan0.DomainNameServers;

            Task.WaitAll(Tasks);

            HostName    = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().TrimEnd() : string.Empty;
            WiFiCountry = Tasks[1].Result.Okay() ? Tasks[1].Result.GetOutput().TrimEnd() : string.Empty;
            SSID        = Tasks[2].Result.Okay() ? Tasks[2].Result.GetOutput().TrimEnd() : string.Empty;
            Passphrase  = "";
            Interfaces  = Tasks[3].Result.Okay() ? IFCONFIG.Parse(Tasks[3].Result.GetOutput()) : new NICInterface[0];

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

            NetworkProperties CurrentSettings = Core.Instance.Network.RepopulateAndGetProperties();

            if (theModel.Eth0Routers == null){ theModel.Eth0Routers = new string[0]; }
            if (theModel.Eth0DomainNameServers == null) { theModel.Eth0DomainNameServers = new string[0]; }
            if (theModel.Wlan0Routers == null) { theModel.Wlan0Routers = new string[0]; }
            if (theModel.Wlan0DomainNameServers == null) { theModel.Wlan0DomainNameServers = new string[0]; }

            if (CurrentSettings.Eth0IPAddress != theModel.Eth0IPAddress)
            {
                AskToRestart = true;
                if( string.IsNullOrEmpty( theModel.Eth0IPAddress ) ) { Log?.Invoke( EventLogEntryCodes.NICIP4DynamicChanging, new string[] { c_Eth0 }); }
                else { Log?.Invoke( EventLogEntryCodes.NICIP4StaticChanging, new string[] { c_Eth0, theModel.Eth0IPAddress }); }
            }

            if ( CurrentSettings.Eth0IP6Address != theModel.Eth0IP6Address )
            {
                AskToRestart = true;
                if (string.IsNullOrEmpty(theModel.Eth0IP6Address)) { Log?.Invoke( EventLogEntryCodes.NICIP6DynamicChanging, new string[] { c_Eth0 }); }
                else { Log?.Invoke( EventLogEntryCodes.NICIP6StaticChanging, new string[] { c_Eth0, theModel.Eth0IP6Address }); }
            }

            if ( ! CurrentSettings.Eth0Routers.OrderBy(e => e).SequenceEqual(theModel.Eth0Routers.OrderBy(e => e)))
            {
                AskToRestart = true;
                Log?.Invoke( EventLogEntryCodes.NICRoutersChanging, new string[] { c_Eth0, string.Join(" ", theModel.Eth0Routers) });
            }

            if ( ! CurrentSettings.Eth0DomainNameServers.OrderBy(e => e).SequenceEqual(theModel.Eth0DomainNameServers.OrderBy(e => e)))
            {
                AskToRestart = true;
                Log?.Invoke( EventLogEntryCodes.NICDNSChanging, new string[] { c_Eth0, string.Join(" ", theModel.Eth0DomainNameServers) });
            }

            if ( CurrentSettings.Wlan0IPAddress != theModel.Wlan0IPAddress )
            {
                AskToRestart = true;
                if (string.IsNullOrEmpty(theModel.Wlan0IPAddress)) { Log?.Invoke( EventLogEntryCodes.NICIP6DynamicChanging, new string[] { c_Wlan0 }); }
                else { Log?.Invoke( EventLogEntryCodes.NICIP6StaticChanging, new string[] { c_Wlan0, theModel.Wlan0IPAddress }); }
            }

            if( CurrentSettings.Wlan0IP6Address != theModel.Wlan0IP6Address )
            {
                AskToRestart = true;
                if (string.IsNullOrEmpty(theModel.Eth0IP6Address)) { Log?.Invoke( EventLogEntryCodes.NICIP6DynamicChanging, new string[] { c_Wlan0 }); }
                else { Log?.Invoke( EventLogEntryCodes.NICIP6StaticChanging, new string[] { c_Wlan0, theModel.Wlan0IP6Address }); }
            }

            if ( ! CurrentSettings.Wlan0Routers.OrderBy(e => e).SequenceEqual(theModel.Wlan0Routers.OrderBy(e => e)))
            {
                AskToRestart = true;
                Log?.Invoke( EventLogEntryCodes.NICDNSChanging, new string[] { c_Wlan0, string.Join(" ", theModel.Wlan0Routers) });
            }

            if ( ! CurrentSettings.Wlan0DomainNameServers.OrderBy(e => e).SequenceEqual(theModel.Wlan0DomainNameServers.OrderBy(e => e)))
            {
                AskToRestart = true;
                Log?.Invoke( EventLogEntryCodes.NICDNSChanging, new string[] { c_Wlan0, string.Join(" ", theModel.Wlan0DomainNameServers) });
            }

            Task<ProcessResult> IPAddressTask = null;
            if ( AskToRestart)
            {
                var eth0 = new NICProperties(c_Eth0);
                eth0.IPAddress = (theModel.Eth0IPAddress != string.Empty) ? theModel.Eth0IPAddress : null;
                eth0.IP6Address = (theModel.Eth0IP6Address != string.Empty) ? theModel.Eth0IP6Address : null;
                eth0.Routers = theModel.Eth0Routers;
                eth0.DomainNameServers = theModel.Eth0DomainNameServers;

                var wlan0 = new NICProperties(c_Wlan0);
                wlan0.IPAddress = (theModel.Wlan0IPAddress != string.Empty) ? theModel.Wlan0IPAddress : null;
                wlan0.IP6Address = (theModel.Wlan0IP6Address != string.Empty) ? theModel.Wlan0IP6Address : null;
                wlan0.Routers = theModel.Wlan0Routers;
                wlan0.DomainNameServers = theModel.Wlan0DomainNameServers;

                IPAddressTask = DHCPCD.UpdateResultAsync(new NICProperties[] { eth0, wlan0 });
                Tasks.Add(IPAddressTask);
            }

            Task<ProcessResult> HostNameTask = null;
            if (CurrentSettings.HostName != theModel.HostName)
            {
                AskToRestart = true;
                HostNameTask = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_hostname " + theModel.HostName );
                Tasks.Add(HostNameTask);
                Log?.Invoke( EventLogEntryCodes.HostNameChanging, new string[] { theModel.HostName });
            }

            Task<ProcessResult> SetWiFiSSIDPassphrase = null;
            if ((CurrentSettings.SSID != theModel.SSID) || (CurrentSettings.Passphrase != theModel.Passphrase))
            {
                SetWiFiSSIDPassphrase = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_wifi_ssid_passphrase " + theModel.SSID + " " + theModel.Passphrase);
                Tasks.Add(SetWiFiSSIDPassphrase);
                Log?.Invoke( EventLogEntryCodes.SSIDChanging, new string[] { theModel.SSID });
            }

            if (AskToRestart) { RestartDue?.Invoke(); }

            Task.WaitAll(Tasks.ToArray());

            // Check if Tasks have completed Okay and Log result

            LoggingActions.LogTaskResult(Log, IPAddressTask, EventLogEntryCodes.NICIPChangesComplete, EventLogEntryCodes.NICIPChangesError);
            LoggingActions.LogTaskResult(Log, HostNameTask, EventLogEntryCodes.HostNameChanged, EventLogEntryCodes.HostNameChangeError);
            LoggingActions.LogTaskResult(Log, SetWiFiSSIDPassphrase, EventLogEntryCodes.SSIDChanged, EventLogEntryCodes.SSIDChangeError);
        }
    }
}
