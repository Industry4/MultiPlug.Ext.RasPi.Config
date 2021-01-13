
namespace MultiPlug.Ext.RasPi.Config.Models.Components.Network
{
    public class DHCPcdConfLine
    {
        public string Line { get; set; }
        public NICProperties NICProperties { get; set; }
        public bool Id { get; set; }
        public bool IPAddress { get; set; }
        public bool IP6Address { get; set; }
        public bool Routers { get; set; }
        public bool DomainNameServers { get; set; }
    }
}
