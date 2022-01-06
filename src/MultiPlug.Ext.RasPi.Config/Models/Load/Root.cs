using System.Runtime.Serialization;

namespace MultiPlug.Ext.RasPi.Config.Models.Load
{
    public class Root
    {
        [DataMember]
        public LocalisationLoad Localisation { get; set; }
    }
}
