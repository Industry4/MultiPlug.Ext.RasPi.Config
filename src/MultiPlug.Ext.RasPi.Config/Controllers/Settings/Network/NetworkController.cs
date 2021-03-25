using System;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components.Network;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings.SharedRazor;

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

        public Response Post(NetworkProperties theModel)
        {
            Core.Instance.Network.UpdateProperties(theModel);

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = new Uri(Context.Request.AbsoluteUri)
            };
        }
    }
}
