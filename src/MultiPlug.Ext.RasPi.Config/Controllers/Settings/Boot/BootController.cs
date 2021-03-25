using System;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components.Boot;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings.SharedRazor;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Boot
{
    [Route("boot")]
    public class BootController : SettingsApp
    {
        public Response Get()
        {
            BootProperties Properties = Core.Instance.Boot.RepopulateAndGetProperties();

            if(Utils.Hardware.isRunningRaspberryPi)
            {
                return new Response
                {
                    Model = Properties,
                    Template = Templates.Boot
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

        public Response Post(BootProperties theProperties)
        {
            Core.Instance.Boot.UpdateProperties( theProperties );

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = new Uri(Context.Request.AbsoluteUri)
            };
        }
    }
}
