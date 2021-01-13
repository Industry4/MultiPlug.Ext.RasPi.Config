using System;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components.Memory;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Memory
{
    [Route("memory")]
    public class MemoryController : SettingsApp
    {
        public Response Get()
        {
            MemoryProperties Properties = Core.Instance.Memory.RepopulateAndGetProperties();

            if (Properties.RunningRaspberryPi)
            {
                return new Response
                {
                    Model = Properties,
                    Template = "RaspPiConfig_Settings_Memory"
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
