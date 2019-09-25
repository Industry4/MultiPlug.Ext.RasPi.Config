using System;
using System.Collections.Generic;
using MultiPlug.Base.Http;
using MultiPlug.Ext.RasPi.Config.Controllers.Settings.Home;
using MultiPlug.Extension.Core.Views;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Settings
{
    class SettingsApp : ViewBase
    {
        readonly Guid m_Id = Guid.NewGuid();
        readonly Controller[] m_Controllers = {
            new HomeController(),
        };

        public override Guid Id
        {
            get
            {
                return m_Id;
            }
        }

        public override ViewType Type
        {
            get
            {
                return ViewType.Settings;
            }
        }

        public override bool isPartial
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "RasPi Config";
            }
        }

        public override IEnumerable<Controller> Controllers
        {
            get
            {
                return m_Controllers;
            }
        }
    }
}
