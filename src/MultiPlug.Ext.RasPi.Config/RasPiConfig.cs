﻿using MultiPlug.Extension.Core;
using MultiPlug.Extension.Core.Http;
using MultiPlug.Ext.RasPi.Config.Properties;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings.SharedRazor;
using MultiPlug.Ext.RasPi.Config.Models.Load;

namespace MultiPlug.Ext.RasPi.Config
{
    public class RasPiConfig : MultiPlugExtension
    {
        public RasPiConfig()
        {
                Core.Instance.Init(MultiPlugActions, MultiPlugServices, MultiPlugAPI);
        }

        public override RazorTemplate[] RazorTemplates
        {
            get
            {
                return new RazorTemplate[]
                {
                    new RazorTemplate(Templates.Navigation, Resources.SettingsNavigation),
                    new RazorTemplate(Templates.NotRaspberryPi, Resources.SettingsNotRaspberryPi),
                    new RazorTemplate(Templates.Home, Resources.SettingsHome),
                    new RazorTemplate(Templates.Network, Resources.SettingsNetwork),
                    new RazorTemplate(Templates.Hat, Resources.SettingsHat),
                    new RazorTemplate(Templates.Localisation, Resources.SettingsLocalisation),
                    new RazorTemplate(Templates.Interfacing, Resources.SettingsInterfacing),
                    new RazorTemplate(Templates.Performance, Resources.SettingsPerformance),
                    new RazorTemplate(Templates.Boot, Resources.SettingsBoot),
                    new RazorTemplate(Templates.Memory, Resources.SettingsMemory),
                    new RazorTemplate(Templates.Users, Resources.SettingsUsers),
                    new RazorTemplate(Templates.About, Resources.SettingsAbout)
                };
           }
        }

        public override object Save()
        {
            return Core.Instance;
        }

        public void Load(Root config)
        {
            Core.Instance.Load(config);
        }

        public override void Start()
        {
            Core.Instance.Start();
        }
    }
}
