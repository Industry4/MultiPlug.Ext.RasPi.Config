using System.Threading.Tasks;
using MultiPlug.Base.Attribute;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Components.Home;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;

namespace MultiPlug.Ext.RasPi.Config.Controllers.API.Journal
{
        [Route("linux/journal/")]
    public class JournalController : APIEndpoint
    {
        public Response Get( string service, bool thisboot )
        {
            Task<ProcessResult>[] Tasks = new Task<ProcessResult>[1];

            Tasks[0] = HomeComponent.GetJournalEntry( service, thisboot );

            Task.WaitAll(Tasks);

            return new Response
            {
                MediaType = "application/json",
                Model = HomeComponent.ProcessJournal(Tasks[0])
            };
        }
    }
}
