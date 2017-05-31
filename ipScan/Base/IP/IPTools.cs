using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ipScan.Base.IP
{
    class IPTools
    {
        [DllImport("Iphlpapi.dll", EntryPoint = "SendARP")]
        internal extern static Int32 SendArp(
            Int32 destIpAddress, Int32 srcIpAddress,
            byte[] macAddress, ref Int32 macAddressLength
        );

        public static uint IPAddress2UInt32(string ipAddress)
        {
            return IPAddress2UInt32(IPAddress.Parse(ipAddress));
        }
        //https://stackoverflow.com/questions/36831/how-do-you-parse-an-ip-address-string-to-a-uint-value-in-c
        public static uint IPAddress2UInt32(IPAddress ipAddress)
        {
            byte[] ipBytes = ipAddress.GetAddressBytes();
            uint result =   (uint)ipBytes[3] << 24;
            result +=       (uint)ipBytes[2] << 16;
            result +=       (uint)ipBytes[1] << 8;
            result +=       (uint)ipBytes[0];
            return result;
        }
        public static IPAddress UInt322IPAddress(uint ipAddress)
        {            
            return IPAddress.Parse(UInt322IPAddressStr(ipAddress));
        }
        //https://stackoverflow.com/questions/22058697/c-sharp-winrt-convert-ipv4-address-from-uint-to-string
        public static string UInt322IPAddressStr(uint ipAddress)
        {            
            byte[] bytes = BitConverter.GetBytes(ipAddress);
            return string.Join(".", bytes);            
        }

        public static PingBySocketReply PingHostBySocket(IPAddress Address)
        {
            try
            {
                byte[] data = new byte[1024];
                int recv = 0;

                Socket socket = new Socket(Address.AddressFamily, SocketType.Raw, ProtocolType.Icmp);

                IPEndPoint iep = new IPEndPoint(Address, 0);
                EndPoint ep = iep;

                ICMP packet = new ICMP()
                {
                    Type = 0x08,
                    Code = 0x00,
                    Checksum = 0
                };
                Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, packet.Message, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, packet.Message, 2, 2);
                data = Encoding.ASCII.GetBytes("test packet");
                Buffer.BlockCopy(data, 0, packet.Message, 4, data.Length);
                packet.MessageSize = data.Length + 4;
                int packetsize = packet.MessageSize + 4;

                UInt16 chcksum = packet.getChecksum();
                packet.Checksum = chcksum;

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1);
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                socket.SendTo(packet.getBytes(), packetsize, SocketFlags.None, iep);

                IPStatus ipStatus = !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0) ? IPStatus.Success : IPStatus.BadDestination;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                try
                {
                    data = new byte[1024];
                    recv = socket.ReceiveFrom(data, ref ep);
                }
                catch (SocketException)
                {
                    return new PingBySocketReply(Address, -1, IPStatus.IcmpError);
                }
                stopwatch.Stop();

                socket.Close();

                return new PingBySocketReply(Address, (long)stopwatch.Elapsed.TotalMilliseconds, ipStatus);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }

            return new PingBySocketReply(Address, -1, IPStatus.IcmpError);
        }
        public static Int32             ConvertIpToInt32(IPAddress apAddress)
        {
            byte[] bytes = apAddress.GetAddressBytes();
            return BitConverter.ToInt32(bytes, 0);

        }

        /// <summary>
        /// http://www.dreamincode.net/forums/topic/365661-Get-MAC-address-from-remote-computer/
        /// method for getting the MAC address of a remote computer
        /// NOTE: This only works on a local network computer that you have access to
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static PhysicalAddress   GetMACFromNetworkComputer(IPAddress ipAddress)
        {
            try
            {
                //check what family the ip is from <cref="http://msdn.microsoft.com/en-us/library/system.net.sockets.addressfamily.aspx"/>
                if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                    throw new ArgumentException("The remote system only supports IPv4 addresses");

                //convert the IPAddress to an Int32
                Int32 convertedIp = ConvertIpToInt32(ipAddress);
                Int32 src = ConvertIpToInt32(IPAddress.Any);
                //byte array
                byte[] macByteArray = new byte[6]; // 48 bit
                                                   //set the length of the byte array
                int len = macByteArray.Length;
                //call the Win32 API SendArp <cref="http://msdn.microsoft.com/en-us/library/aa366358%28VS.85%29.aspx"/>
                int arpReply = SendArp(convertedIp, src, macByteArray, ref len);

                //check the reply, zero (0) is an error
                if (arpReply != 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                //return the MAC address in a PhysicalAddress format
                return new PhysicalAddress(macByteArray);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                return null;
            }
        }
        public static PingReply         PingHost(IPAddress Address, int timeOut = 100)
        {
            //http://stackoverflow.com/questions/11800958/using-ping-in-c-sharp
            Ping pinger = new Ping();
            PingReply reply = null;
            try
            {
                reply = pinger.Send(Address, timeOut, new byte[] { 0 }, new PingOptions(64, true));
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            return reply;
        }
        public async static Task<PingReply> PingHostAsync(IPAddress Address, int timeOut = 100)
        {
            //http://stackoverflow.com/questions/11800958/using-ping-in-c-sharp
            Ping pinger = new Ping();
            PingReply reply = null;
            try
            {
                reply = await pinger.SendPingAsync(Address, timeOut, new byte[] { 0 }, new PingOptions(64, true)); 
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            return reply;
        }
    }
}
