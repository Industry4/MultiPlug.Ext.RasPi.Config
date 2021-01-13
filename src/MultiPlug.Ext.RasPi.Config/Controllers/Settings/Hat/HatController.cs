using System;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components.Hat;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Home
{
    [Route("hat")]
    public class HatController : SettingsApp
    {
        public Response Get()
        {
            HatProperties Properties = Core.Instance.Hat.RepopulateAndGetProperties();

            if (Properties.RunningRaspberryPi)
            {
                return new Response
                {
                    Model = Properties,
                    Template = "RaspPiConfig_Settings_Hat"
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
    }
}
