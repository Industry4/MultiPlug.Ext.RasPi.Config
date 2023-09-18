# MultiPlug.Ext.RasPi.Config

Raspberry Pi Configuration Extension for the [MultiPlug .Net Edge Computing Platform](https://www.multiplug.app).

### Functionality

The functionality mirrors most of the functionality found in the command line tool [raspi-config](https://www.raspberrypi.org/documentation/configuration/raspi-config.md). Due to MultiPlug's browser based interface, visual setting haven't been included.

* General overview - Raspberry Pi Model - OS Version, Hostname, Date and Time, Temperature and Disk Space.
* Network - Hostname, static values for IP Address, IP 6 Address, Routers and Domain Name Servers for all Eth and Wlan, Wireless SSID and Passphrase.
* Localisation - Wifi Country, Time, Time Zone, Date
* Interfacing - Camera, SSH, VNC, SPI, I2C, Serial, 1-Wire, Remote GPIO
* Performance - Temperature control via Fan
* Boot - Desktop or CLI, Boot Order, Boot BOM Version
* Memory - Current Disk and RAM usage
* Users - Set user passwords
* Hardware Attached On Top (HAT) Details
* REST Json API for Temperature and Disk Space

## Getting Started

These instructions will guide you the installation of the Extension on an instance of the MultiPlug .Net Edge Computing Platform.

### Installing

See the Prerequisites at the end of this document to install MultiPlug.

#### Option 1 - Online

The Extension can be installed using the in-built MultiPlug installer located at [http://multiplug.local/settings/add/](http://multiplug.local/settings/add/)
 *Replace multiplug.local for the IP Address of the MultiPlug instance* you must have an connection to the internet to do this, see Option 2 if you don't.
 
#### Option 2 - Offline

* Download the package from Nuget [https://www.nuget.org/api/v2/package/MultiPlug.Ext.RasPi.Config/](https://www.nuget.org/api/v2/package/MultiPlug.Ext.RasPi.Config/)
* Sideload the extension using the option toward the bottom of the Add Extension page in MultiPlug located at [http://multiplug.local/settings/add/](http://multiplug.local/settings/add/)
 *Replace multiplug.local for the IP Address of the MultiPlug instance*


## Runtime
### Screenshot

![Image of MultiPlug.Ext.RasPi.Config](https://raw.githubusercontent.com/Industry4/MultiPlug.Ext.RasPi.Config/master/media/screen-shot1.png)

More Screenshots can be viewed on [the Wiki](https://github.com/Industry4/MultiPlug.Ext.RasPi.Config/wiki)

### Application

The Extension can be accessed from: [http://multiplug.local/extensions/multiplug.ext.raspi.config//](http://multiplug.local/extensions/multiplug.ext.raspi.config/)
 *Replace multiplug.local for the IP Address of the MultiPlug instance*
 
### Known Bugs
* None

### Prerequisites - Installing MultiPlug

The MultiPlug .Net Edge Computing Platform must be installed on Raspberry Pi OS.

#### Option 1 - Quick Start

Download multiplug.deb from [https://apt.multiplug.app/multiplug.deb](https://apt.multiplug.app/multiplug.deb) using the command:

```
wget https://apt.multiplug.app/multiplug.deb
```

Start the installation by using the command:

```
sudo apt-get install -f ./multiplug.deb
```

#### Option 2 - Install to get updates

This option will update MultiPlug during each [system update detailed here](https://www.raspberrypi.org/documentation/raspbian/updating.md).

Add the following line to /etc/apt/sources.list

```
deb [trusted=yes] https://apt.multiplug.app ./
```
Run the following command:
```
sudo apt update
```
To install MultiPlug run the following command:
```
sudo apt-get install multiplug
```

## MultiPlug - Further reading

* [https://www.multiplug.app](https://www.multiplug.app)
* [MultiPlug Wiki](https://github.com/British-Systems/MultiPlug/wiki)

## Authors

* **David Graham** - *Initial work* - [4IR British Systems](https://4ir.uk)

## License

This project is licensed under the MIT License
## Acknowledgments

* Uses calls to raspi-config shell commands using undocumented nonint feature.
* Uses Process Runner from Unosquare Swan (Stuff we all need).

Thanks for the support from:
* Seemin Suleri
* Julian Singh
* Ian Rathbone
* Julius Angwenyi
* Brainboxes Ltd
