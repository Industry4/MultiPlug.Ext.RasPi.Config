using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings.SharedRazor;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Home
{
    [Route("")]
    public class HomeController : SettingsApp
    {
        public Response Get(string action)
        {
            var Model = Core.Instance.Overview.RepopulateAndGetProperties();

            if( !Utils.Hardware.isRunningRaspberryPi)
            {
                return new Response
                {
                    Model = new SharedProperties(),
                    Template = Templates.NotRaspberryPi
                };
            }
            else
            {
                return new Response
                {
                    Model = Core.Instance.Overview.RepopulateAndGetProperties(),
                    Template = Templates.Home
                };
            }
        }
    }
}
