using System.Net.Sockets;

namespace ipScan.Base.IP
{
    public class PortInfo
    {
        public int Port { get; set; }
        public ProtocolType ProtocolType { get; set; }
        /// <summary>
        /// Time To Live value of IP packet
        /// </summary>
        public bool isOpen { get; set; }

        public PortInfo(int port, ProtocolType protocolType, bool isOpen)
        {
            Port = port;
            ProtocolType = protocolType;
            this.isOpen = isOpen;
        }
    }
}
