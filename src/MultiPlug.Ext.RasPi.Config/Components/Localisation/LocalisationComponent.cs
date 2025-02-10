/// <summary>
/// Uses process calls to raspi-config
/// https://github.com/RPi-Distro/raspi-config
/// MIT License
/// </summary>

using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using MultiPlug.Ext.RasPi.Config.Models.Components.Localisation;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;
using MultiPlug.Base.Exchange;
using MultiPlug.Ext.RasPi.Config.Models.Load;
using MultiPlug.Ext.RasPi.Config.Components.Network;
using MultiPlug.Ext.RasPi.Config.Models.Components.Network;

namespace MultiPlug.Ext.RasPi.Config.Components.Localisation
{
    public class LocalisationComponent : LocalisationProperties
    {
        internal event Action<EventLogEntryCodes, string[]> Log;
        internal event Action RestartDue;

        private SyncWifiCountryService m_SyncService;

        internal LocalisationComponent(IMultiPlugAPI theMultiPlugAPI)
        {
            m_SyncService = new SyncWifiCountryService(theMultiPlugAPI);
            m_SyncService.Synced += OnWifiSynced;
            m_SyncService.Log += OnSyncServiceLog;

            WiFiCountrySyncEnabled = true;  // Temp On by default
        }

        private void OnSyncServiceLog(EventLogEntryCodes theCode, string[] theData)
        {
            Log?.Invoke(theCode, theData);
        }

        private void OnWifiSynced()
        {
            RestartDue?.Invoke();
        }

        internal LocalisationProperties RepopulateAndGetProperties()
        {
            if (!Utils.Hardware.isRunningRaspberryPi) { return this; }

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[11];

            Tasks[0] = ProcessRunner.GetProcessResultAsync(c_CatCommand, "/usr/share/zoneinfo/iso3166.tab");
            Tasks[1] = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint get_wifi_country");
            Tasks[2] = ProcessRunner.GetProcessResultAsync(c_TimeDateControlCommand, "list-timezones");
            Tasks[3] = ProcessRunner.GetProcessResultAsync(c_CatCommand, "/etc/timezone");
            Tasks[4] = ProcessRunner.GetProcessResultAsync(c_DateCommand, "+%d/%m/%Y");
            Tasks[5] = ProcessRunner.GetProcessResultAsync(c_DateCommand, "+%T");
            Tasks[6] = ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "is-active systemd-timesyncd");
            Tasks[7] = ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "is-active fake-hwclock");
            Tasks[8] = ProcessRunner.GetProcessResultAsync(c_HardwareClockCommand, "-r");
            Tasks[9] = ProcessRunner.GetProcessResultAsync(c_IFConfigCommand);
            Tasks[10] = ProcessRunner.GetProcessResultAsync(c_GrepCommand, "-P '(?=^((?!#).)*$)NTP=' /etc/systemd/timesyncd.conf"); //Ignore Commented out

            Task.WaitAll(Tasks);

            WifiCountries       = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Where( line => ( ! line.StartsWith("#") ) ).Select(line => new WifiCountryModel { AlphaTwo = line.Split('\t')[0], Country = line.Split('\t')[1] }).OrderBy(Country => Country.Country).ToArray() : new WifiCountryModel[0]; 
            WifiCountry         = Tasks[1].Result.Okay() ? Tasks[1].Result.GetOutput().Trim() : string.Empty;
            TimeZones           = Tasks[2].Result.Okay() ? Tasks[2].Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
            TimeZone            = Tasks[3].Result.Okay() ? Tasks[3].Result.GetOutput().TrimEnd() : string.Empty;
            Date                = Tasks[4].Result.Okay() ? Tasks[4].Result.GetOutput().TrimEnd() : string.Empty;
            Time                = Tasks[5].Result.Okay() ? Tasks[5].Result.GetOutput().TrimEnd() : string.Empty;
            TimeSyncdEnabled    = Tasks[6].Result.Okay(); // Will Error if inactive
            FakeHWClockEnabled  = Tasks[7].Result.Okay(); // Will Error if inactive
            HWClockPresent      = Tasks[8].Result.Okay(); // Will Error if not Present

            CanChangeWifiCountry = true;

            var NICInterfaces = Tasks[9].Result.Okay() ? IFCONFIG.Parse(Tasks[9].Result.GetOutput()) : new NICInterface[0];
            NICInterfaces = NICInterfaces.Where(i => i.Name.StartsWith("wlan", StringComparison.OrdinalIgnoreCase)).ToArray();

            Task<ProcessResult>[] SSIDTasks = new Task<ProcessResult>[NICInterfaces.Length];

            for (int i = 0; i < NICInterfaces.Length; i++)
            {
                SSIDTasks[i] = NetworkComponent.GetSSIDs(NICInterfaces[i].Name);
            }

            Task.WaitAll(SSIDTasks);

            List<ConnectedSSID> ConnectedSSIDs = new List<ConnectedSSID>();

            for (int i = 0; i < NICInterfaces.Length; i++)
            {
                LoggingActions.LogTaskResult(Log, SSIDTasks[i], EventLogEntryCodes.SSIDSettingsGetError);

                var SSIDs = NetworkComponent.ProcessSSIDs(SSIDTasks[i], NICInterfaces[i].Name);

                if(SSIDs.Any())
                {
                    // Connection are already made on the WiFi so the Country can not be changed.
                    CanChangeWifiCountry = false;
                    break;
                }
            }

            NTPServer1 = string.Empty;
            NTPServer2 = string.Empty;
            NTPServer3 = string.Empty;

            // #NTP=0.debian.pool.ntp.org 1.debian.pool.ntp.org 2.debian.pool.ntp.org 3.debian.pool.ntp.org
            if (Tasks[10].Result.Okay())
            {
                var SplitByEquals = Tasks[10].Result.GetOutput().Split(new char[] { '=' });

                if(SplitByEquals.Length > 1)
                {
                    // 0.debian.pool.ntp.org 1.debian.pool.ntp.org 2.debian.pool.ntp.org 3.debian.pool.ntp.org
                    var SplitBySpace = SplitByEquals[1].Split(new char[] { ' ' });

                    for (int i = 0; i < SplitBySpace.Length; i++)
                    {
                        // 0.debian.pool.ntp.org
                        // 1.debian.pool.ntp.org
                        // 2.debian.pool.ntp.org
                        // 3.debian.pool.ntp.org

                        switch (i)
                        {
                            case 0:
                                NTPServer1 = SplitBySpace[i].Trim();
                                break;
                            case 1:
                                NTPServer2 = SplitBySpace[i].Trim();
                                break;
                            case 2:
                                NTPServer3 = SplitBySpace[i].Trim();
                                break;
                        }
                    }
                }
            }

            // Log only if errors have occured
            LoggingActions.LogTaskResult(Log, Tasks[0], EventLogEntryCodes.WiFiCountriesSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[1], EventLogEntryCodes.WiFiCountrySettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[2], EventLogEntryCodes.TimeZonesSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[3], EventLogEntryCodes.TimeZoneSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[4], EventLogEntryCodes.DateSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[5], EventLogEntryCodes.TimeSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[8], EventLogEntryCodes.NICInterfacesGetError);

            return this;
        }

        internal void StartServices()
        {
            if(WiFiCountrySyncEnabled)
            {
                m_SyncService.Begin();
            }
        }

        internal void UpdateProperties(LocalisationLoad theModel)
        {
            WiFiCountrySyncEnabled = theModel.WiFiCountrySyncEnabled;

            if(WiFiCountrySyncEnabled == false)
            {
                m_SyncService.Reset();
            }
        }

        internal void UpdateProperties(LocalisationProperties theModel)
        {
            List<Task<ProcessResult>> Tasks = new List<Task<ProcessResult>>();

            RepopulateAndGetProperties();

            bool AskToRestart = false;

            Task<ProcessResult> StartTimesyncd = null;
            Task<ProcessResult> DisableTimesyncd = null;

            if (TimeSyncdEnabled != theModel.TimeSyncdEnabled)
            {
                LoggingActions.LogTaskAction(Log, theModel.TimeSyncdEnabled, EventLogEntryCodes.TimesyncdEnabling, EventLogEntryCodes.TimesyncdStopping);

                if (theModel.TimeSyncdEnabled)
                {
                    Task<ProcessResult> EnableTimesyncd = ProcessRunner.GetProcessResultAsync(c_LinuxSystemControlCommand, "enable systemd-timesyncd");
                    EnableTimesyncd.Wait();
                    LoggingActions.LogTaskResult(Log, EnableTimesyncd, EventLogEntryCodes.TimesyncdEnabled, EventLogEntryCodes.TimesyncdEnablingError);

                    if(EnableTimesyncd.Result.Okay())
                    {
                        Log?.Invoke( EventLogEntryCodes.TimesyncdStarting, null);
                        StartTimesyncd = ProcessRunner.GetProcessResultAsync(c_LinuxSystemControlCommand, "start systemd-timesyncd");
                        Tasks.Add(StartTimesyncd);
                    }
                }
                else
                {
                    Task<ProcessResult> StopTimesyncd = ProcessRunner.GetProcessResultAsync(c_LinuxSystemControlCommand, "stop systemd-timesyncd");
                    StopTimesyncd.Wait();
                    LoggingActions.LogTaskResult(Log, StopTimesyncd, EventLogEntryCodes.TimesyncdStopped, EventLogEntryCodes.TimesyncdStoppingError);

                    if (StopTimesyncd.Result.Okay())
                    {
                        Log?.Invoke( EventLogEntryCodes.TimesyncdDisabling, null);
                        DisableTimesyncd = ProcessRunner.GetProcessResultAsync(c_LinuxSystemControlCommand, "disable systemd-timesyncd");
                        Tasks.Add(DisableTimesyncd);
                    }
                }
            }

            Task<ProcessResult> StartFakeHWClock = null;
            Task<ProcessResult> DisableFakeHWClock = null;

            // Raspberry Pi 5 always has a RTC
            if (Utils.Hardware.isRunningRaspberryPi5 == false && FakeHWClockEnabled != theModel.FakeHWClockEnabled)
            {
                LoggingActions.LogTaskAction(Log, theModel.FakeHWClockEnabled, EventLogEntryCodes.FakeHwClockEnabling, EventLogEntryCodes.FakeHwClockStopping);

                if (theModel.FakeHWClockEnabled)
                {
                    Task<ProcessResult> EnableFakeHWClock = ProcessRunner.GetProcessResultAsync(c_LinuxSystemControlCommand, "enable fake-hwclock");
                    EnableFakeHWClock.Wait();
                    LoggingActions.LogTaskResult(Log, EnableFakeHWClock, EventLogEntryCodes.FakeHwClockEnabled, EventLogEntryCodes.FakeHwClockEnablingError);

                    if (EnableFakeHWClock.Result.Okay())
                    {
                        Log?.Invoke( EventLogEntryCodes.TimesyncdEnabling, null);
                        StartFakeHWClock = ProcessRunner.GetProcessResultAsync(c_LinuxSystemControlCommand, "start fake-hwclock");
                        Tasks.Add(StartFakeHWClock);
                    }
                }
                else
                {
                    Task<ProcessResult> StopFakeHWClock = ProcessRunner.GetProcessResultAsync(c_LinuxSystemControlCommand, "stop fake-hwclock");
                    StopFakeHWClock.Wait();
                    LoggingActions.LogTaskResult(Log, StopFakeHWClock, EventLogEntryCodes.FakeHwClockStopped, EventLogEntryCodes.FakeHwClockStoppingError);

                    if (StopFakeHWClock.Result.Okay())
                    {
                        Log?.Invoke( EventLogEntryCodes.FakeHwClockDisabling, null);
                        DisableFakeHWClock = ProcessRunner.GetProcessResultAsync(c_LinuxSystemControlCommand, "disable fake-hwclock");
                        Tasks.Add(DisableFakeHWClock);
                    }
                }
            }

            Task<ProcessResult> SetTimeZone = null;

            if (TimeZone != theModel.TimeZone)
            {
                Log?.Invoke( EventLogEntryCodes.TimeZoneSetting, new string[] { theModel.TimeZone });
                SetTimeZone = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_change_timezone " + theModel.TimeZone);
                Tasks.Add(SetTimeZone);
            }

            Task<ProcessResult> RestartTimesyncd = null;

            if (NTPServer1 != theModel.NTPServer1 || NTPServer2 != theModel.NTPServer2 || NTPServer3 != theModel.NTPServer3)
            {
                if(theModel.NTPServer1.Trim() == string.Empty && theModel.NTPServer2.Trim() == string.Empty && theModel.NTPServer3.Trim() == string.Empty)
                {
                    var CommentOut = ProcessRunner.GetProcessResultAsync(c_SedCommand, "/etc/systemd/timesyncd.conf -i -e \"s/^NTP=/#NTP=/\"");
                    CommentOut.Wait();
                }
                else
                {
                    List<string> list = new List<string>(3);

                    if(theModel.NTPServer1.Trim() != string.Empty)
                    {
                        list.Add(theModel.NTPServer1.Trim());
                    }
                    if (theModel.NTPServer2.Trim() != string.Empty)
                    {
                        list.Add(theModel.NTPServer2.Trim());
                    }
                    if (theModel.NTPServer3.Trim() != string.Empty)
                    {
                        list.Add(theModel.NTPServer3.Trim());
                    }

                    string TimeServers = "NTP=";

                    string[] NTPServers = list.ToArray();

                    for (int i = 0; i < NTPServers.Length; i++)
                    {
                        if(i != 0)
                        {
                            TimeServers += " ";
                        }
                        TimeServers += NTPServers[i];
                    }

                    var UnCommentOut = ProcessRunner.GetProcessResultAsync(c_SedCommand, "/etc/systemd/timesyncd.conf -i -e \"s/^#NTP=/NTP=/\"");
                    UnCommentOut.Wait();

                    var UpdateTimeServers = ProcessRunner.GetProcessResultAsync(c_SedCommand, "/etc/systemd/timesyncd.conf -i -e \"s/^NTP=.*/" + TimeServers + "/g\"");
                    UpdateTimeServers.Wait();
                }

                if(TimeSyncdEnabled)
                {
                    RestartTimesyncd = ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "restart systemd-timesyncd");
                    Tasks.Add(RestartTimesyncd);
                }
            }

            Task<ProcessResult> SetWifiCountry = null;

            if (CanChangeWifiCountry && WifiCountry != theModel.WifiCountry)
            {
                if (theModel.WifiCountry == string.Empty )
                {
                    if ( ! WifiCountry.StartsWith("FAIL"))
                    {
                        AskToRestart = true;
                        Log?.Invoke(EventLogEntryCodes.WifiCountrySetting, new string[] { "None" });
                        SetWifiCountry = UnsetWiFiSequence();
                        Tasks.Add(SetWifiCountry);
                    }
                }
                else
                {
                    if(Utils.Hardware.isUsingNetworkManager)
                    {
                        var NetworkManagerTask = ProcessRunner.GetProcessResultAsync(c_NetworkMonitorCliCommand, "radio wifi off");
                        NetworkManagerTask.Wait();
                        NetworkManagerTask = ProcessRunner.GetProcessResultAsync(c_IWCommand, "reg set 00");
                        NetworkManagerTask.Wait();
                    }

                    AskToRestart = true;
                    Log?.Invoke( EventLogEntryCodes.WifiCountrySetting, new string[] { theModel.WifiCountry });
                    SetWifiCountry = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_wifi_country " + theModel.WifiCountry);
                    Tasks.Add(SetWifiCountry);
                }
            }

            Task<ProcessResult> SetDate = null;

            if (theModel.SetDate)
            {
                DateTime DateConvert = DateTime.ParseExact(theModel.Date.Replace("/", ""), "ddMMyyyy", CultureInfo.InvariantCulture);
                string DateCommand = "+%Y%m%d -s \"" + DateConvert.ToString("yyyyMMdd") + "\"";
                Log?.Invoke( EventLogEntryCodes.DateSetting, new string[]{ "date " + DateCommand});
                SetDate = ProcessRunner.GetProcessResultAsync(c_DateCommand, DateCommand);
                Tasks.Add(SetDate);
            }

            Task<ProcessResult> SetTime = null;

            if (theModel.SetTime)
            {
                string TimeCommand = "+%T -s \"" + theModel.Time + "\"";
                Log?.Invoke( EventLogEntryCodes.TimeSetting, new string[] { "date " + TimeCommand });
                SetTime = ProcessRunner.GetProcessResultAsync(c_DateCommand, TimeCommand);
                Tasks.Add(SetTime);
            }

            if( theModel.WiFiCountrySyncEnabled && WiFiCountrySyncEnabled == false)
            {
                m_SyncService.Begin();
                WiFiCountrySyncEnabled = true;
            }
            else if( theModel.WiFiCountrySyncEnabled == false)
            {
                WiFiCountrySyncEnabled = false;
                m_SyncService.Reset();
            }

            if (AskToRestart) { RestartDue?.Invoke(); }

            Task.WaitAll(Tasks.ToArray());

            Task<ProcessResult> SyncRTC = null;

            if (HWClockPresent && ( theModel.SetTime || theModel.SetDate || (theModel.TimeSyncdEnabled && ! TimeSyncdEnabled ) ) && theModel.HWClockPresent )
            {
                Log?.Invoke(EventLogEntryCodes.RTCSyncing, null);
                SyncRTC = ProcessRunner.GetProcessResultAsync(c_HardwareClockCommand, "-w");
                SyncRTC.Wait();
            } 

            // Check if Tasks have completed Okay and Log result
            LoggingActions.LogTaskResult(Log, StartTimesyncd, EventLogEntryCodes.TimesyncdStarted, EventLogEntryCodes.TimesyncdStartingError);
            LoggingActions.LogTaskResult(Log, DisableTimesyncd, EventLogEntryCodes.TimesyncdDisabled, EventLogEntryCodes.TimesyncdDisablingError);
            LoggingActions.LogTaskResult(Log, RestartTimesyncd, EventLogEntryCodes.TimesyncdRestarted, EventLogEntryCodes.TimesyncdRestartError);
            LoggingActions.LogTaskResult(Log, StartFakeHWClock, EventLogEntryCodes.FakeHwClockStarted, EventLogEntryCodes.FakeHwClockStartingError);
            LoggingActions.LogTaskResult(Log, DisableFakeHWClock, EventLogEntryCodes.FakeHwClockDisabled, EventLogEntryCodes.FakeHwClockDisablingError);
            LoggingActions.LogTaskResult(Log, SetTimeZone, EventLogEntryCodes.TimeZoneSet, EventLogEntryCodes.TimeZoneSettingError);
            LoggingActions.LogTaskResult(Log, SetWifiCountry, EventLogEntryCodes.WifiCountrySet, EventLogEntryCodes.WifiCountrySettingError);
            LoggingActions.LogTaskResult(Log, SetDate, EventLogEntryCodes.DateSet, EventLogEntryCodes.DateSettingError);
            LoggingActions.LogTaskResult(Log, SetTime, EventLogEntryCodes.TimeSet, EventLogEntryCodes.TimeSettingError);
            LoggingActions.LogTaskResult(Log, SyncRTC, EventLogEntryCodes.RTCSynced, EventLogEntryCodes.RTCSyncError);
        }

        private Task<ProcessResult> UnsetWiFiSequence()
        {
            return Task.Run(async () =>
            {
                ProcessResult Result = await ProcessRunner.GetProcessResultAsync(c_SedCommand, "-i '/country=/d' /etc/wpa_supplicant/wpa_supplicant.conf");

                if (Result.Okay())
                {
                    Result = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 reconfigure");
                }

                if (Utils.Hardware.isUsingNetworkManager)
                {
                    await ProcessRunner.GetProcessResultAsync(c_NetworkMonitorCliCommand, "radio wifi off");
                    await ProcessRunner.GetProcessResultAsync(c_IWCommand, "reg set 00");
                }

                return await ProcessRunner.GetProcessResultAsync(c_RFKillCommand, "block wifi");
            });
        }
    }
}
