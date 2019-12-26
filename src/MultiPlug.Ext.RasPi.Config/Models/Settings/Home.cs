using MultiPlug.Base;

namespace MultiPlug.Ext.RasPi.Config.Models.Settings
{
    public class Home : MultiPlugBase
    {
        public string Eth0IPAddress { get; set; }
        public string Eth0IP6Address { get; set; }
        public string[] Eth0Routers { get; set; }
        public string[] Eth0DomainNameServers { get; set; }

        public string Wlan0IPAddress { get; set; }
        public string Wlan0IP6Address { get; set; }
        public string[] Wlan0Routers { get; set; }
        public string[] Wlan0DomainNameServers { get; set; }
    }
}
