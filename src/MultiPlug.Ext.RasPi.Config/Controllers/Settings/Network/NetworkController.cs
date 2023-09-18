using System;
using System.Collections.Generic;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components.Network;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings.SharedRazor;
using MultiPlug.Ext.RasPi.Config.Models.Settings.Network;
using System.Linq;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Home
{
    [Route("network")]
    public class NetworkController : SettingsApp
    {
        public Response Get()
        {
            NetworkProperties Properties = Core.Instance.Network.RepopulateAndGetProperties();

            if (Utils.Hardware.isRunningRaspberryPi)
            {
                var Search = Properties.Interfaces.FirstOrDefault(Interface => Interface.Inet == this.Context.Request.Host);

                if(Search != null)
                {
                    Properties.NICInUse = Search.Name;
                }
                else
                {
                    Properties.NICInUse = string.Empty;
                }

                return new Response
                {
                    Model = Properties,
                    Template = Templates.Network
                };
            }
            else
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.Redirect,
                    Location = new Uri(Context.Paths.Base + Context.Paths.Home)
                };
            }
        }

        private NICProperties[] PopulateNICProperties(string[] theNICId, string[] theIPAddress, string[] theIPAddressCidr, string[] theIP6Address, string[] theIP6AddressCidr, string[] theRouterNICId, string[] theRouter, string[] theDomainNameServerNICId, string[] theDomainNameServer)
        {
            List<NICProperties> NICPropertiesList = new List<NICProperties>();

            for (int i = 0; i < theNICId.Length; i++)
            {
                NICProperties NICProperties = new NICProperties(theNICId[i]);

                if( ! string.IsNullOrEmpty( theIPAddress[i] ) )
                {
                    NICProperties.IPAddress = theIPAddress[i] + "/" + theIPAddressCidr[i];
                }
                else
                {
                    NICProperties.IPAddress = string.Empty;
                }

                if ( ! string.IsNullOrEmpty( theIP6Address[i] ) )
                {
                    NICProperties.IP6Address = theIP6Address[i] + "/" + theIP6AddressCidr[i];
                }
                else
                {
                    NICProperties.IP6Address = string.Empty;
                }

                List<string> Routers = new List<string>();

                if (theRouterNICId != null)
                {
                    for (int j = 0; j < theRouterNICId.Length; j++)
                    {
                        if (theRouterNICId[j] == theNICId[i])
                        {
                            Routers.Add(theRouter[j]);
                        }
                    }
                }

                NICProperties.Routers = Routers.ToArray();

                List<string> DomainNameServers = new List<string>();

                if (theDomainNameServerNICId != null)
                {
                    for (int j = 0; j < theDomainNameServerNICId.Length; j++)
                    {
                        if (theDomainNameServerNICId[j] == theNICId[i])
                        {
                            DomainNameServers.Add(theDomainNameServer[j]);
                        }
                    }
                }

                NICProperties.DomainNameServers = DomainNameServers.ToArray();

                NICPropertiesList.Add(NICProperties);
            }

            return NICPropertiesList.ToArray();
        }


        public Response Post(PostModel theModel)
        {
            Core.Instance.Network.UpdateProperties(new NetworkProperties
            {
                HostName = theModel.HostName,
                NewSSID = theModel.NewSSID,
                NewPassphrase = theModel.NewPassphrase,
                Eths = PopulateNICProperties(theModel.NICId, theModel.IPAddress, theModel.IPAddressCidr, theModel.IP6Address, theModel.IP6AddressCidr, theModel.RouterNICId, theModel.Router, theModel.DomainNameServerNICId, theModel.DomainNameServer),
                Wlans = PopulateNICProperties(theModel.WlanNICId, theModel.WlanIPAddress, theModel.WlanIPAddressCidr, theModel.WlanIP6Address, theModel.WlanIP6AddressCidr, theModel.WlanRouterNICId, theModel.WlanRouter, theModel.WlanDomainNameServerNICId, theModel.WlanDomainNameServer)
            });

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = new Uri(Context.Request.AbsoluteUri)
            };
        }
    }
}
