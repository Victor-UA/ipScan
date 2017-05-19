using System.Net;
using System.Net.NetworkInformation;

namespace ipScan.Base.IP
{
    public class PingBySocketReply
    {
        public PingBySocketReply(IPAddress Address, long RoundtripTime, IPStatus Status)
        {
            this.Address = Address;
            this.RoundtripTime = RoundtripTime;
            this.Status = Status;
        }

        public IPStatus Status { get; }
        public IPAddress Address { get; }
        public long RoundtripTime { get; }
        public PingOptions Options { get; }
        public byte[] Buffer { get; }
    }
}
