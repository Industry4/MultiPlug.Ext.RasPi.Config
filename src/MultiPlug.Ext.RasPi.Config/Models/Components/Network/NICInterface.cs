

using MultiPlug.Base;

namespace MultiPlug.Ext.RasPi.Config.Models.Components.Network
{
    public class NICInterface : MultiPlugBase
    {
        public string Broadcast { get; set; } = string.Empty;
        public string Inet { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Netmask { get; set; } = string.Empty;
    }
}
