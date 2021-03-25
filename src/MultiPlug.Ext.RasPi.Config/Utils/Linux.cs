using System;
using System.Runtime.InteropServices;

namespace MultiPlug.Ext.RasPi.Config.Utils
{
    internal static class Linux
    {
        [DllImport("libcrypt.so", EntryPoint = "crypt", ExactSpelling = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr UnixCrypt([MarshalAs(UnmanagedType.LPStr)]string key, [MarshalAs(UnmanagedType.LPStr)]string salt);

        internal static string Crypt(string theKey, string theSalt)
        {
            return Marshal.PtrToStringAnsi(UnixCrypt(theKey, theSalt));
        }
    }
}
