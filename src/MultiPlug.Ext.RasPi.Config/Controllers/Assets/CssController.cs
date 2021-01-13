using System.Text;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Properties;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Assets
{

    [Route("css/*")]
    public class CssController : AssetsEndpoint
    {
        public Response Get(string theName)
        {
            string Result = string.Empty;

            switch (theName)
            {
                case "bootstrap-timepicker.min.css":
                    Result = Resources.bootstrap_timepicker_min_css;
                    break;

                case "datepicker.css":
                    Result = Resources.datepicker_css;
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
                return new Response { MediaType = "text/css", RawBytes = Encoding.ASCII.GetBytes(Result) };
            }
        }
    }
}
