
namespace MultiPlug.Ext.RasPi.Config.Diagnostics
{
    internal enum EventLogEntryCodes
    {
        Reserved = 0,
        SourceHome = 1,
        SourceUser = 2,
        SourceActions = 3,
        SourceBoot = 4,
        SourceHAT = 5,
        SourceInterfacing = 6,
        SourceLocalisation = 7,
        SourceMemory = 8,
        SourceNetwork = 9,
        SourceDebug = 10,
        SourcePerformance = 11,

        RunningRaspberryPiFalse = 26,
        SystemShutdown = 27,
        HostNameChanging = 28,
        HostNameChanged = 29,
        HostNameChangeError = 30,
        SSIDChanging = 31,
        SSIDChanged = 32,
        SSIDChangeError = 33,
        NICIP4StaticChanging = 34,
        NICIP4DynamicChanging = 35,
        NICIP6StaticChanging = 36,
        NICIP6DynamicChanging = 37,
        NICRoutersChanging = 38,
        NICDNSChanging = 39,
        NICIPChangesComplete = 40,
        NICIPChangesError = 41,

        TimesyncdEnabling = 42,
        TimesyncdEnablingError = 43,
        TimesyncdEnabled = 44,

        TimesyncdStarting = 45,
        TimesyncdStartingError = 46,
        TimesyncdStarted = 47,

        TimesyncdStopping = 48,
        TimesyncdStoppingError = 49,
        TimesyncdStopped = 50,

        TimesyncdDisabling = 51,
        TimesyncdDisablingError = 52,
        TimesyncdDisabled = 53,

        FakeHwClockEnabling = 54,
        FakeHwClockEnablingError = 55,
        FakeHwClockEnabled = 56,

        FakeHwClockStarting = 57,
        FakeHwClockStartingError = 58,
        FakeHwClockStarted = 59,

        FakeHwClockStopping = 60,
        FakeHwClockStoppingError = 61,
        FakeHwClockStopped = 62,

        FakeHwClockDisabling = 63,
        FakeHwClockDisablingError = 64,
        FakeHwClockDisabled = 65,

        TimeZoneSetting = 66,
        TimeZoneSettingError = 67,
        TimeZoneSet = 68,

        WifiCountrySetting = 69,
        WifiCountrySettingError = 70,
        WifiCountrySet = 71,

        DateSetting = 72,
        DateSettingError = 73,
        DateSet = 74,

        TimeSetting = 75,
        TimeSettingError = 76,
        TimeSet = 77,

        NetworkWaitSettingTrue = 78,
        NetworkWaitSettingFalse = 79,
        NetworkWaitSettingError = 80,
        NetworkWaitSet = 81,

        SplashScreenSettingTrue = 82,
        SplashScreenSettingFalse = 83,
        SplashScreenSettingError = 84,
        SplashScreenSet = 85,

        BootBehaviourSettingConsole = 86,
        BootBehaviourSettingConsoleAutologin = 87,
        BootBehaviourSettingDesktop = 88,
        BootBehaviourSettingDesktopAutologin = 89,
        BootBehaviourSettingError = 90,
        BootBehaviourSet = 91,

        BootOrderSettingUSB = 92,
        BootOrderSettingNetwork = 93,
        BootOrderSettingError = 94,
        BootOrderSet = 95,

        BootROMSettingLatest = 96,
        BootROMSettingDefault = 97,
        BootROMSettingError = 98,
        BootROMSet = 99,

        CameraSettingEnabling = 100,
        CameraSettingDisabling = 101,
        CameraSettingError = 102,
        CameraSettingEnabled = 103,
        CameraSettingDisabled = 104,

        SSHSettingEnabling = 105,
        SSHSettingDisabling = 106,
        SSHSettingError = 107,
        SSHSettingEnabled = 108,
        SSHSettingDisabled = 109,

        VNCSettingEnabling = 110,
        VNCSettingDisabling = 111,
        VNCSettingError = 112,
        VNCSettingEnabled = 113,
        VNCSettingDisabled = 114,

        SPISettingEnabling = 115,
        SPISettingDisabling = 116,
        SPISettingError = 117,
        SPISettingEnabled = 118,
        SPISettingDisabled = 119,

        I2CSettingEnabling = 120,
        I2CSettingDisabling = 121,
        I2CSettingError = 122,
        I2CSettingEnabled = 123,
        I2CSettingDisabled = 124,

        SerialSettingEnabling = 125,
        SerialSettingDisabling = 126,
        SerialSettingError = 127,
        SerialSettingEnabled = 128,
        SerialSettingDisabled = 129,

        OneWireSettingEnabling = 130,
        OneWireSettingDisabling = 131,
        OneWireSettingError = 132,
        OneWireSettingEnabled = 133,
        OneWireSettingDisabled = 134,

        RemoteGPIOSettingEnabling = 135,
        RemoteGPIOSettingDisabling = 136,
        RemoteGPIOSettingError = 137,
        RemoteGPIOSettingEnabled = 138,
        RemoteGPIOSettingDisabled = 139,

        CameraSettingGetError = 140,
        SSHSettingGetError = 141,
        VNCSettingGetError = 142,
        SPISettingGetError = 143,
        I2CSettingGetError = 144,
        SerialSettingGetError = 145,
        OneWireSettingGetError = 146,
        RemoteGPIOSettingGetError = 147,

        HATSettingsGetProductError = 148,
        HATSettingsGetProductIdError = 149,
        HATSettingsGetProductVersionError = 150,
        HATSettingsGetUUIDError = 151,
        HATSettingsGetVendorError = 152,

        NetworkWaitSettingsGetError = 153,
        //SplashScreenSettingsGetError = 154,

        HostNameSettingsGetError = 155,
        //WiFiCountrySettingsGetError = 156,
        SSIDSettingsGetError = 157,
        NICInterfacesGetError = 158,

        RaspberryPiModelSettingGetError = 159,
        DebianVersionSettingGetError = 160,
        DateTimeSettingGetError = 161,
        GPUTemperatureSettingGetError = 162,
        CPUTemperatureSettingGetError = 163,
        DiskFreeSettingGetError = 164,

        WiFiCountriesSettingGetError = 165,
        WiFiCountrySettingGetError = 166,
        TimeZonesSettingGetError = 167,
        TimeZoneSettingGetError = 168,
        DateSettingGetError = 169,
        TimeSettingGetError = 170,
        TimeSyncdEnabledSettingGetError = 171,
        FakeHWClockEnabledSettingGetError = 172,
        RAMFreeSettingGetError = 173,

        RTCSyncing = 174,
        RTCSyncError = 175,
        RTCSynced = 176,
        DebugWriteLine = 177,


        PerformanceFanEnabled = 180,
        PerformanceFanEnabledError = 181,
        PerformanceFanDisabled = 182,
        PerformanceFanDisabledError = 183,
        PerformanceFanGPIOOrTempChanged = 184,
        PerformanceFanGPIOOrTempChangedError = 185,

        BootOrderSettingSDCard = 186,
        ExpandRootFsSet = 187,
        ExpandRootFsError = 188
    }
}
