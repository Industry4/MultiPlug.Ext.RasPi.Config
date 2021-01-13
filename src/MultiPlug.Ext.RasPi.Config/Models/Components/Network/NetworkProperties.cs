﻿using System.Runtime.Serialization;

namespace MultiPlug.Ext.RasPi.Config.Models.Components.Network
{
    public class NetworkProperties : SharedProperties
    {
        public string Eth0IPAddress { get; set; }
        public string Eth0IP6Address { get; set; }
        public string[] Eth0Routers { get; set; }
        public string[] Eth0DomainNameServers { get; set; }

        public string Wlan0IPAddress { get; set; }
        public string Wlan0IP6Address { get; set; }
        public string[] Wlan0Routers { get; set; }
        public string[] Wlan0DomainNameServers { get; set; }
        [DataMember]
        public string HostName { get; set; }
        [DataMember]
        public string WiFiCountry { get; set; }
        [DataMember]
        public string SSID { get; set; }
        public string Passphrase { get; set; }
        public NICInterface[] Interfaces { get; set; }
    }
}
