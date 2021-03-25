using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings.Network
{
    [Route("network/deletewifi")]
    public class DeleteWiFiController : SettingsApp
    {
        public Response Post(string id)
        {
            if (Core.Instance.Network.DeleteWiFi(id))
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            else
            {
                return new Response
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound
                };
            }
        }
    }
}
