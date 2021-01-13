using System.Runtime.Serialization;

namespace MultiPlug.Ext.RasPi.Config.Models.Components.Interfacing
{
    public class InterfacingProperties : SharedProperties
    {
        [DataMember]
        public bool Camera { get; set; }
        [DataMember]
        public bool I2C { get; set; }
        [DataMember]
        public bool OneWire { get; set; }
        [DataMember]
        public bool RemoteGPIO { get; set; }
        [DataMember]
        public bool Serial { get; set; }
        [DataMember]
        public bool SPI { get; set; }
        [DataMember]
        public bool SSH { get; set; }
        [DataMember]
        public bool VNC { get; set; }
    }
}
