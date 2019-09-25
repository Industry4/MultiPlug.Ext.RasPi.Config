using System;
using System.Collections.Generic;
using MultiPlug.Base.Http;
using MultiPlug.Extension.Core.Views;

namespace MultiPlug.Ext.RasPi.Config.Controllers.Assets
{
    class AssetsEndpoint : ViewBase
    {
        private Controller[] m_Controllers = new Controller[] { new ImageController() };

        public override IEnumerable<Controller> Controllers
        {
            get
            {
                return m_Controllers;
            }
        }

        public override Guid Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool isPartial
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override ViewType Type
        {
            get
            {
                return ViewType.Assets;
            }
        }
    }
}
