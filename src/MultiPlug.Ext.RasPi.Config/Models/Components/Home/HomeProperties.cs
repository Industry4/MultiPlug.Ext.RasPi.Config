using System.Runtime.Serialization;

namespace MultiPlug.Ext.RasPi.Config.Models.Components.Home
{
    public class HomeProperties : SharedProperties
    {
        [DataMember]
        public string RaspberryPiModel { get; set; }
        [DataMember]
        public string OSVersion { get; set; }
        public string HostName { get; set; }
        public string Date { get; set; }
        [DataMember]
        public string GPUTemperature { get; set; }
        public string CPUTemperature { get; set; }
        [DataMember]
        public string FreeDiskPercentage { get; set; }
    }
}
