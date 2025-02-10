using System;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Models.Components.Performance;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;

namespace MultiPlug.Ext.RasPi.Config.Components.Performance
{
    internal class PerformanceComponent : PerformanceProperties
    {
        internal event Action<Diagnostics.EventLogEntryCodes, string[]> Log;

        private const int c_UndervoltageDetected = 0x1;
        private const int c_ArmFrequencyCapped = 0x2;
        private const int c_CurrentlyThrottled = 0x4;
        private const int c_SoftTemperatureLimitActive = 0x8;
        private const int c_UndervoltageHasOccurred = 0x10000;
        private const int c_ArmFrequencyCappingHasOccurred = 0x20000;
        private const int c_ThrottlingHasOccurred = 0x40000;
        private const int c_SoftTemperatureLimitHasOccurred = 0x80000;

        internal PerformanceProperties RepopulateAndGetProperties()
        {
            if (!Utils.Hardware.isRunningRaspberryPi) { return this; }

            Task<ProcessResult>[] Tasks;

            if (Utils.Hardware.isRunningRaspberryPi5 == false)
            {
                Tasks = new Task<ProcessResult>[2];
                Tasks[0] = ProcessRunner.GetProcessResultAsync(c_VCGenCommand, "get_throttled");
                Tasks[1] = ProcessRunner.GetProcessResultAsync(c_GrepCommand, "^dtoverlay=gpio-fan " + Utils.Hardware.ConfigPath);
            }
            else
            {
                Tasks = new Task<ProcessResult>[1];
                Tasks[0] = ProcessRunner.GetProcessResultAsync(c_VCGenCommand, "get_throttled");
            }

            Task.WaitAll(Tasks);

            UndervoltageDetected = false;
            ArmFrequencyCapped = false;
            CurrentlyThrottled = false;
            SoftTemperatureLimitActive = false;
            UndervoltageHasOccurred = false;
            ArmFrequencyCappingHasOccurred = false;
            ThrottlingHasOccurred = false;
            SoftTemperatureLimitHasOccurred = false;

            if (Tasks[0].Result.Okay())
            {
                var SplitByX = Tasks[0].Result.GetOutput().Split(new char[] { 'x' });

                if(SplitByX.Length > 1)
                {
                    var Result = SplitByX[1].Trim();

                    int HexValue = int.Parse(Result, System.Globalization.NumberStyles.AllowHexSpecifier);

                    if((HexValue & c_UndervoltageDetected) != 0)
                    {
                        UndervoltageDetected = true;
                    }
                    if ((HexValue & c_ArmFrequencyCapped) != 0)
                    {
                        ArmFrequencyCapped = true;
                    }
                    if ((HexValue & c_CurrentlyThrottled) != 0)
                    {
                        CurrentlyThrottled = true;
                    }
                    if ((HexValue & c_SoftTemperatureLimitActive) != 0)
                    {
                        SoftTemperatureLimitActive = true;
                    }
                    if ((HexValue & c_UndervoltageHasOccurred) != 0)
                    {
                        UndervoltageHasOccurred = true;
                    }
                    if ((HexValue & c_ArmFrequencyCappingHasOccurred) != 0)
                    {
                        ArmFrequencyCappingHasOccurred = true;
                    }
                    if ((HexValue & c_ThrottlingHasOccurred) != 0)
                    {
                        ThrottlingHasOccurred = true;
                    }
                    if ((HexValue & c_SoftTemperatureLimitHasOccurred) != 0)
                    {
                        SoftTemperatureLimitHasOccurred = true;
                    }
                }
            }

            if ( Utils.Hardware.isRunningRaspberryPi5 == false)
            {
                FanEnabled = false;

                if (Tasks[1].Result.Okay())
                {
                    string Result = Tasks[1].Result.GetOutput();

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

            if ( Utils.Hardware.isRunningRaspberryPi5 == false )
            {

                // Disable
                if (CurrentValue.FanEnabled && !theModel.FanEnabled)
                {
                    var Task = ProcessRunner.GetProcessResultAsync(c_SedCommand, Utils.Hardware.ConfigPath + " -i -e \"/^.*dtoverlay=gpio-fan.*/d\"");
                    Task.Wait();
                    LoggingActions.LogTaskResult(Log, Task, theModel.FanEnabled, EventLogEntryCodes.PerformanceFanEnabled, EventLogEntryCodes.PerformanceFanDisabled, EventLogEntryCodes.PerformanceFanEnabledError);
                }
                else
                {
                    // Enable
                    if (!CurrentValue.FanEnabled && theModel.FanEnabled)
                    {
                        var Task = ProcessRunner.GetProcessResultAsync(c_SedCommand, Utils.Hardware.ConfigPath + " -i -e \"\\$adtoverlay=gpio-fan,gpiopin=" + theModel.FanGPIO + ",temp=" + theModel.FanTemperature + "000\"");
                        Task.Wait();
                        LoggingActions.LogTaskResult(Log, Task, theModel.FanEnabled, EventLogEntryCodes.PerformanceFanEnabled, EventLogEntryCodes.PerformanceFanDisabled, EventLogEntryCodes.PerformanceFanDisabledError);
                    }
                    // Replace
                    else if (CurrentValue.FanEnabled && (CurrentValue.FanGPIO != theModel.FanGPIO || CurrentValue.FanTemperature != theModel.FanTemperature))
                    {
                        var Task = ProcessRunner.GetProcessResultAsync(c_SedCommand, Utils.Hardware.ConfigPath + " -i -e \"s/^.*dtoverlay=gpio-fan.*/dtoverlay=gpio-fan,gpiopin=" + theModel.FanGPIO + ",temp=" + theModel.FanTemperature + "000/\"");
                        Task.Wait();
                        LoggingActions.LogTaskResult(Log, Task, EventLogEntryCodes.PerformanceFanGPIOOrTempChanged, EventLogEntryCodes.PerformanceFanGPIOOrTempChangedError);
                    }
                }
            }
        }
    }
}
