using System;
using System.Collections.Generic;
using System.Linq;
using MultiPlug.Ext.RasPi.Config.Models.Components.Network;

namespace MultiPlug.Ext.RasPi.Config.Components.Network
{
    internal static class IFCONFIG
    {
        internal static NICInterface[] Parse(string theIfConfigResult)
        {
            string[] Lines = theIfConfigResult.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            List<NICInterface> NICInterfaceList = new List<NICInterface>();

            NICInterface CurrentNICInterface = null;

            foreach ( string line in Lines)
            {
                if ( ! line.StartsWith(" ") )
                {
                    if (CurrentNICInterface != null)
                    {
                        NICInterfaceList.Add(CurrentNICInterface);
                    }
                    CurrentNICInterface = new NICInterface();
                    CurrentNICInterface.Name = line.Split(':').First();
                }
                else
                {
                    string TrimmedLine = line.Trim();

                    if (TrimmedLine.StartsWith("inet "))
                    {
                        string[] InterfaceProperties = TrimmedLine.Split(new string[] { "  " }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string InterfaceProperty in InterfaceProperties)
                        {
                            if (InterfaceProperty.StartsWith("inet"))
                            {
                                CurrentNICInterface.Inet = InterfaceProperty.Split(' ').Last();
                            }
                            else if (InterfaceProperty.StartsWith("netmask"))
                            {
                                CurrentNICInterface.Netmask = InterfaceProperty.Split(' ').Last();
                            }
                            else if (InterfaceProperty.StartsWith("broadcast"))
                            {
                                CurrentNICInterface.Broadcast = InterfaceProperty.Split(' ').Last();
                            }
                        }
                    }
                }
            }

            if(CurrentNICInterface != null)
            {
                NICInterfaceList.Add(CurrentNICInterface);
            }

            return NICInterfaceList.ToArray();
        }
    }
}
