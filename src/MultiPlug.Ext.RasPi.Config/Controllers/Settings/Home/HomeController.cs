using System;
using System.Linq;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Components.ConfigFiles;
using MultiPlug.Ext.RasPi.Config.Models.ConfigFiles.DHCPCD;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Home
{
    [Route("")]
    class HomeController : Controller
    {
        public Response Get()
        {
            ReadResult ReadResult = DHCPCD.Read();

            var Model = new Models.Settings.Home();

            var Eth0 = ReadResult.Properties.FirstOrDefault(nic => nic.Id == "eth0");

            Model.Eth0IPAddress = (Eth0 == null) ? string.Empty : Eth0.IPAddress;
            Model.Eth0IP6Address = (Eth0 == null) ? string.Empty : Eth0.IP6Address;
            Model.Eth0Routers = (Eth0 == null) ? new string[0] : (Eth0.Routers == null) ? new string[0] : Eth0.Routers;
            Model.Eth0DomainNameServers = (Eth0 == null) ? new string[0] : (Eth0.DomainNameServers == null) ? new string[0] : Eth0.DomainNameServers;

            var Wlan0 = ReadResult.Properties.FirstOrDefault(nic => nic.Id == "wlan0");

            Model.Wlan0IPAddress = (Wlan0 == null) ? string.Empty : Wlan0.IPAddress;
            Model.Wlan0IP6Address = (Wlan0 == null) ? string.Empty : Wlan0.IP6Address;
            Model.Wlan0Routers = (Wlan0 == null) ? new string[0] : (Wlan0.Routers == null) ? new string[0] : Wlan0.Routers;
            Model.Wlan0DomainNameServers = (Wlan0 == null) ? new string[0] : (Wlan0.DomainNameServers == null) ? new string[0] : Wlan0.DomainNameServers;

            return new Response
            {
                Model = Model,
                Template = "RaspPiConfig_Settings_Home"
            };
        }

        public Response Post(Models.Settings.Home theModel)
        {
            var eth0 = new NICProperties("eth0");
            eth0.IPAddress = (theModel.Eth0IPAddress != string.Empty )  ? theModel.Eth0IPAddress : null;
            eth0.IP6Address = (theModel.Eth0IP6Address != string.Empty) ? theModel.Eth0IP6Address : null;
            eth0.Routers = theModel.Eth0Routers;
            eth0.DomainNameServers = theModel.Eth0DomainNameServers;

            var wlan0 = new NICProperties("wlan0");
            wlan0.IPAddress = (theModel.Wlan0IPAddress != string.Empty) ? theModel.Wlan0IPAddress : null;
            wlan0.IP6Address = (theModel.Wlan0IP6Address != string.Empty) ? theModel.Wlan0IP6Address : null;
            wlan0.Routers = theModel.Wlan0Routers;
            wlan0.DomainNameServers = theModel.Wlan0DomainNameServers;

            DHCPCD.Update(new NICProperties[] { eth0, wlan0 });

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = new Uri(Context.Request.AbsoluteUri)
            };
        }


    }
}
