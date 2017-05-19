using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ipScan.Base.IP
{
    class IPTools
    {        
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
    }
}
