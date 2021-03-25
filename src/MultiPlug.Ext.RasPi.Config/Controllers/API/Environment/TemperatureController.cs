using System.Threading.Tasks;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Components.Home;
using MultiPlug.Ext.RasPi.Config.Models.Components.Home.API;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;

namespace MultiPlug.Ext.RasPi.Config.Controllers.API.Environment
{
    [Route("environment/temperatures/")]
    public class TemperatureController : APIEndpoint
    {
        public Response Get()
        {
            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[2];

            Tasks[0] = HomeComponent.GetGPUTemperature();
            Tasks[1] = HomeComponent.GetCPUTemperature();

            Task.WaitAll(Tasks);

            return new Response
            {
                MediaType = "application/json",
                Model = new Temperatures
                {
                    GPU = HomeComponent.ProcessGPUTemperature(Tasks[0]),
                    CPU = HomeComponent.ProcessCPUTemperature(Tasks[1])
                }
            };
        }
    }
}
