
namespace MultiPlug.Ext.RasPi.Config.Models.ConfigFiles.DHCPCD
{
    public class ReadResult
    {
        public DHCPcdConfLine[] ConfigLines { get; set; } = new DHCPcdConfLine[0];
        public NICProperties[] Properties { get; set; } = new NICProperties[0];
    }
}
