using System.Collections.Generic;

using MultiPlug.Base.Http;
using MultiPlug.Extension.Core.Http;
using MultiPlug.Extension.Core.Attribute;

using MultiPlug.Ext.RasPi.Config.Controllers.Settings.Home;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings
{
    [HttpEndpointType(HttpEndpointType.Settings)]
    [ViewAs(ViewAs.Partial)]
    class SettingsApp : HttpEndpoint
    {
        readonly Controller[] m_Controllers = { new HomeController() };

        public override IEnumerable<Controller> Controllers
        {
            get
            {
                return m_Controllers;
            }
        }
    }
}
