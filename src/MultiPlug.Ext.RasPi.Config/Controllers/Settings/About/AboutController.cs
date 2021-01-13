using System.Reflection;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.About
{
    [Route("about")]
    public class AboutController : SettingsApp
    {
        public Response Get()
        {
            var ExecutingAssembly = Assembly.GetExecutingAssembly();

            return new Response
            {
                Template = "RaspPiConfig_Settings_About",
                Model = Core.Instance.About.RepopulateAndGetProperties()
            };
        }
    }
}