using System;
using MultiPlug.Extension.Core;
using MultiPlug.Base.Exchange;
using MultiPlug.Extension.Core.Collections;
using MultiPlug.Extension.Core.Views;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings;
using MultiPlug.Ext.RasPi.Config.Properties;
using MultiPlug.Ext.RasPi.Config.Controllers.Assets;

namespace MultiPlug.Ext.RasPi.Config
{
    public class RasPiConfig : MultiPlugExtension
    {
        private ViewBase[] m_Apps = new ViewBase[]
        {
            new SettingsApp(),
            new AssetsEndpoint()
        };

        public override ViewBase[] Apps
        {
            get
            {
                return m_Apps;
            }
        }

        public override Event[] Events
        {
            get
            {
                throw new NotImplementedException();
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

        public override Subscription[] Subscriptions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override event EventHandler<ViewBase[]> AppsUpdated;
        public override event EventHandler<Event[]> EventsUpdated;
        public override event EventHandler<RazorTemplate[]> NewRazorTemplates;
        public override event EventHandler<Subscription[]> SubscriptionsUpdated;

        public override void Initialise()
        {
            throw new NotImplementedException();
        }

        public override void Load(KeyValuesJson[] config)
        {
            throw new NotImplementedException();
        }

        public override void OnUnhandledException(UnhandledExceptionEventArgs args)
        {
            throw new NotImplementedException();
        }

        public override object Save()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
