using System;
using MultiPlug.Ext.RasPi.Config.Models.Components.Performance;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;

namespace MultiPlug.Ext.RasPi.Config.Components.Performance
{
    internal class PerformanceComponent : PerformanceProperties
    {
        internal event Action<Diagnostics.EventLogEntryCodes, string[]> Log;

        internal PerformanceProperties RepopulateAndGetProperties()
        {
            if (!Utils.Hardware.isRunningRaspberryPi) { return this; }

            var Task = ProcessRunner.GetProcessResultAsync(c_GrepCommand, "^dtoverlay=gpio-fan /boot/config.txt");
            Task.Wait();

            FanEnabled = false;

            if ( Task.Result.Okay())
            {
                string Result = Task.Result.GetOutput();

                if ( string.IsNullOrEmpty(Result) )
                {
                    SetFanDefaults();
                }
                else
                {
                    string[] Columns = Result.Split(',');

                    if (Columns.Length == 3)
                    {
                        FanEnabled = true;
                        foreach (string Property in Columns)
                        {
                            if (Property.StartsWith("gpiopin="))
                            {
                                FanGPIO = Property.Split('=')[1].TrimEnd();
                            }
                            else if (Property.StartsWith("temp="))
                            {
                                var Temp = Property.Split('=')[1];
                                FanTemperature = Temp.TrimEnd().Substring(0, Temp.Length - 4);
                            }
                        }
                    }
                }
            }
            else
            {
                SetFanDefaults();
            }

            return this;
        }

        private void SetFanDefaults()
        {
            FanEnabled = false;
            FanGPIO = "14";
            FanTemperature = "60";
        }

        internal void UpdateProperties(PerformanceProperties theModel)
        {
            var CurrentValue = RepopulateAndGetProperties();

            // Disable
            if( CurrentValue.FanEnabled && !theModel.FanEnabled)
            {
                var Task = ProcessRunner.GetProcessResultAsync(c_SedCommand, "/boot/config.txt -i -e \"/^.*dtoverlay=gpio-fan.*/d\"");
                Task.Wait();
                LoggingActions.LogTaskResult(Log, Task, theModel.FanEnabled, EventLogEntryCodes.PerformanceFanEnabled, EventLogEntryCodes.PerformanceFanDisabled, EventLogEntryCodes.PerformanceFanEnabledError);
            }
            else
            {
                // Enable
                if( ! CurrentValue.FanEnabled && theModel.FanEnabled)
                {
                    var Task = ProcessRunner.GetProcessResultAsync(c_SedCommand, "/boot/config.txt -i -e \"\\$adtoverlay=gpio-fan,gpiopin=" + theModel.FanGPIO + ",temp=" + theModel.FanTemperature + "000\"");
                    Task.Wait();
                    LoggingActions.LogTaskResult(Log, Task, theModel.FanEnabled, EventLogEntryCodes.PerformanceFanEnabled, EventLogEntryCodes.PerformanceFanDisabled, EventLogEntryCodes.PerformanceFanDisabledError);
                }
                // Replace
                else if(CurrentValue.FanEnabled && (CurrentValue.FanGPIO != theModel.FanGPIO || CurrentValue.FanTemperature != theModel.FanTemperature))
                {
                    var Task = ProcessRunner.GetProcessResultAsync(c_SedCommand, "/boot/config.txt -i -e \"s/^.*dtoverlay=gpio-fan.*/dtoverlay=gpio-fan,gpiopin=" + theModel.FanGPIO + ",temp=" + theModel.FanTemperature + "000/\"");
                    Task.Wait();
                    LoggingActions.LogTaskResult(Log, Task, EventLogEntryCodes.PerformanceFanGPIOOrTempChanged, EventLogEntryCodes.PerformanceFanGPIOOrTempChangedError);
                }
            }
        }
    }
}
