using System.Runtime.Serialization;

namespace MultiPlug.Ext.RasPi.Config.Models.Components.Hat
{
    public class HatProperties : SharedProperties
    {
        [DataMember]
        public string Product { get; internal set; }
        [DataMember]
        public string ProductId { get; internal set; }
        [DataMember]
        public string ProductVersion { get; internal set; }
        [DataMember]
        public string UUID { get; internal set; }
        [DataMember]
        public string Vendor { get; internal set; }
    }
}
