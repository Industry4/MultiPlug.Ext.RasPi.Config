/// <summary>
/// Uses process calls to raspi-config
/// https://github.com/RPi-Distro/raspi-config
/// MIT License
/// </summary>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Models.Components.Interfacing;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;

namespace MultiPlug.Ext.RasPi.Config.Components.Interfacing
{
    public class InterfacingComponent : InterfacingProperties
    {
        internal event Action<Diagnostics.EventLogEntryCodes, string[]> Log;
        internal event Action RestartDue;

        internal InterfacingProperties RepopulateAndGetProperties()
        {
            if (!RunningRaspberryPi) { return this; }

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[8];

            Tasks[0] = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint get_camera");
            Tasks[1] = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint get_ssh");
            Tasks[2] = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint get_vnc");
            Tasks[3] = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint get_spi");
            Tasks[4] = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint get_i2c");
            Tasks[5] = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint get_serial");
            Tasks[6] = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint get_onewire");
            Tasks[7] = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint get_rgpio");

            Task.WaitAll(Tasks);

            this.Camera     = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().TrimEnd().Equals("0") : false;
            this.SSH        = Tasks[1].Result.Okay() ? Tasks[1].Result.GetOutput().TrimEnd().Equals("0") : false;
            this.VNC        = Tasks[2].Result.Okay() ? Tasks[2].Result.GetOutput().TrimEnd().Equals("0") : false;
            this.SPI        = Tasks[3].Result.Okay() ? Tasks[3].Result.GetOutput().TrimEnd().Equals("0") : false;
            this.I2C        = Tasks[4].Result.Okay() ? Tasks[4].Result.GetOutput().TrimEnd().Equals("0") : false;
            this.Serial     = Tasks[5].Result.Okay() ? Tasks[5].Result.GetOutput().TrimEnd().Equals("0") : false;
            this.OneWire    = Tasks[6].Result.Okay() ? Tasks[6].Result.GetOutput().TrimEnd().Equals("0") : false;
            this.RemoteGPIO = Tasks[7].Result.Okay() ? Tasks[7].Result.GetOutput().TrimEnd().Equals("0") : false;

            // Log only if errors have occured
            LoggingActions.LogTaskResult(Log, Tasks[0], EventLogEntryCodes.CameraSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[1], EventLogEntryCodes.SSHSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[2], EventLogEntryCodes.VNCSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[3], EventLogEntryCodes.SPISettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[4], EventLogEntryCodes.I2CSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[5], EventLogEntryCodes.SerialSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[6], EventLogEntryCodes.OneWireSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[7], EventLogEntryCodes.RemoteGPIOSettingGetError);

            return this;
        }

        internal void UpdateProperties(InterfacingProperties theModel)
        {
            List<Task<ProcessResult>> Tasks = new List<Task<ProcessResult>>();

            bool AskToRestart = false;

            RepopulateAndGetProperties();

            Task<ProcessResult> SetCamera = null;

            if ( Camera != theModel.Camera)
            {
                AskToRestart = true;
                LoggingActions.LogTaskAction(Log, theModel.Camera, EventLogEntryCodes.CameraSettingEnabling, EventLogEntryCodes.CameraSettingDisabling);
                SetCamera = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint do_camera " + (theModel.Camera ? "0" : "1"));
                Tasks.Add(SetCamera);
            }

            Task<ProcessResult> SetSSH = null;

            if ( SSH != theModel.SSH)
            {
                LoggingActions.LogTaskAction(Log, theModel.SSH, EventLogEntryCodes.SSHSettingEnabling, EventLogEntryCodes.SSHSettingDisabling);
                SetSSH = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint do_ssh " + (theModel.SSH ? "0" : "1"));
                Tasks.Add(SetSSH);
            }

            Task<ProcessResult> SetVNC = null;

            if ( VNC != theModel.VNC)
            {
                LoggingActions.LogTaskAction(Log, theModel.VNC, EventLogEntryCodes.VNCSettingEnabling, EventLogEntryCodes.VNCSettingDisabling);
                SetVNC = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint do_vnc " + (theModel.VNC ? "0" : "1"));
                Tasks.Add(SetVNC);
            }

            Task<ProcessResult> SetSPI = null;

            if ( SPI != theModel.SPI)
            {
                LoggingActions.LogTaskAction(Log, theModel.SPI, EventLogEntryCodes.SPISettingEnabling, EventLogEntryCodes.SPISettingDisabling);
                SetSPI = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint do_spi " + (theModel.SPI ? "0" : "1"));
                Tasks.Add(SetSPI);
            }

            Task<ProcessResult> SetI2C = null;

            if ( I2C != theModel.I2C)
            {
                LoggingActions.LogTaskAction(Log, theModel.I2C, EventLogEntryCodes.I2CSettingEnabling, EventLogEntryCodes.I2CSettingDisabling);
                SetI2C = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint do_i2c " + (theModel.I2C ? "0" : "1"));
                Tasks.Add(SetI2C);
            }

            Task<ProcessResult> SetSerial = null;

            if ( Serial != theModel.Serial)
            {
                AskToRestart = true;
                LoggingActions.LogTaskAction(Log, theModel.Serial, EventLogEntryCodes.SerialSettingEnabling, EventLogEntryCodes.SerialSettingDisabling);
                SetSerial = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint do_serial " + (theModel.Serial ? "0" : "1"));
                Tasks.Add(SetSerial);
            }

            Task<ProcessResult> SetOneWire = null;

            if ( OneWire != theModel.OneWire)
            {
                AskToRestart = true;
                LoggingActions.LogTaskAction(Log, theModel.OneWire, EventLogEntryCodes.OneWireSettingEnabling, EventLogEntryCodes.OneWireSettingDisabling);
                SetOneWire = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint do_onewire " + (theModel.OneWire ? "0" : "1"));
                Tasks.Add(SetOneWire);
            }

            Task<ProcessResult> SetRemoteGPIO = null;

            if ( RemoteGPIO != theModel.RemoteGPIO)
            {
                LoggingActions.LogTaskAction(Log, theModel.RemoteGPIO, EventLogEntryCodes.RemoteGPIOSettingEnabling, EventLogEntryCodes.RemoteGPIOSettingDisabling);
                SetRemoteGPIO = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint do_rgpio " + (theModel.RemoteGPIO ? "0" : "1"));
                Tasks.Add(SetRemoteGPIO);
            }

            Task.WaitAll( Tasks.ToArray() );

            if (AskToRestart) { RestartDue?.Invoke(); }

            // Check if Tasks have completed Okay and Log result
            LoggingActions.LogTaskResult(Log, SetCamera, theModel.Camera, EventLogEntryCodes.CameraSettingEnabled, EventLogEntryCodes.CameraSettingDisabled, EventLogEntryCodes.CameraSettingError);
            LoggingActions.LogTaskResult(Log, SetSSH, theModel.SSH, EventLogEntryCodes.SSHSettingEnabled, EventLogEntryCodes.SSHSettingDisabled, EventLogEntryCodes.SSHSettingError);
            LoggingActions.LogTaskResult(Log, SetVNC, theModel.VNC, EventLogEntryCodes.VNCSettingEnabled, EventLogEntryCodes.VNCSettingDisabled, EventLogEntryCodes.VNCSettingError);
            LoggingActions.LogTaskResult(Log, SetSPI, theModel.SPI, EventLogEntryCodes.SPISettingEnabled, EventLogEntryCodes.SPISettingDisabled, EventLogEntryCodes.SPISettingError);
            LoggingActions.LogTaskResult(Log, SetI2C, theModel.I2C, EventLogEntryCodes.I2CSettingEnabled, EventLogEntryCodes.I2CSettingDisabled, EventLogEntryCodes.I2CSettingError);
            LoggingActions.LogTaskResult(Log, SetSerial, theModel.Serial, EventLogEntryCodes.SerialSettingEnabled, EventLogEntryCodes.SerialSettingDisabled, EventLogEntryCodes.SerialSettingError);
            LoggingActions.LogTaskResult(Log, SetOneWire, theModel.OneWire, EventLogEntryCodes.OneWireSettingEnabled, EventLogEntryCodes.OneWireSettingDisabled, EventLogEntryCodes.OneWireSettingError);
            LoggingActions.LogTaskResult(Log, SetRemoteGPIO, theModel.RemoteGPIO, EventLogEntryCodes.RemoteGPIOSettingEnabled, EventLogEntryCodes.RemoteGPIOSettingDisabled, EventLogEntryCodes.RemoteGPIOSettingError);
        }
    }
}
