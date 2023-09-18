using MultiPlug.Ext.RasPi.Config.Models.Components.Network;
using System.IO;

namespace MultiPlug.Ext.RasPi.Config.Components.Network
{
    internal static class InterfacesD
    {
        internal const string FileLocation = "/etc/network/interfaces.d/";

        public static void Write(NICProperties theProperties)
        {
            string FullPath = Path.Combine(FileLocation, theProperties.Id);


            using (StreamWriter writer = File.CreateText(FullPath))
            {
                if (string.IsNullOrEmpty(theProperties.IPAddress))
                {
                    writer.Write("allow-hotplug ");
                    writer.Write(theProperties.Id);
                    writer.Write("\n");

                    writer.Write("iface ");
                    writer.Write(theProperties.Id);
                    writer.Write(" inet dhcp");
                    writer.Write("\n");
                }
                else
                {
                    writer.Write("allow-hotplug ");
                    writer.Write(theProperties.Id);
                    writer.Write("\n");

                    writer.Write("iface ");
                    writer.Write(theProperties.Id);
                    writer.Write(" inet static");
                    writer.Write("\n");

                    writer.Write("    address ");
                    writer.Write(theProperties.IPAddress);
                    writer.Write("\n");

                    if( theProperties.Routers != null && theProperties.Routers.Length > 0 )
                    {
                        writer.Write("    gateway ");
                        writer.Write(theProperties.Routers[0]);
                        writer.Write("\n");
                    }

                    if (theProperties.DomainNameServers != null && theProperties.DomainNameServers.Length > 0)
                    {
                        writer.Write("    dns-nameservers ");
                        writer.Write( string.Join( " ", theProperties.DomainNameServers));
                        writer.Write("\n");
                    }
                }
            }
        }
    }
}
