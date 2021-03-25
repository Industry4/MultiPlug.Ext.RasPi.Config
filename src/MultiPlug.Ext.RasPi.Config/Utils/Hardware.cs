using System;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;

namespace MultiPlug.Ext.RasPi.Config.Utils
{
    public static class Hardware
    {
        public static bool isRunningRaspberryPi { get; private set; } = false;
        public static bool isRunningMono { get; private set; } = false;

        public static bool RebootUserPrompt { get; private set; } = false;


        internal static void CheckRunningRaspberryPi()
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                isRunningMono = true;

                Task<ProcessResult> RaspberryPiModel = ProcessRunner.GetProcessResultAsync("cat", "/proc/device-tree/model");

                RaspberryPiModel.Wait();

                if( RaspberryPiModel.Result.Okay())
                {
                    isRunningRaspberryPi = true;
                }
            }
        }

        internal static void SetRebootUserPrompt()
        {
            RebootUserPrompt = true;
        }
    }
}
