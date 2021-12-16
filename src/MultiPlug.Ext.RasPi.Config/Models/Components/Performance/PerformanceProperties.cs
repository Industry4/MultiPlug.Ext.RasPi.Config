
namespace MultiPlug.Ext.RasPi.Config.Models.Components.Performance
{
    public class PerformanceProperties : SharedProperties
    {
        public bool FanEnabled { get; set; }
        public string FanTemperature { get; set; }
        public string FanGPIO { get; set; }

        public int FanTemperatureMin = 60;
        public int FanTemperatureMax = 120;

        public int GPIOMin = 2;
        public int GPIOMax = 27;

    }
}
