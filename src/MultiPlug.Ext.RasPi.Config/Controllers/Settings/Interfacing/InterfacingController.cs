using System;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components.Interfacing;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings.SharedRazor;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Interfacing
{
    [Route("interfacing")]
    public class InterfacingController : SettingsApp
    {
        public Response Get()
        {
            InterfacingProperties Properties = Core.Instance.Interfacing.RepopulateAndGetProperties();

            if (Utils.Hardware.isRunningRaspberryPi)
            {
                return new Response
                {
                    Model = Properties,
                    Template = Templates.Interfacing
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

        public Response Post(InterfacingProperties theModel)
        {
            Core.Instance.Interfacing.UpdateProperties(theModel);

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = new Uri(Context.Request.AbsoluteUri)
            };
        }
    }
}
