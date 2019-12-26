using MultiPlug.Extension.Core;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings;
using MultiPlug.Ext.RasPi.Config.Properties;
using MultiPlug.Ext.RasPi.Config.Controllers.Assets;
using MultiPlug.Extension.Core.Http;

namespace MultiPlug.Ext.RasPi.Config
{
    public class RasPiConfig : MultiPlugExtension
    {
        private HttpEndpoint[] m_Apps = new HttpEndpoint[]
        {
            new SettingsApp(),
            new AssetsEndpoint()
        };

        public override HttpEndpoint[] HttpEndpoints
        {
            get
            {
                return m_Apps;
            }
        }

        public override RazorTemplate[] RazorTemplates
        {
            get
            {
                return new RazorTemplate[]
                {
                    new RazorTemplate("RaspPiConfig_Settings_Home", Resources.SettingsHome)
                };
           }
        }
    }
}
