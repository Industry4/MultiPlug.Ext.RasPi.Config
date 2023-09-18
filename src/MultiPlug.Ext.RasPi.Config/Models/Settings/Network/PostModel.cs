
namespace MultiPlug.Ext.RasPi.Config.Models.Settings.Network
{
    public class PostModel
    {
        public string HostName { get; set; }
        public string NewSSID { get; set; }
        public string NewPassphrase { get; set; }
        public string NewWiFiNIC { get; set; }

        public string[] NICId { get; set; }
        public string[] IPAddress { get; set; }
        public string[] IPAddressCidr { get; set; }
        public string[] IP6Address { get; set; }
        public string[] IP6AddressCidr { get; set; }

        public string[] RouterNICId { get; set; }
        public string[] Router { get; set; }

        public string[] DomainNameServerNICId { get; set; }
        public string[] DomainNameServer { get; set; }

        public string[] WlanNICId { get; set; }
        public string[] WlanIPAddress { get; set; }
        public string[] WlanIPAddressCidr { get; set; }
        public string[] WlanIP6Address { get; set; }
        public string[] WlanIP6AddressCidr { get; set; }

        public string[] WlanRouterNICId { get; set; }
        public string[] WlanRouter { get; set; }

        public string[] WlanDomainNameServerNICId { get; set; }
        public string[] WlanDomainNameServer { get; set; }
    }
}
