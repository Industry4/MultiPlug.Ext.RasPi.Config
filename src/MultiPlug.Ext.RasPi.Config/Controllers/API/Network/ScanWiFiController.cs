using System;
using System.Linq;
using System.Threading.Tasks;

using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;

namespace MultiPlug.Ext.RasPi.Config.Controllers.API.Network
{
    [Route("network/scan/*")]
    public class ScanWiFiController : APIEndpoint
    {
        public Response Get(string WlanId)
        {
            Task<ProcessResult> ScanTask = ProcessRunner.GetProcessResultAsync("iwlist", WlanId + " scan");

            ScanTask.Wait();

            const string c_SSIDLine = "ESSID:\"";

            string[] SSIDS = ScanTask.Result.Okay() ? ScanTask.Result.GetOutput().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.Contains(c_SSIDLine))
                .Select(line => line.Substring(line.IndexOf(c_SSIDLine) + c_SSIDLine.Length).TrimEnd('"'))
                .ToArray()
                : new string[0];

            return new Response { MediaType = "application/json", Model = SSIDS.Where(x => !string.IsNullOrEmpty(x)).ToArray() };
        }
    }
}
