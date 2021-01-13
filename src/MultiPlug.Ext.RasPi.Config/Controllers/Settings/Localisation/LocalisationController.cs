using System;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Models.Components.Localisation;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Localisation
{
    [Route("localisation")]
    public class LocalisationController : SettingsApp
    {
        public Response Get()
        {
            LocalisationProperties Properties = Core.Instance.Localisation.RepopulateAndGetProperties();

            if (Properties.RunningRaspberryPi)
            {
                return new Response
                {
                    Model = Properties,
                    Template = "RaspPiConfig_Settings_Localisation"
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

        public Response Post(LocalisationProperties theModel)
        {
            Core.Instance.Localisation.UpdateProperties(theModel);

            return new Response
            {
                StatusCode = System.Net.HttpStatusCode.Moved,
                Location = new Uri(Context.Request.AbsoluteUri)
            };
        }
    }
}
