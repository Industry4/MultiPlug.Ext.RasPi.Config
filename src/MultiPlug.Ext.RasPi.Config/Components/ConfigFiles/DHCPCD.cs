using System;
using System.Linq;
using System.Collections.Generic;
using MultiPlug.Ext.RasPi.Config.Models.ConfigFiles.DHCPCD;
using System.Text;

namespace MultiPlug.Ext.RasPi.Config.Components.ConfigFiles
{
    static class DHCPCD
    {
        public const string FileLocation = "/etc/dhcpcd.conf";

        public static ReadResult Read()
        {
            string line;

            var ConfLines = new List<DHCPcdConfLine>();
            var IPConfigs = new List<NICProperties>();

            NICProperties prop = null;

            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(FileLocation))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        line.Trim();

                        if (prop != null && line.StartsWith("static", StringComparison.OrdinalIgnoreCase))
                        {
                            line = line.Remove(0, 6);
                            line = line.Trim();

                            if (line.StartsWith("ip_address", StringComparison.OrdinalIgnoreCase))
                            {
                                var LineSplit = line.Split(new char[] { '=' });

                                if (LineSplit.Length == 2)
                                {
                                    line = LineSplit[1];
                                    line = line.Trim();

                                    prop.IPAddress = line;
                                    ConfLines.Add(new DHCPcdConfLine { NICProperties = prop, IPAddress = true });
                                }
                            }
                            else if (line.StartsWith("ip6_address", StringComparison.OrdinalIgnoreCase))
                            {
                                var LineSplit = line.Split(new char[] { '=' });

                                if (LineSplit.Length == 2)
                                {
                                    line = LineSplit[1];
                                    line = line.Trim();

                                    prop.IP6Address = line;
                                    ConfLines.Add(new DHCPcdConfLine { NICProperties = prop, IP6Address = true });
                                }
                            }
                            else if (line.StartsWith("routers", StringComparison.OrdinalIgnoreCase))
                            {
                                var LineSplit = line.Split(new char[] { '=' });

                                if (LineSplit.Length == 2)
                                {
                                    line = LineSplit[1];
                                    line = line.Trim();

                                    LineSplit = line.Split(null);

                                    if (LineSplit.Length > 0)
                                    {
                                        prop.Routers = LineSplit;
                                        ConfLines.Add(new DHCPcdConfLine { NICProperties = prop, Routers = true });
                                    }
                                }
                            }
                            else if (line.StartsWith("domain_name_servers", StringComparison.OrdinalIgnoreCase))
                            {
                                var LineSplit = line.Split(new char[] { '=' });

                                if (LineSplit.Length == 2)
                                {
                                    line = LineSplit[1];
                                    line = line.Trim();

                                    LineSplit = line.Split(null);

                                    if (LineSplit.Length > 0)
                                    {
                                        prop.DomainNameServers = LineSplit;
                                        ConfLines.Add(new DHCPcdConfLine { NICProperties = prop, DomainNameServers = true });
                                    }
                                }
                            }
                        }
                        else if (line.StartsWith("interface", StringComparison.OrdinalIgnoreCase))
                        {


                            string[] ssize = line.Split(null);

                            if (ssize.Length == 2)
                            {
                                prop = new NICProperties(ssize[1]);

                                IPConfigs.Add(prop);
                                ConfLines.Add(new DHCPcdConfLine { NICProperties = prop, Id = true });
                            }
                        }
                        else
                        {
                            ConfLines.Add(new DHCPcdConfLine { Line = line });
                        }
                    }
                }
            }
            catch( System.IO.FileNotFoundException)
            {

            }

            return new ReadResult { ConfigLines = ConfLines.ToArray(), Properties = IPConfigs.ToArray() };
        }

        private static void Write(DHCPcdConfLine[] theLines)
        {
            var ConfLine = new List<string>();


            foreach (DHCPcdConfLine Line in theLines)
            {
                if (Line.NICProperties != null)
                {
                    if (Line.Id)
                    {
                        ConfLine.Add(string.Format("interface {0}\n", Line.NICProperties.Id));
                    }
                    else if (Line.IPAddress)
                    {
                        ConfLine.Add(string.Format("static ip_address={0}\n", Line.NICProperties.IPAddress));
                    }
                    else if (Line.IP6Address)
                    {
                        ConfLine.Add(string.Format("static ip6_address={0}\n", Line.NICProperties.IP6Address));
                    }
                    else if (Line.Routers)
                    {
                        ConfLine.Add(string.Format("static routers={0}\n", string.Join(" ", Line.NICProperties.Routers)));
                    }
                    else if (Line.DomainNameServers)
                    {
                        ConfLine.Add(string.Format("static domain_name_servers={0}\n", string.Join(" ", Line.NICProperties.DomainNameServers)));
                    }
                }
                else
                {
                    ConfLine.Add(string.Format(Line.Line + "\n"));
                }
            }
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(FileLocation, false, new UTF8Encoding(false)))
                {
                    foreach (string line in ConfLine.ToArray())
                    {
                        file.Write(line);
                    }
                }
            }
            catch
            {

            }
        }

        public static void Update(NICProperties[] theProperties)
        {
            var ReadResult = Read();

            DHCPcdConfLine[] ToWrite = ReadResult.ConfigLines;

            foreach(NICProperties Properties in theProperties)
            {
                var NICSearch = ReadResult.Properties.FirstOrDefault(nic => nic.Id == Properties.Id);

                if (NICSearch != null)
                {
                    if (NullValues(Properties))
                    {
                        ToWrite = DeleteLines(ToWrite, NICSearch);
                    }
                    else
                    {
                        ToWrite = UpdateLines(ToWrite, NICSearch, Properties);
                    }
                }
                else if (!NullValues(Properties))
                {
                    ToWrite = AddLines(ToWrite, Properties);
                }
            }

            Write(ToWrite);
        }

        private static bool NullValues(NICProperties theProperties)
        {
            return theProperties.IPAddress == null &&
                    theProperties.IP6Address == null &&
                    theProperties.DomainNameServers == null &&
                    theProperties.Routers == null;
        }

        private static DHCPcdConfLine[] DeleteLines(DHCPcdConfLine[] theConfigLines, NICProperties theExistingProperties)
        {
            var ConfigLinesList = new List<DHCPcdConfLine>(theConfigLines);

            ConfigLinesList.RemoveAll(Line => Line?.NICProperties?.Id == theExistingProperties.Id);

            return ConfigLinesList.ToArray();
        }

        private static DHCPcdConfLine[] AddLines(DHCPcdConfLine[] theConfigLines, NICProperties NewProperties)
        {
            var ConfigLinesList = new List<DHCPcdConfLine>(theConfigLines);

            ConfigLinesList.Add(new DHCPcdConfLine { Id = true, NICProperties = NewProperties });

            if (NewProperties.IPAddress != null)
            {
                ConfigLinesList.Add(new DHCPcdConfLine { IPAddress = true, NICProperties = NewProperties });
            }

            if (NewProperties.IP6Address != null)
            {
                ConfigLinesList.Add(new DHCPcdConfLine { IP6Address = true, NICProperties = NewProperties });
            }

            if (NewProperties.Routers != null)
            {
                ConfigLinesList.Add(new DHCPcdConfLine { Routers = true, NICProperties = NewProperties });
            }

            if (NewProperties.DomainNameServers != null)
            {
                ConfigLinesList.Add(new DHCPcdConfLine { DomainNameServers = true, NICProperties = NewProperties });
            }

            return ConfigLinesList.ToArray();
        }

        private static DHCPcdConfLine[] UpdateLines(DHCPcdConfLine[] theConfigLines, NICProperties theExistingProperties, NICProperties theNewProperties)
        {
            var ConfigLinesList = new List<DHCPcdConfLine>(theConfigLines);
            var HeaderSearch = ConfigLinesList.FirstOrDefault(Line => Line?.NICProperties?.Id == theNewProperties.Id);
            var HeaderIndex = ConfigLinesList.IndexOf(HeaderSearch);

            if (theNewProperties.IPAddress != null)
            {
                // The New Properties has the IP Address value

                if (theExistingProperties.IPAddress == null)
                {
                    // But the current properties collection dosn't have a IP address value,
                    // so it will have to be injected at the current line.
                    theExistingProperties.IPAddress = theNewProperties.IPAddress;

                    // Now insert the new value below the NIC Id header.
                    ConfigLinesList.Insert(HeaderIndex + 1, new DHCPcdConfLine { IPAddress = true, NICProperties = theExistingProperties });
                }
                else
                {
                    theExistingProperties.IPAddress = theNewProperties.IPAddress;
                }
            }
            else
            {
                ConfigLinesList.RemoveAll(Line => Line?.NICProperties?.Id == theExistingProperties.Id && Line.IPAddress);
            }

            if (theNewProperties.IP6Address != null)
            {
                if (theExistingProperties.IP6Address == null)
                {
                    theExistingProperties.IP6Address = theNewProperties.IP6Address;

                    ConfigLinesList.Insert(HeaderIndex + 1, new DHCPcdConfLine { IP6Address = true, NICProperties = theExistingProperties });
                }
                else
                {
                    theExistingProperties.IP6Address = theNewProperties.IP6Address;
                }
            }
            else
            {
                ConfigLinesList.RemoveAll(Line => Line?.NICProperties?.Id == theExistingProperties.Id && Line.IP6Address);
            }

            if (theNewProperties.Routers != null)
            {
                if (theExistingProperties.Routers == null)
                {
                    theExistingProperties.Routers = theNewProperties.Routers;

                    ConfigLinesList.Insert(HeaderIndex + 1, new DHCPcdConfLine { Routers = true, NICProperties = theExistingProperties });
                }
                else
                {
                    theExistingProperties.Routers = theNewProperties.Routers;
                }
            }
            else
            {
                ConfigLinesList.RemoveAll(Line => Line?.NICProperties?.Id == theExistingProperties.Id && Line.Routers);
            }

            if (theNewProperties.DomainNameServers != null)
            {
                if (theExistingProperties.DomainNameServers == null)
                {
                    theExistingProperties.DomainNameServers = theNewProperties.DomainNameServers;

                    ConfigLinesList.Insert(HeaderIndex + 1, new DHCPcdConfLine { DomainNameServers = true, NICProperties = theExistingProperties });
                }
                else
                {
                    theExistingProperties.DomainNameServers = theNewProperties.DomainNameServers;
                }
            }
            else
            {
                ConfigLinesList.RemoveAll(Line => Line?.NICProperties?.Id == theExistingProperties.Id && Line.DomainNameServers);
            }

            return ConfigLinesList.ToArray();
        }
    }
}
