using System.Runtime.Serialization;

namespace MultiPlug.Ext.RasPi.Config.Models.Components.Network
{
    public class NetworkProperties : SharedProperties
    {
        [DataMember]
        public string HostName { get; set; }
        public bool WiFiCountrySet { get; set; }
        [DataMember]
        public string WiFiCountry { get; set; }
        public ConnectedSSID[] SSIDs { get; set; }
        public string NewSSID { get; set; }
        public string NewPassphrase { get; set; }
        public string NewWiFiNIC { get; set; }
        public NICInterface[] Interfaces { get; set; }
        public NICProperties[] Eths { get; set; }
        public NICProperties[] Wlans { get; set; }
        public string NICInUse { get; set; }
    }
}
