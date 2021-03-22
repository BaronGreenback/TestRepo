using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Test
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct IpAddressInfo
        {
            public int InterfaceIndex;
            public fixed byte AddressBytes[16];
            public byte NumAddressBytes;
            public byte PrefixLength;
            private fixed byte __padding[2];
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct NetworkInterfaceInfo
        {
            public fixed byte Name[16];
            public long Speed;
            public int InterfaceIndex;
            public int Mtu;
            public ushort HardwareType;
            public byte OperationalState;
            public byte NumAddressBytes;
            public fixed byte AddressBytes[8];
            public byte SupportsMulticast;
            private fixed byte __padding[3];
        }

        [DllImport("libSystem.Native", EntryPoint = "SystemNative_GetNetworkInterfaces")]
        public static unsafe extern int GetNetworkInterfaces(ref int count, ref NetworkInterfaceInfo* addrs, ref int addressCount, ref IpAddressInfo* aa);

        /// <summary>
        /// Test routine.
        /// </summary>
        /// <returns></returns>
        public static unsafe void GetLinuxNetworkInterfaces()
        {
            int interfaceCount = 0;
            int addressCount = 0;
            NetworkInterfaceInfo* nii = null;
            IpAddressInfo* ai = null;
            IntPtr globalMemory = (IntPtr)null;

            int i = GetNetworkInterfaces(ref interfaceCount, ref nii, ref addressCount, ref ai);
            if (i != 0)
            {
                Trace.WriteLine("Error getting interfaces. " + i.ToString());
                return;
            }

            globalMemory = (IntPtr)nii;
            try
            {
                NetworkInterface[] interfaces = new NetworkInterface[interfaceCount];

                for (int x = 0; x < interfaceCount; x++)
                {
                    Trace.WriteLine("Name: " + Marshal.PtrToStringAnsi((IntPtr) nii->Name)!);
                    Trace.WriteLine("Speed:" + nii->Speed);
                    Trace.WriteLine("Idx: " + nii->InterfaceIndex);
                    Trace.WriteLine("MTU:" + nii->Mtu);
                    Trace.WriteLine("Type:" + (NetworkInterfaceType) nii->HardwareType);
                    Trace.WriteLine("Speed:" + (OperationalStatus) nii->OperationalState);                    
                    Trace.WriteLine("Multicast" + nii->SupportsMulticast);
                    if (nii->NumAddressBytes > 0)
                    {
                        Trace.WriteLine("Addr Bytes " + new PhysicalAddress(new ReadOnlySpan<byte>(nii->AddressBytes, nii->NumAddressBytes).ToArray()));
                    }
                    Trace.WriteLine("Addr Bytes: " + new ReadOnlySpan<byte>(ai->AddressBytes, ai->NumAddressBytes).ToString());
                }
            }
            finally
            {
                Marshal.FreeHGlobal(globalMemory);
            }
        }

        static void Main(string[] args)
        {
            GetLinuxNetworkInterfaces();
        }
    }
}
