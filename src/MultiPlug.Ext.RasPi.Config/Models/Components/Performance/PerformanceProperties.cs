
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

        public bool UndervoltageDetected { get; set; }
        public bool ArmFrequencyCapped { get; set; }
        public bool CurrentlyThrottled { get; set; }
        public bool SoftTemperatureLimitActive { get; set; }
        public bool UndervoltageHasOccurred { get; set; }
        public bool ArmFrequencyCappingHasOccurred { get; set; }
        public bool ThrottlingHasOccurred { get; set; }
        public bool SoftTemperatureLimitHasOccurred { get; set; }

    }
}
