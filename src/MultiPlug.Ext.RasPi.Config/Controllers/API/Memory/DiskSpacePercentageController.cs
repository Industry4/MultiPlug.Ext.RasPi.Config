using System.Threading.Tasks;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Components.Home;
using MultiPlug.Ext.RasPi.Config.Models.Components.Home.API;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;

namespace MultiPlug.Ext.RasPi.Config.Controllers.API.Memory
{
    [Route("memory/disk/")]
    public class DiskSpacePercentageController : APIEndpoint
    {
        public Response Get()
        {
            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[1];

            Tasks[0] = HomeComponent.GetDiskSpacePercentage();

            Task.WaitAll(Tasks);

            return new Response
            {
                MediaType = "application/json",
                Model = new DiskSpace
                {
                    DiskSpacePC = HomeComponent.ProcessDiskSpacePercentage(Tasks[0])
                }
            };
        }
    }
}
