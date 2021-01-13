using MultiPlug.Extension.Core;
using MultiPlug.Ext.RasPi.Config.Properties;
using MultiPlug.Extension.Core.Http;

namespace MultiPlug.Ext.RasPi.Config
{
    public class RasPiConfig : MultiPlugExtension
    {
        public RasPiConfig()
        {
                Core.Instance.Init(MultiPlugActions, MultiPlugServices);
        }


        public override RazorTemplate[] RazorTemplates
        {
            get
            {
                return new RazorTemplate[]
                {
                    new RazorTemplate("RaspPiConfig_Settings_Navigation", Resources.SettingsNavigation),
                    new RazorTemplate("RaspPiConfig_Settings_NotRaspberryPi", Resources.SettingsNotRaspberryPi),
                    new RazorTemplate("RaspPiConfig_Settings_Home", Resources.SettingsHome),
                    new RazorTemplate("RaspPiConfig_Settings_Network", Resources.SettingsNetwork),
                    new RazorTemplate("RaspPiConfig_Settings_Hat", Resources.SettingsHat),
                    new RazorTemplate("RaspPiConfig_Settings_Localisation", Resources.SettingsLocalisation),
                    new RazorTemplate("RaspPiConfig_Settings_Interfacing", Resources.SettingsInterfacing),
                    new RazorTemplate("RaspPiConfig_Settings_Boot", Resources.SettingsBoot),
                    new RazorTemplate("RaspPiConfig_Settings_Memory", Resources.SettingsMemory),
                    new RazorTemplate("RaspPiConfig_Settings_About", Resources.SettingsAbout)
                };
           }
        }

        public override object Save()
        {
            return Core.Instance;
        }
    }
}
