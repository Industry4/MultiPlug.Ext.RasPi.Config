using System.Text;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Properties;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Assets
{
    [Route("js/*")]
    public class JavaScriptsController : AssetsEndpoint
    {
        public Response Get(string theName)
        {
            string Result = string.Empty;

            switch (theName)
            {
                case "bootstrap-timepicker.js":
                Result = Resources.bootstrap_timepicker_js;
                    break;

                case "bootstrap-datepicker.js":
                    Result = Resources.bootstrap_datepicker_js;
                    break;

                default:
                    //m_Logger.WriteLine("ERROR Javascript missing:               " + theName);
                    break;
            }

            if (string.IsNullOrEmpty(Result))
            {
                return new Response { StatusCode = System.Net.HttpStatusCode.NotFound };
            }
            else
            {
                return new Response { MediaType = "text/javascript", RawBytes = Encoding.ASCII.GetBytes(Result) };
            }
        }
    }
}
