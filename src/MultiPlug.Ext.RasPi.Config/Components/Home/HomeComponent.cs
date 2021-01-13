using System;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Models.Components.Home;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;

namespace MultiPlug.Ext.RasPi.Config.Components.Home
{
    public class HomeComponent : HomeProperties
    {
        internal event Action<Diagnostics.EventLogEntryCodes, string[]> Log;

        internal HomeProperties RepopulateAndGetProperties()
        {
            if( ! RunningRaspberryPi ) { return this; }

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[7];

            Tasks[0] = ProcessRunner.GetProcessResultAsync("cat", "/proc/device-tree/model");
            Tasks[1] = ProcessRunner.GetProcessResultAsync("cat", "/etc/debian_version");
            Tasks[2] = ProcessRunner.GetProcessResultAsync("cat", "/etc/hostname");
            Tasks[3] = ProcessRunner.GetProcessResultAsync("date");
            Tasks[4] = ProcessRunner.GetProcessResultAsync("vcgencmd", "measure_temp");
            Tasks[5] = ProcessRunner.GetProcessResultAsync("cat", "/sys/class/thermal/thermal_zone0/temp");
            Tasks[6] = ProcessRunner.GetProcessResultAsync("df", "-h");

            Task.WaitAll(Tasks);

            RaspberryPiModel    = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().TrimEnd() : string.Empty;
            OSVersion           = Tasks[1].Result.Okay() ? Tasks[1].Result.GetOutput().TrimEnd() : string.Empty;
            HostName            = Tasks[2].Result.Okay() ? Tasks[2].Result.GetOutput().TrimEnd() : string.Empty;
            Date                = Tasks[3].Result.Okay() ? Tasks[3].Result.GetOutput().TrimEnd() : string.Empty;
            GPUTemperature      = Tasks[4].Result.Okay() ? Tasks[4].Result.GetOutput().TrimEnd().Replace("temp=", "") : string.Empty;
            CPUTemperature      = "?";
            FreeDiskPercentage  = string.Empty;

            if (Tasks[5].Result.Okay())
            {
                int CPUTemp = 0;

                if (int.TryParse(Tasks[5].Result.GetOutput(), out CPUTemp))
                {
                    CPUTemperature = (CPUTemp / 1000).ToString();
                }
            }

            if ( Tasks[6].Result.Okay() )
            {
                FreeDiskPercentage = Tasks[6].Result.GetOutput().Split(new string[] { "\n", "\r\n" }, 2, StringSplitOptions.RemoveEmptyEntries)[1]
                                                                .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[4].TrimEnd('%');
            }

            // Log only if errors have occured
            LoggingActions.LogTaskResult(Log, Tasks[0], EventLogEntryCodes.RaspberryPiModelSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[1], EventLogEntryCodes.DebianVersionSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[2], EventLogEntryCodes.HostNameSettingsGetError);
            LoggingActions.LogTaskResult(Log, Tasks[3], EventLogEntryCodes.DateTimeSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[4], EventLogEntryCodes.GPUTemperatureSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[5], EventLogEntryCodes.CPUTemperatureSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[6], EventLogEntryCodes.DiskFreeSettingGetError);

            return this;
        }
    }
}
