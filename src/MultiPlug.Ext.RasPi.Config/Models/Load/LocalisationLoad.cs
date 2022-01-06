using System.Runtime.Serialization;

namespace MultiPlug.Ext.RasPi.Config.Models.Load
{
    public class LocalisationLoad
    {
        [DataMember]
        public bool WiFiCountrySyncEnabled { get; set; }
    }
}
