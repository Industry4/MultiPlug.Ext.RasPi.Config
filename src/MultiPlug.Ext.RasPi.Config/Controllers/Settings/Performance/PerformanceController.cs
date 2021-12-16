using System;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings.SharedRazor;
using MultiPlug.Ext.RasPi.Config.Models.Components.Performance;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Performance
{
    [Route("Performance")]
    public class PerformanceController : SettingsApp
    {
        public Response Get()
        {
            PerformanceProperties Properties = Core.Instance.Performance.RepopulateAndGetProperties();

            if (Utils.Hardware.isRunningRaspberryPi)
            {
                return new Response
                {
                    Model = Properties,
                    Template = Templates.Performance
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

        public Response Post(PerformanceProperties theModel)
        {
            Core.Instance.Performance.UpdateProperties(theModel);

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = new Uri(Context.Request.AbsoluteUri)
            };
        }
    }
}
