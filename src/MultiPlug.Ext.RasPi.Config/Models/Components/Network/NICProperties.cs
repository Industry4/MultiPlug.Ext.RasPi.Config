
namespace MultiPlug.Ext.RasPi.Config.Models.Components.Network
{
    public class NICProperties
    {
        readonly string m_Id;

        public NICProperties(string theId)
        {
            m_Id = theId;
        }

        public string Id { get { return m_Id; } }
        public string IPAddress { get; set; }
        public string IP6Address { get; set; }
        public string[] Routers { get; set; }
        public string[] DomainNameServers { get; set; }
    }
}
