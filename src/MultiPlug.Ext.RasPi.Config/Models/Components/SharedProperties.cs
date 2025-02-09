using MultiPlug.Base;

namespace MultiPlug.Ext.RasPi.Config.Models.Components
{
    public class SharedProperties : MultiPlugBase
    {
        internal const string c_Eth0 = "eth0";
        internal const string c_Wlan0 = "wlan0";
        internal const string c_Enabled = "0";
        internal const string c_Disabled = "1";
        internal const string c_WiFiCountryNotSet = "FAIL";

        internal const string c_LinuxRaspconfigCommand = "raspi-config";
        internal const string c_WPACliCommand = "wpa_cli";
        internal const string c_SystemCtlCommand = "systemctl";
        internal const string c_IPCommand = "ip";
        internal const string c_LinuxSystemControlCommand = "systemctl";
        internal const string c_IFConfigCommand = "ifconfig";
        internal const string c_CatCommand = "cat";
        internal const string c_DateCommand = "date";
        internal const string c_GrepCommand = "grep";
        internal const string c_TimeDateControlCommand = "timedatectl";
        internal const string c_HardwareClockCommand = "hwclock";
        internal const string c_SedCommand = "sed";
        internal const string c_VCGenCommand = "vcgencmd";
        internal const string c_DFCommand = "df";
        internal const string c_FreeCommand = "free";
        internal const string c_IFDown = "ifdown";
        internal const string c_IFUp = "ifup";
        internal const string c_JournalCommand = "journalctl";
        internal const string c_RFKillCommand = "rfkill";
        internal const string c_NetworkMonitorCliCommand = "nmcli";
        internal const string c_IWCommand = "iw";
    }
}
