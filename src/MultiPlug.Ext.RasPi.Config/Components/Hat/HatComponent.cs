using System;
using System.IO;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Models.Components.Hat;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Diagnostics;

namespace MultiPlug.Ext.RasPi.Config.Components.Hat
{
    public class HatComponent : HatProperties
    {
        internal event Action<EventLogEntryCodes, string[]> Log;

        internal HatProperties RepopulateAndGetProperties()
        {
            if (!RunningRaspberryPi) { return this; }

            if (Directory.Exists("/proc/device-tree/hat/"))
            {
                Task<ProcessResult>[] Tasks = new Task<ProcessResult>[5];

                Tasks[0] = ProcessRunner.GetProcessResultAsync("cat", "/proc/device-tree/hat/product");
                Tasks[1] = ProcessRunner.GetProcessResultAsync("cat", "/proc/device-tree/hat/product_id");
                Tasks[2] = ProcessRunner.GetProcessResultAsync("cat", "/proc/device-tree/hat/product_ver");
                Tasks[3] = ProcessRunner.GetProcessResultAsync("cat", "/proc/device-tree/hat/uuid");
                Tasks[4] = ProcessRunner.GetProcessResultAsync("cat", "/proc/device-tree/hat/vendor");

                Task.WaitAll(Tasks);

                Product        = Tasks[0].Result.Okay() ? Tasks[0].Result.GetOutput() : string.Empty;
                ProductId      = Tasks[1].Result.Okay() ? Tasks[1].Result.GetOutput() : string.Empty;
                ProductVersion = Tasks[2].Result.Okay() ? Tasks[2].Result.GetOutput() : string.Empty;
                UUID           = Tasks[3].Result.Okay() ? Tasks[3].Result.GetOutput() : string.Empty;
                Vendor         = Tasks[4].Result.Okay() ? Tasks[4].Result.GetOutput() : string.Empty;

                // Log only if errors have occured
                LoggingActions.LogTaskResult(Log, Tasks[0], EventLogEntryCodes.HATSettingsGetProductError);
                LoggingActions.LogTaskResult(Log, Tasks[1], EventLogEntryCodes.HATSettingsGetProductIdError);
                LoggingActions.LogTaskResult(Log, Tasks[2], EventLogEntryCodes.HATSettingsGetProductVersionError);
                LoggingActions.LogTaskResult(Log, Tasks[3], EventLogEntryCodes.HATSettingsGetUUIDError);
                LoggingActions.LogTaskResult(Log, Tasks[4], EventLogEntryCodes.HATSettingsGetVendorError);
            }     
            else
            {
                Product = string.Empty;
                ProductId = string.Empty;
                ProductVersion = string.Empty;
                UUID = string.Empty;
                Vendor = string.Empty;
            }

            return this;
        }
    }
}
