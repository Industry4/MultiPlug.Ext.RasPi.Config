using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Home
{
    [Route("")]
    public class HomeController : SettingsApp
    {
        public Response Get(string action)
        {
            var Model = Core.Instance.Overview.RepopulateAndGetProperties();

            if( ! Model.RunningRaspberryPi )
            {
                return new Response
                {
                    Model = new SharedProperties(),
                    Template = "RaspPiConfig_Settings_NotRaspberryPi"
                };
            }
            else
            {
                return new Response
                {
                    Model = Core.Instance.Overview.RepopulateAndGetProperties(),
                    Template = "RaspPiConfig_Settings_Home"
                };
            }
        }
    }
}
