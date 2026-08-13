using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MultiPlug.Ext.RasPi.Config.Utils.Swan;
using MultiPlug.Ext.RasPi.Config.Models.Components;
using MultiPlug.Ext.RasPi.Config.Models.Components.Network;

namespace MultiPlug.Ext.RasPi.Config.Components.Network
{
    /// <summary>
    /// Support for NetworkManager found in Raspberry Pi OS (Debian Bookworm (12) or Trixie (13) onwards)
    /// This replaces DHCPCD found in Debian 11
    /// https://wiki.debian.org/NetworkManager
    /// </summary>
    internal class NetworkManager
    {
        internal static ReadResult GetDevices()
        {
            var Devices = GetDeviceList();

            return new ReadResult
            {
                Properties = Devices.Select(Device => GetCurrentSettings(Device)).ToArray()
            };
        }

        private static int IndexOfNonWhitespace(string source, int startIndex = 0)
        {
            if (startIndex < 0) throw new ArgumentOutOfRangeException("startIndex");
            if (source != null)
                for (int i = startIndex; i < source.Length; i++)
                    if (!char.IsWhiteSpace(source[i])) return i;
            return -1;
        }

        private static string GetConnectionProfile(string theDeviceName)
        {
            var DeviceTask = ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "-f GENERAL.CONNECTION device show " + theDeviceName);
            DeviceTask.Wait();

            if (DeviceTask.Result.Okay())
            {
                string Line = DeviceTask.Result.GetOutput();
                // Example: 'GENERAL.CONNECTION:                     Wired connection 1'
                int Start = Line.IndexOf(":");
                int End = IndexOfNonWhitespace(Line, Start + 1);

                return Line.Substring(End).Trim();
            }

            return string.Empty;
        }

        private static NICProperties GetCurrentSettings(string theDeviceName)
        {
            var Result = new NICProperties(theDeviceName);

            string ConnectionProfile = GetConnectionProfile(theDeviceName);

            if (ConnectionProfile != string.Empty)
            {
                var ConnectionTask = ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "-f ipv4.method connection show \"" + ConnectionProfile + "\"");
                ConnectionTask.Wait();

                if (ConnectionTask.Result.Okay())
                {
                    if (ConnectionTask.Result.GetOutput().Contains("manual"))
                    {
                        ConnectionTask = ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "-f ipv4.addresses connection show \"" + ConnectionProfile + "\"");
                        ConnectionTask.Wait();

                        if (ConnectionTask.Result.Okay())
                        {
                            string Line = ConnectionTask.Result.GetOutput();
                            // Example: 'ipv4.addresses:                         192.168.1.181/24, 192.168.1.182/24'
                            int Start = Line.IndexOf(":");
                            int End = IndexOfNonWhitespace(Line, Start + 1);

                            Line = Line.Substring(End).Trim();

                            if (Line.StartsWith("--") == false)
                            {
                                Result.IPAddress = Line.Split(',').Select(v => v.Trim()).ToArray()[0]; // Split to get only the first IP in the list.
                            }
                        }
                    }
                }

                ConnectionTask = ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "-f ipv6.addresses connection show \"" + ConnectionProfile + "\"");
                ConnectionTask.Wait();

                if (ConnectionTask.Result.Okay())
                {
                    string Line = ConnectionTask.Result.GetOutput();
                    // Example: 'ipv6.addresses:                         192.168.1.181/24, 192.168.1.182/24'
                    int Start = Line.IndexOf(":");
                    int End = IndexOfNonWhitespace(Line, Start + 1);

                    Line = Line.Substring(End).Trim();

                    if (Line.StartsWith("--") == false)
                    {
                        Result.IP6Address = Line.Split(',').Select(v => v.Trim()).ToArray()[0]; // Split to get only the first IP in the list.
                    }
                }

                ConnectionTask = ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "-f ipv4.routes connection show \"" + ConnectionProfile + "\"");
                ConnectionTask.Wait();

                if (ConnectionTask.Result.Okay())
                {
                    string Line = ConnectionTask.Result.GetOutput();
                    // Example: 'ipv4.routes:                         { ip = 192.168.0.1/32, nh = 10.0.0.1 }; { ip = 192.168.1.1/32, nh = 10.0.0.1 }'
                    int Start = Line.IndexOf(":");
                    int End = IndexOfNonWhitespace(Line, Start + 1);

                    Line = Line.Substring(End).Trim();

                    if(Line.StartsWith("--") == false)
                    {
                        var RoutesStringFormat = Line.Split(';')
                            .Select(v => v.Trim()
                            .Replace(" ", "")) // Example: '{ip=192.168.0.1/32,nh=10.0.0.1}'
                            .ToArray();

                        var Routes = new List<string>();

                        foreach (var Route in RoutesStringFormat)
                        {
                            int RouteStart = Route.IndexOf("ip=");

                            if(RouteStart != -1)
                            {
                                int RouteEnd = Route.IndexOf("/", RouteStart);

                                if (RouteEnd != -1)
                                {
                                    Routes.Add(Route.Substring(RouteStart + 3 /* ip= */, RouteEnd - RouteStart - 3 /* ip= */));
                                }
                            }
                        
                        }

                        Result.Routers = Routes.ToArray();
                    }
                }

                ConnectionTask = ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "-f ipv4.dns connection show \"" + ConnectionProfile + "\"");
                ConnectionTask.Wait();

                if (ConnectionTask.Result.Okay())
                {
                    string Line = ConnectionTask.Result.GetOutput();
                    // Example: 'ipv4.dns:                         8.8.8.8,9.9.9.9'
                    int Start = Line.IndexOf(":");
                    int End = IndexOfNonWhitespace(Line, Start + 1);
                    Line = Line.Substring(End).Trim();

                    if (Line.StartsWith("--") == false)
                    {
                        Result.DomainNameServers = Line.Split(',').Select(v => v.Trim()).ToArray();
                    }
                }
            }

            return Result;
        }

        static string[] GetDeviceList()
        {
            List<string> DeviceList = new List<string>();

            var Task = ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "device");
            Task.Wait();

            if (Task.Result.Okay())
            {
                var lines = Task.Result.GetOutput().Split('\n');

                foreach (var line in lines)
                {
                    var columns = line.Split(' ');

                    if (columns[0] == "DEVICE" || columns[0] == string.Empty)
                    {
                        continue;
                    }

                    DeviceList.Add(columns[0]);
                }
            }

            return DeviceList.ToArray();
        }

        private static void UpdateDevice(NICProperties theNICProperties)
        {
            string ConnectionProfile = GetConnectionProfile(theNICProperties.Id);

            if (ConnectionProfile != string.Empty)
            {
                if (string.IsNullOrEmpty(theNICProperties.IPAddress))
                {
                    ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection modify \"" + ConnectionProfile + "\" ipv4.method auto ipv4.addresses \"\"").Wait();
                }
                else
                {
                    ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection modify \"" + ConnectionProfile + "\" ipv4.addresses \"" + theNICProperties.IPAddress + "\" ipv4.method manual").Wait();
                }

                if (string.IsNullOrEmpty(theNICProperties.IP6Address))
                {
                    ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection modify \"" + ConnectionProfile + "\" ipv6.method auto ipv6.addresses \"\"").Wait();
                }
                else
                {
                    ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection modify \"" + ConnectionProfile + "\" ipv6.addresses \"" + theNICProperties.IP6Address + "\" ipv6.method manual").Wait();
                }

                if (theNICProperties.Routers == null || theNICProperties.Routers.Length == 0)
                {
                    ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection modify \"" + ConnectionProfile + "\" ipv4.routes \"\"").Wait();
                }
                else
                {
                    ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection modify \"" + ConnectionProfile + "\" ipv4.routes \"" + string.Join(" ", theNICProperties.Routers) + "\"").Wait();
                }

                if (theNICProperties.DomainNameServers == null || theNICProperties.DomainNameServers.Length == 0)
                {
                    ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection modify \"" + ConnectionProfile + "\" ipv4.dns \"\"").Wait();
                }
                else
                {
                    ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection modify \"" + ConnectionProfile + "\" ipv4.dns \"" + string.Join(" ", theNICProperties.DomainNameServers) + "\"").Wait();
                }

                ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection down \"" + ConnectionProfile + "\"").Wait();
                ProcessRunner.GetProcessResultAsync(SharedProperties.c_NetworkMonitorCliCommand, "connection up \"" + ConnectionProfile + "\"").Wait();
            }
        }

        internal static Task<ProcessResult> UpdateDevices(NICProperties[] theEth, NICProperties[] theWlan)
        {
            return Task.Run(() =>
            {
                try
                {
                    foreach (var Eth in theEth)
                    {
                        UpdateDevice(Eth);
                    }

                    foreach (var Wlan in theWlan)
                    {
                        UpdateDevice(Wlan);
                    }
                }
                catch(Exception)
                {
                    return new ProcessResult(-1, string.Empty, string.Empty);
                }

                return new ProcessResult(0, string.Empty, string.Empty);
            });
        }
    }
}
