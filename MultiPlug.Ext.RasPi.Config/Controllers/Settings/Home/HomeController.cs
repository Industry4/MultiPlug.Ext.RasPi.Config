using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Home
{
    [Route("")]
    class HomeController : Controller
    {
        public Response Get()
        {
            return new Response
            {
                Model = new Models.Settings.Home
                {
                },
                Template = "RaspPiConfig_Settings_Home"
            };
        }
    }
}
