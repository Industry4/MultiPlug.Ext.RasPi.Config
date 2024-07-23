/// <summary>
/// Uses process calls to raspi-config
/// https://github.com/RPi-Distro/raspi-config
/// MIT License
/// </summary>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Models.Components.Boot;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;

namespace MultiPlug.Ext.RasPi.Config.Components.Boot
{
    public class BootComponent : BootProperties
    {
        internal event Action<EventLogEntryCodes, string[]> Log;
        internal event Action RestartDue;

        internal BootProperties RepopulateAndGetProperties()
        {
            if (!Utils.Hardware.isRunningRaspberryPi) { return this; }

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[2];

            Tasks[0] = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint get_boot_wait");
            Tasks[1] = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint get_boot_splash");

            Task.WaitAll(Tasks);

            NetworkWait     = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().TrimEnd().Equals("0") : false;
            SplashScreen    = Tasks[1].Result.Okay() ? Tasks[1].Result.GetOutput().TrimEnd().Equals("0") : false;

            // Log only if errors have occured
            LoggingActions.LogTaskResult(Log, Tasks[0], EventLogEntryCodes.NetworkWaitSettingsGetError);
            LoggingActions.LogTaskResult(Log, Tasks[1], EventLogEntryCodes.SplashScreenSettingsGetError);

            return this;
        }

        internal void UpdateProperties(BootProperties theModel)
        {
            List<Task<ProcessResult>> Tasks = new List<Task<ProcessResult>>();

            bool AskToRestart = false;

            RepopulateAndGetProperties();

            Task<ProcessResult> SetBootBehaviour = null;

            if (theModel.BootBehaviour != 0)
            {
                AskToRestart = true;
                LoggingActions.LogTaskAction(Log, (theModel.BootBehaviour - 1), new EventLogEntryCodes[] {
                    EventLogEntryCodes.BootBehaviourSettingConsole,
                    EventLogEntryCodes.BootBehaviourSettingConsoleAutologin,
                    EventLogEntryCodes.BootBehaviourSettingDesktop,
                    EventLogEntryCodes.BootBehaviourSettingDesktopAutologin });
                SetBootBehaviour = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_boot_behaviour B" + theModel.BootBehaviour);
                Tasks.Add(SetBootBehaviour);
            }

            Task<ProcessResult> SetNetworkWait = null;

            if (NetworkWait != theModel.NetworkWait)
            {
                LoggingActions.LogTaskAction(Log, theModel.NetworkWait, EventLogEntryCodes.NetworkWaitSettingTrue, EventLogEntryCodes.NetworkWaitSettingFalse);
                SetNetworkWait = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_boot_wait " + (theModel.NetworkWait ? "0" : "1"));
                Tasks.Add(SetNetworkWait);
            }

            Task<ProcessResult> SetSplashScreen = null;

            if (SplashScreen != theModel.SplashScreen)
            {
                LoggingActions.LogTaskAction(Log, theModel.SplashScreen, EventLogEntryCodes.SplashScreenSettingTrue, EventLogEntryCodes.SplashScreenSettingFalse);
                SetSplashScreen = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_boot_splash " + (theModel.SplashScreen ? "0" : "1"));
                Tasks.Add(SetSplashScreen);
            }

            Task<ProcessResult> SetBootOrder = null;

            if (theModel.BootOrder != 0)
            {
                LoggingActions.LogTaskAction(Log, (theModel.BootOrder - 1), new EventLogEntryCodes[] {
                    EventLogEntryCodes.BootOrderSettingSDCard,
                    EventLogEntryCodes.BootOrderSettingUSB,
                    EventLogEntryCodes.BootOrderSettingNetwork });
                SetBootOrder = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_boot_order B" + theModel.BootOrder);
                Tasks.Add(SetBootOrder);
            }

            Task<ProcessResult> SetBootROM = null;

            if (theModel.BootROM != 0)
            {
                AskToRestart = true;
                LoggingActions.LogTaskAction(Log, (theModel.BootROM - 1), new EventLogEntryCodes[] {
                    EventLogEntryCodes.BootROMSettingLatest,
                    EventLogEntryCodes.BootROMSettingDefault });
                SetBootROM = ProcessRunner.GetProcessResultAsync(c_LinuxRaspconfigCommand, "nonint do_boot_rom E" + theModel.BootROM);
                Tasks.Add(SetBootROM);
            }

            if (AskToRestart) { RestartDue?.Invoke(); }

            Task.WaitAll(Tasks.ToArray());

            // Check if Tasks have completed Okay and Log result
            LoggingActions.LogTaskResult(Log, SetBootBehaviour, EventLogEntryCodes.BootBehaviourSet, EventLogEntryCodes.BootBehaviourSettingError);
            LoggingActions.LogTaskResult(Log, SetNetworkWait, EventLogEntryCodes.NetworkWaitSet, EventLogEntryCodes.NetworkWaitSettingError);
            LoggingActions.LogTaskResult(Log, SetSplashScreen, EventLogEntryCodes.SplashScreenSet, EventLogEntryCodes.SplashScreenSettingError);
            LoggingActions.LogTaskResult(Log, SetBootOrder, EventLogEntryCodes.BootOrderSet, EventLogEntryCodes.BootOrderSettingError);
            LoggingActions.LogTaskResult(Log, SetBootROM, EventLogEntryCodes.BootROMSet, EventLogEntryCodes.BootROMSettingError);
        }
    }
}
