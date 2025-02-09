
namespace MultiPlug.Ext.RasPi.Config.Models.Components.Boot
{
    public class BootProperties : SharedProperties
    {
        public int BootBehaviour { get; set; }
        public bool NetworkWait { get; set; }
        public int SplashScreen { get; set; }
        public int BootOrder { get; set; }
        public int BootROM { get; set; }
    }
}
