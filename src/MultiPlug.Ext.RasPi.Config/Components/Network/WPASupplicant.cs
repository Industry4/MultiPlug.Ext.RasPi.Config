using System.IO;
using MultiPlug.Ext.RasPi.Config.Models.Components.Network;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;

namespace MultiPlug.Ext.RasPi.Config.Components.Network
{
    internal static class WPASupplicant
    {
        const string HomePath = "/etc/wpa_supplicant/wpa_supplicant";
        const string Extension = ".conf";
        const string PostFix = "-wlan";

        private const string c_WPACliCommand = "wpa_cli";
        private const string c_WPASupplicant = "wpa_supplicant";

        private static bool Lock = false;

        internal static bool ManageInterfaceCountChanges(NICInterface[] theWiFiNICInterfaces)
        {
            if(Lock)
            {
                return false;
            }
            else
            {
                Lock = true;
            }

            var wpa_supplicant_conf = HomePath + Extension;
            var wpa_supplicant_wlan0_conf = HomePath + PostFix + "0" + Extension;

            bool Changes = false;

            if (theWiFiNICInterfaces.Length == 1)
            {
                if (File.Exists(wpa_supplicant_conf))
                {
                    return Changes;
                }
                else
                {
                    if (File.Exists(wpa_supplicant_wlan0_conf))
                    {

                        ProcessRunner.GetProcessResultAsync(c_WPACliCommand, " terminate -i " + theWiFiNICInterfaces[0].Name).Wait();

                        File.Move(wpa_supplicant_wlan0_conf, wpa_supplicant_conf);

                        ProcessRunner.GetProcessResultAsync(c_WPASupplicant, "-B -i " + theWiFiNICInterfaces[0].Name + " -c " + wpa_supplicant_conf).Wait();

                        Changes = true;

                        // TODO Tidy up old wpa_supplicant_wlan1,2,3,4,5..._conf files ?
                    }
                    else
                    {
                        // Not Found
                    }
                }
            }
            else if (theWiFiNICInterfaces.Length > 1 )
            {
                foreach (var item in theWiFiNICInterfaces)
                {
                    ProcessRunner.GetProcessResultAsync(c_WPACliCommand, " terminate -i " + item.Name).Wait();
                }

                if (File.Exists(wpa_supplicant_conf))
                {
                    File.Move(wpa_supplicant_conf, wpa_supplicant_wlan0_conf);

                    Changes = true;

                    if (CreateFiles(1, theWiFiNICInterfaces.Length, wpa_supplicant_wlan0_conf) )
                    {
                        Changes = true;
                    }
                }
                else
                {
                    if( File.Exists(wpa_supplicant_wlan0_conf) && CreateFiles(0, theWiFiNICInterfaces.Length, wpa_supplicant_wlan0_conf))
                    {
                        Changes = true;
                    }
                }

                foreach (var item in theWiFiNICInterfaces)
                {
                    ProcessRunner.GetProcessResultAsync(c_WPASupplicant, "-B -i " + item.Name + " -c " + HomePath + "-" + item.Name + ".conf").Wait();
                }
            }

            Lock = false;
            return Changes;
        }

        private static bool CreateFiles(int start, int length, string thePathToCopy)
        {
            bool Changes = false;

            for ( int i = start; i < length; i++)
            {
                var wpa_supplicant_wlanX_conf = HomePath + PostFix + i.ToString() + Extension;

                if( ! File.Exists(wpa_supplicant_wlanX_conf))
                {
                    File.Copy(thePathToCopy, wpa_supplicant_wlanX_conf);

                    Changes = true;
                }
            }

            return Changes;
        }
    }
}
