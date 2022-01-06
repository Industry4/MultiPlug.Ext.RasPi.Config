using System.Runtime.Serialization;

using MultiPlug.Base;
using MultiPlug.Base.Exchange;
using MultiPlug.Base.Exchange.API;
using MultiPlug.Extension.Core;

using MultiPlug.Ext.RasPi.Config.Components.Home;
using MultiPlug.Ext.RasPi.Config.Components.Network;
using MultiPlug.Ext.RasPi.Config.Components.Hat;
using MultiPlug.Ext.RasPi.Config.Components.Interfacing;
using MultiPlug.Ext.RasPi.Config.Components.Localisation;
using MultiPlug.Ext.RasPi.Config.Components.Boot;
using MultiPlug.Ext.RasPi.Config.Components.Memory;
using MultiPlug.Ext.RasPi.Config.Components.Actions;
using MultiPlug.Ext.RasPi.Config.Components.Performance;
using MultiPlug.Ext.RasPi.Config.Components.About;

using MultiPlug.Ext.RasPi.Config.Diagnostics;
using MultiPlug.Ext.RasPi.Config.Utils;
using MultiPlug.Ext.RasPi.Config.Models.Load;

namespace MultiPlug.Ext.RasPi.Config
{
    public class Core : MultiPlugBase
    {
        private static Core m_Instance = null;

        private ILoggingService m_LoggingService;
        private IMultiPlugActions m_MultiPlugActions;
        internal IMultiPlugAPI MultiPlugAPI { get; private set; }

        public static Core Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new Core();
                }
                return m_Instance;
            }
        }
        internal void Init(IMultiPlugActions theMultiPlugActions, IMultiPlugServices theMultiPlugServices, IMultiPlugAPI theMultiPlugAPI)
        {
            m_MultiPlugActions = theMultiPlugActions;
            MultiPlugAPI = theMultiPlugAPI;

            Localisation  = new LocalisationComponent(theMultiPlugAPI);

            theMultiPlugServices.Logging.RegisterDefinitions(EventLogDefinitions.DefinitionsId, EventLogDefinitions.Definitions, true);

            m_LoggingService = theMultiPlugServices.Logging.New("RasPiConfig", Diagnostics.EventLogDefinitions.DefinitionsId);

            About = new AboutComponent(m_LoggingService);

            Hardware.CheckRunningRaspberryPi();

            Overview.Log += OnLogWriteEntry;
            Network.Log += OnLogWriteEntry;
            Hat.Log += OnLogWriteEntry;
            Interfacing.Log += OnLogWriteEntry;
            Localisation.Log += OnLogWriteEntry;
            Performance.Log += OnLogWriteEntry;
            Boot.Log += OnLogWriteEntry;
            Memory.Log += OnLogWriteEntry;
            Actions.Log += OnLogWriteEntry;

            Network.RestartDue += OnRestartDue;
            Interfacing.RestartDue += OnRestartDue;
            Localisation.RestartDue += OnRestartDue;
            Boot.RestartDue += OnRestartDue;

            Actions.DoSystemRestart += OnDoSystemRestart;
        }

        internal void Start()
        {
            Localisation.StartServices();
        }

        internal void Load(Root config)
        {
            if(config.Localisation != null)
            {
                Localisation.UpdateProperties(config.Localisation);
            }
        }

        private void OnDoSystemRestart()
        {
            m_MultiPlugActions.System.Power.Restart();
        }

        private void OnRestartDue()
        {
            Hardware.SetRebootUserPrompt();
        }

        private void OnLogWriteEntry(EventLogEntryCodes theLogCode, string[] theArg)
        {
            m_LoggingService.WriteEntry((uint)theLogCode, theArg);
        }

        [DataMember]
        public HomeComponent Overview { get; private set; } = new HomeComponent();
        [DataMember]
        public NetworkComponent Network { get; private set; } = new NetworkComponent();
        [DataMember]
        public HatComponent Hat { get; private set; } = new HatComponent();
        [DataMember]
        public InterfacingComponent Interfacing { get; private set; } = new InterfacingComponent();
        internal PerformanceComponent Performance { get; private set; } = new PerformanceComponent();
        [DataMember]
        public LocalisationComponent Localisation { get; private set; }
        [DataMember] 
        public BootComponent Boot { get; private set; } = new BootComponent();
        [DataMember]
        public MemoryComponent Memory { get; private set; } = new MemoryComponent();
        internal ActionsComponent Actions { get; private set; } = new ActionsComponent();
        internal AboutComponent About { get; private set; }
    }
}
