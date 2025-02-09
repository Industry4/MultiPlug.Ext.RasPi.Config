using System.Runtime.Serialization;

namespace MultiPlug.Ext.RasPi.Config.Models.Components.Localisation
{
    public class LocalisationProperties : SharedProperties
    {
        public WifiCountryModel[] WifiCountries { get; set; }
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
        public string[] TimeZones { get; set; }
        public bool SetTime { get; set; }
        public bool SetDate { get; set; }
        public bool HWClockPresent { get; set; }
        [DataMember]
        public bool WiFiCountrySyncEnabled { get; set; }
        public bool CanChangeWifiCountry { get; set; }
    }
}
