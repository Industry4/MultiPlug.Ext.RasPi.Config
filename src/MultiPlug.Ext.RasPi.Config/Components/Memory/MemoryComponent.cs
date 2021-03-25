using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Models.Components.Memory;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;

namespace MultiPlug.Ext.RasPi.Config.Components.Memory
{
    public class MemoryComponent : MemoryProperties
    {
        internal event Action<EventLogEntryCodes, string[]> Log;

        internal MemoryProperties RepopulateAndGetProperties()
        {
            if (!Utils.Hardware.isRunningRaspberryPi) { return this; }

            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[2];

            Tasks[0] = ProcessRunner.GetProcessResultAsync(c_DFCommand, "-h");
            Tasks[1] = ProcessRunner.GetProcessResultAsync(c_FreeCommand, "-h -t");

            Task.WaitAll(Tasks);

            this.FreeDisk = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(line => line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))
                .ToArray()
                : new string[0][];

            this.FreeRam = Tasks[1].Result.Okay() ? Tasks[1].Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(line => line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))
                .ToArray()
                : new string[0][];

            LoggingActions.LogTaskResult(Log, Tasks[0], EventLogEntryCodes.DiskFreeSettingGetError);
            LoggingActions.LogTaskResult(Log, Tasks[1], EventLogEntryCodes.RAMFreeSettingGetError);

            return this;
        }
    }
}
