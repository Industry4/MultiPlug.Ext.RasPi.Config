using System;
using System.Threading.Tasks;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using System.IO;

namespace MultiPlug.Ext.RasPi.Config.Utils
{
    public static class Hardware
    {
        public static bool isRunningRaspberryPi { get; private set; } = false;
        public static bool isRunningRaspberryPi5 { get; private set; } = false;
        public static bool isRunningMono { get; private set; } = false;

        public static bool RebootUserPrompt { get; private set; } = false;

        public static bool PermissionsErrorRestart { get; private set; } = false;

        public static string ConfigPath { get; private set; } = string.Empty;

        public static bool isUsingNetworkManager { get; set; }


        internal static void CheckRunningRaspberryPi()
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                isRunningMono = true;

                Task<ProcessResult>[] Tasks = new Task<ProcessResult>[3];

                Tasks[0] = ProcessRunner.GetProcessResultAsync("cat", "/proc/device-tree/model");
                Tasks[1] = ProcessRunner.GetProcessResultAsync("raspi-config", "nonint is_pifive");
                Tasks[2] = ProcessRunner.GetProcessResultAsync("systemctl", "-q is-active NetworkManager");

                Task.WaitAll(Tasks);

                if (Tasks[0].Result.Okay())
                {
                    isRunningRaspberryPi = true;

                    ConfigPath = File.Exists("/boot/firmware/config.txt") ? "/boot/firmware/config.txt" : "/boot/config.txt";

                    if (Tasks[1].Result.Okay())
                    {
                        isRunningRaspberryPi5 = true;
                    }
                }

                isUsingNetworkManager = Tasks[2].Result.Okay();
            }
        }

        internal static void SetRebootUserPrompt()
        {
            RebootUserPrompt = true;
        }

        internal static void SetPermissionsErrorRestart()
        {
            PermissionsErrorRestart = true;
        }
    }
}
