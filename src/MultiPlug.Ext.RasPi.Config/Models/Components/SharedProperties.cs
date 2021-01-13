using MultiPlug.Base;

namespace MultiPlug.Ext.RasPi.Config.Models.Components
{
    public class SharedProperties : MultiPlugBase, IRunningRaspberryPiProperty
    {
        public bool RebootUserPrompt { get; set; } = false;
        public bool RunningRaspberryPi { get; set; } = false;
    }
}
