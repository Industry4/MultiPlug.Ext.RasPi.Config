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

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[9];

            Tasks[0] = ProcessRunner.GetProcessResultAsync(c_CatCommand, "/usr/share/zoneinfo/iso3166.tab");
            Tasks[1] = ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 get country");
            Tasks[2] = ProcessRunner.GetProcessResultAsync(c_TimeDateControlCommand, "list-timezones");
            Tasks[3] = ProcessRunner.GetProcessResultAsync(c_CatCommand, "/etc/timezone");
            Tasks[4] = ProcessRunner.GetProcessResultAsync(c_DateCommand, "+%d/%m/%Y");
            Tasks[5] = ProcessRunner.GetProcessResultAsync(c_DateCommand, "+%T");
            Tasks[6] = ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "is-active systemd-timesyncd");
            Tasks[7] = ProcessRunner.GetProcessResultAsync(c_SystemCtlCommand, "is-active fake-hwclock");
            Tasks[8] = ProcessRunner.GetProcessResultAsync(c_HardwareClockCommand, "-r");

            Task.WaitAll(Tasks);

            WifiCountry         = Tasks[1].Result.Okay() ? Tasks[1].Result.GetOutput() : string.Empty;
            WifiCountries       = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(25).Select(line => new string[] { line.Split('\t')[0], line.Split('\t')[1], line.StartsWith(WifiCountry) ? "selected=\"selected\"" : "" }).ToArray() : new string[0][];
            TimeZone            = Tasks[3].Result.Okay() ? Tasks[3].Result.GetOutput().TrimEnd() : string.Empty;
            TimeZones           = Tasks[2].Result.Okay() ? Tasks[2].Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(25).Select(line => new string[] { line, line.StartsWith(TimeZone) ? "selected=\"selected\"" : "" }).ToArray() : new string[0][];
            Date                = Tasks[4].Result.Okay() ? Tasks[4].Result.GetOutput().TrimEnd() : string.Empty;
            Time                = Tasks[5].Result.Okay() ? Tasks[5].Result.GetOutput().TrimEnd() : string.Empty;
            TimeSyncdEnabled    = Tasks[6].Result.Okay(); // Will Error if inactive
            FakeHWClockEnabled  = Tasks[7].Result.Okay(); // Will Error if inactive
            HWClockPresent      = Tasks[8].Result.Okay();

            // Log only if errors have occured
            LoggingActions.LogTaskResult(Log, Tasks[0], EventLogEntryCodes.WiFiCountriesSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[1], EventLogEntryCodes.WiFiCountrySettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[2], EventLogEntryCodes.TimeZonesSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[3], EventLogEntryCodes.TimeZoneSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[4], EventLogEntryCodes.DateSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[5], EventLogEntryCodes.TimeSettingGetError);

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

            if (FakeHWClockEnabled != theModel.FakeHWClockEnabled)
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

            Task<ProcessResult> SetWifiCountry = null;

            if (WifiCountry != theModel.WifiCountry)
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

                if( Result.Okay())
                {
                    Result = await ProcessRunner.GetProcessResultAsync(c_WPACliCommand, "-i wlan0 reconfigure");
                }

                return Result;
            });
        }
    }
}
