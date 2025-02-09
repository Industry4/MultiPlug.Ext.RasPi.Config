using System;
using System.Linq;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;
using MultiPlug.Ext.RasPi.Config.Models.Components.Home;

namespace MultiPlug.Ext.RasPi.Config.Components.Home
{
    public class HomeComponent : HomeProperties
    {
        internal event Action<Diagnostics.EventLogEntryCodes, string[]> Log;

        internal HomeProperties RepopulateAndGetProperties()
        {
            if( ! Utils.Hardware.isRunningRaspberryPi ) { return this; }

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[7];

            Tasks[0] = ProcessRunner.GetProcessResultAsync(c_CatCommand, "/proc/device-tree/model");
            Tasks[1] = ProcessRunner.GetProcessResultAsync(c_CatCommand, "/etc/debian_version");
            Tasks[2] = ProcessRunner.GetProcessResultAsync(c_CatCommand, "/etc/hostname");
            Tasks[3] = ProcessRunner.GetProcessResultAsync(c_DateCommand);
            Tasks[4] = GetGPUTemperature();
            Tasks[5] = GetCPUTemperature();
            Tasks[6] = GetDiskSpacePercentage();

            Task.WaitAll(Tasks);

            RaspberryPiModel    = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().TrimEnd() : string.Empty;
            OSVersion           = Tasks[1].Result.Okay() ? Tasks[1].Result.GetOutput().TrimEnd() : string.Empty;
            HostName            = Tasks[2].Result.Okay() ? Tasks[2].Result.GetOutput().TrimEnd() : string.Empty;
            Date                = Tasks[3].Result.Okay() ? Tasks[3].Result.GetOutput().TrimEnd() : string.Empty;
            GPUTemperature      = ProcessGPUTemperature(Tasks[4]);
            CPUTemperature      = ProcessCPUTemperature(Tasks[5]);
            FreeDiskPercentage = ProcessDiskSpacePercentage(Tasks[6]);

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

        internal static Task<ProcessResult> GetJournalEntry(string theService, bool thisboot)
        {
            return ProcessRunner.GetProcessResultAsync(c_JournalCommand, (string.IsNullOrEmpty(theService) ? string.Empty : " -u " + theService ) + (thisboot ? " -b" : string.Empty ) );
        }

        internal static Task<ProcessResult> GetGPUTemperature()
        {
            return ProcessRunner.GetProcessResultAsync(c_VCGenCommand, "measure_temp");
        }

        internal static Task<ProcessResult> GetCPUTemperature()
        {
            return ProcessRunner.GetProcessResultAsync(c_CatCommand, "/sys/class/thermal/thermal_zone0/temp");
        }

        internal static Task<ProcessResult> GetDiskSpacePercentage()
        {
            return ProcessRunner.GetProcessResultAsync(c_DFCommand, "-h");
        }

        internal static string ProcessGPUTemperature(Task<ProcessResult> theTask)
        {
            return theTask.Result.Okay() ? theTask.Result.GetOutput().TrimEnd().Replace("temp=", "") : string.Empty;
        }

        internal static string ProcessCPUTemperature(Task<ProcessResult> theTask)
        {
            if (theTask.Result.Okay())
            {
                int CPUTemp = 0;

                if (int.TryParse(theTask.Result.GetOutput(), out CPUTemp))
                {
                    return (CPUTemp / 1000).ToString();
                }
            }

            return "?";
        }

        internal static string ProcessDiskSpacePercentage(Task<ProcessResult> theTask)
        {
            if (theTask.Result.Okay())
            {
                try
                {
                    return theTask.Result.GetOutput()
                        .Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries) // Each Line
                        .Where( line => line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Equals("/") /* Mounted on */)
                        .First() // Should be only 1
                        .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[4 /* % Used */].TrimEnd('%');
                }
                catch
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        internal static string ProcessJournal(Task<ProcessResult> theTask)
        {
            if (theTask.Result.Okay())
            {
                return theTask.Result.GetOutput();
            }

            return string.Empty;
        }
    }
}
