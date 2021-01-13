using System.Runtime.Serialization;

namespace MultiPlug.Ext.RasPi.Config.Models.Components.Localisation
{
    public class LocalisationProperties : SharedProperties
    {
        public string[][] WifiCountries { get; set; }
        [DataMember]
        public string WifiCountry { get; set; }
        [DataMember]
        public string Date { get; set; }
        [DataMember]
        public string Time { get; set; }
        [DataMember]
        public bool TimeSyncdEnabled { get; set; }
        [DataMember]
        public bool FakeHWClockEnabled { get; set; }
        [DataMember]
        public string TimeZone { get; set; }
        public string[][] TimeZones { get; set; }
        public bool SetTime { get; set; } = false;
        public bool SetDate { get; set; } = false;
    }
}
