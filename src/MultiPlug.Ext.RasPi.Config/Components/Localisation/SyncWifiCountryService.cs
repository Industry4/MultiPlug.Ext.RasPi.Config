using System;
using System.Threading.Tasks;
using System.Timers;
using MultiPlug.Base.Exchange;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;

namespace MultiPlug.Ext.RasPi.Config.Components.Localisation
{
    internal class SyncWifiCountryService
    {
        IMultiPlugAPI m_MultiPlugAPI;

        private Timer m_Timer = new Timer();

        internal event Action Synced;
        internal event Action<EventLogEntryCodes, string[]> Log;

        private string m_LastSyncMultiPlugValue = null;

        internal SyncWifiCountryService(IMultiPlugAPI theMultiPlugAPI)
        {
            m_MultiPlugAPI = theMultiPlugAPI;
            m_Timer.Interval = 30000;
            m_Timer.AutoReset = false;
            m_Timer.Elapsed += OnSleepMatured;
        }

        internal void Reset()
        {
            m_LastSyncMultiPlugValue = null;
        }

        internal void Begin()
        {
            if( ! Utils.Hardware.isRunningRaspberryPi )
            {
                return;
            }

            if (m_LastSyncMultiPlugValue != null && m_LastSyncMultiPlugValue.Equals(m_MultiPlugAPI.Configuration.Localisation.Country))
            {
                // MultiPlug Country hasn't changed since last Sync
                return;
            }

            if (!m_Timer.Enabled)
            {
                Task SyncTask = new Task(() => OnSleepMatured(null, null));
                SyncTask.Start();
            }
        }

        private void OnSleepMatured(object sender, ElapsedEventArgs e)
        {
            if( m_MultiPlugAPI.Configuration.Localisation.Country.Equals(string.Empty) )
            {
                m_Timer.Enabled = true;
            }
            else
            {
                m_LastSyncMultiPlugValue = m_MultiPlugAPI.Configuration.Localisation.Country;

                Task<ProcessResult> CurrentWifiCode = ProcessRunner.GetProcessResultAsync("wpa_cli", "-i wlan0 get country");

                CurrentWifiCode.Wait();

                string WifiCountry = CurrentWifiCode.Result.Okay() ? CurrentWifiCode.Result.GetOutput() : string.Empty;

                if ( ! WifiCountry.Equals(m_LastSyncMultiPlugValue))
                {
                    Log?.Invoke(EventLogEntryCodes.WifiCountrySetting, new string[] { m_LastSyncMultiPlugValue });
                    Task<ProcessResult> SetWifiCountry = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint do_wifi_country " + m_LastSyncMultiPlugValue);
                    SetWifiCountry.Wait();
                    LoggingActions.LogTaskResult(Log, SetWifiCountry, EventLogEntryCodes.WifiCountrySet, EventLogEntryCodes.WifiCountrySettingError);
                    Synced?.Invoke();
                }
            }
        }
    }
}
