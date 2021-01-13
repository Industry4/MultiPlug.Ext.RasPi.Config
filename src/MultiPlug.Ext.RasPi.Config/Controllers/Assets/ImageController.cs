using System.IO;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Properties;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Assets
{
    [Route("images/*")]
    public class ImageController : AssetsEndpoint
    {
        public Response Get(string theName)
        {
            using (var stream = new MemoryStream())
            {
                Resources.raspberry_pi.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return new Response { RawBytes = stream.ToArray(), MediaType = "image/png" };
            }
        }
    }
}
