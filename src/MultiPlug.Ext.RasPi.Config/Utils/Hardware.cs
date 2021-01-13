using System;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Models.Components;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;

namespace MultiPlug.Ext.RasPi.Config.Utils
{
    internal static class Hardware
    {
        internal static void CheckRunningRaspberryPi(IRunningRaspberryPiProperty[] thePropertyArray)
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                Task<ProcessResult> RaspberryPiModel = ProcessRunner.GetProcessResultAsync("cat", "/proc/device-tree/model");

                RaspberryPiModel.Wait();

                if( RaspberryPiModel.Result.Okay())
                {
                    Array.ForEach(thePropertyArray, x => x.RunningRaspberryPi = true);
                }
            }
        }
    }
}
