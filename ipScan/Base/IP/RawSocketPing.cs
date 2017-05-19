using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;
using ipScan.Base.IP.ProtocolHeaderDefinition;

namespace ipScan.Base.IP
{
    class RawSocketPing
    {
        public Socket pingSocket;                    // Raw socket handle
        public AddressFamily pingFamily;             // Indicates IPv4 or IPv6 ping
        public int pingTtl;                                 // Time-to-live value to set on ping
        public ushort pingId;                              // ID value to set in ping packet
        public ushort pingSequence;               // Current sending sequence number
        public int pingPayloadLength;          // Size of the payload in ping packet
        public int pingCount;                          // Number of times to send ping request
        public int pingOutstandingReceives;    // Number of outstanding receive operations
        public int pingReceiveTimeout;             // Timeout value to wait for ping response
        private IPEndPoint destEndPoint;                // Destination being pinged
        public IPEndPoint responseEndPoint;        // Contains the source address of the ping response
        public EndPoint castResponseEndPoint;   // Simple cast time used for the responseEndPoint
        private byte[] pingPacket;                         // Byte array of ping packet built
        private byte[] pingPayload;                       // Payload in the ping packet
        private byte[] receiveBuffer;                      // Buffer used to receive ping response
        private IcmpHeader icmpHeader;                // ICMP header built (for IPv4)
        private Icmpv6Header icmpv6Header;           // ICMP header built (for IPv6)
        private Icmpv6EchoRequest icmpv6EchoRequestHeader;    // ICMPv6 echo request header (for IPv6)
        private ArrayList protocolHeaderList;      // List of protocol headers to assemble into a packet
        private AsyncCallback receiveCallback;     // Async callback function called when a receive completes
        private DateTime pingSentTime;            // Timestamp of when ping request was sent
        public ManualResetEvent pingReceiveEvent;   // Event to signal main thread that receive completed
        public ManualResetEvent pingDoneEvent;        // Event to indicate all outstanding receives are done
                                                      //    this ping class can be disposed

        /// <summary>
        /// Base constructor that initializes the member variables to default values. It also
        /// creates the events used and initializes the async callback function.
        /// </summary>
        public RawSocketPing()
        {
            pingSocket = null;
            pingFamily = AddressFamily.InterNetwork;
            pingTtl = 8;
            pingPayloadLength = 8;
            pingSequence = 0;
            pingReceiveTimeout = 4000;
            pingOutstandingReceives = 0;
            destEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
            protocolHeaderList = new ArrayList();
            pingReceiveEvent = new ManualResetEvent(false);
            pingDoneEvent = new ManualResetEvent(false);
            receiveCallback = new AsyncCallback(PingReceiveCallback);
            icmpHeader = null;
            icmpv6Header = null;
            icmpv6EchoRequestHeader = null;
        }

        /// <summary>
        /// Constructor that overrides several members of the ping packet such as TTL,
        /// payload length, ping ID, etc.
        /// </summary>
        /// <param name="af">Indicates whether we're doing IPv4 or IPv6 ping</param>
        /// <param name="ttlValue">Time-to-live value to set on ping packet</param>
        /// <param name="payloadSize">Number of bytes in ping payload</param>
        /// <param name="sendCount">Number of times to send a ping request</param>
        /// <param name="idValue">ID value to put into ping header</param>
        public RawSocketPing(
            AddressFamily af,
            int ttlValue,
            int payloadSize,
            int sendCount,
            ushort idValue
            ) : this()
        {
            pingFamily = af;
            pingTtl = ttlValue;
            pingPayloadLength = payloadSize;
            pingCount = sendCount;
            pingId = idValue;
        }

        /// <summary>
        /// This routine is called when the calling application is done with the ping class.
        /// This routine closes any open resource such as events and socket handles. This
        /// routine closes the socket handle and then blocks on the pingDoneEvent. This is
        /// necessary as if there is an outstanding async ReceiveFrom pending, it will complete
        /// with an error once the socket handle is closed. It is cleaner to wait for any
        /// async operations to complete before disposing of resources it may depend on.
        /// </summary>
        public void Close()
        {
            try
            {
                // Close the socket handle which will cause any async operations on it to complete with an error.
                Console.WriteLine("In closing method...");
                if (pingSocket != null)
                {
                    pingSocket.Close();
                    pingSocket = null;
                }

                // Wait for the async handler to signal no more outstanding operations
                if (pingDoneEvent.WaitOne(10000, false) == false)
                {
                    Console.WriteLine("Wait timed out: Outstanding operations: {0}", pingOutstandingReceives);
                }

                // Close the opened events
                if (pingReceiveEvent != null)
                    pingReceiveEvent.Close();
                if (pingDoneEvent != null)
                    pingDoneEvent.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine("Error occurred during cleanup: {0}", err.Message);
                throw;
            }
        }

        /// <summary>
        /// Since ICMP raw sockets don't care about the port (as the ICMP protocol has no port
        /// field), we require the caller to just update the IPAddress of the destination
        /// although internally we keep it as an IPEndPoint since the SendTo method requires
        /// that (and the port is simply set to zero).
        /// </summary>
        public IPAddress PingAddress
        {
            get
            {
                return destEndPoint.Address;
            }
            set
            {
                destEndPoint = new IPEndPoint(value, 0);
            }
        }

        /// <summary>
        /// This routine initializes the raw socket, sets the TTL, allocates the receive
        /// buffer, and sets up the endpoint used to receive any ICMP echo responses.
        /// </summary>
        public void InitializeSocket()
        {
            IPEndPoint localEndPoint;

            if (destEndPoint.AddressFamily == AddressFamily.InterNetwork)
            {
                // Create the raw socket
                Console.WriteLine("Creating raw socket using Socket()...");
                pingSocket = new Socket(destEndPoint.AddressFamily, SocketType.Raw, ProtocolType.Icmp);
                localEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // Socket must be bound locally before socket options can be applied
                Console.WriteLine("Binding the socket using Bind()...");
                pingSocket.Bind(localEndPoint);

                Console.WriteLine("Using SetSocketOption()...");
                pingSocket.SetSocketOption(
                    SocketOptionLevel.IP,
                    SocketOptionName.IpTimeToLive,
                    pingTtl
                    );

                // Allocate the buffer used to receive the response
                Console.WriteLine("Allocating buffer for received response...");
                receiveBuffer = new byte[Ipv4Header.Ipv4HeaderLength + IcmpHeader.IcmpHeaderLength + pingPayloadLength];
                responseEndPoint = new IPEndPoint(IPAddress.Any, 0);
                castResponseEndPoint = (EndPoint)responseEndPoint;
            }
            else if (destEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                // Create the raw socket
                Console.WriteLine("Creating raw socket using Socket()...");
                pingSocket = new Socket(
                    destEndPoint.AddressFamily,
                    SocketType.Raw,
                    (ProtocolType)58       // ICMPv6 protocol value
                    );

                localEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                Console.WriteLine("IPEndPoint() for local is OK...");

                // Socket must be bound locally before socket options can be applied
                Console.WriteLine("Binding to the interface using Bind()...");
                pingSocket.Bind(localEndPoint);

                Console.WriteLine("Using the SetSocketOPtion()...");
                pingSocket.SetSocketOption(
                    SocketOptionLevel.IPv6,
                    SocketOptionName.IpTimeToLive,
                    pingTtl
                    );

                // Allocate the buffer used to receive the response
                Console.WriteLine("Allocating buffer for the received response...");
                receiveBuffer = new byte[Ipv6Header.Ipv6HeaderLength +
                    Icmpv6Header.Icmpv6HeaderLength + Icmpv6EchoRequest.Icmpv6EchoRequestLength +
                    pingPayloadLength];

                responseEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                Console.WriteLine("IPEndPoint() for remote is OK...");
                castResponseEndPoint = (EndPoint)responseEndPoint;
            }
        }

        /// <summary>
        /// This routine builds the appropriate ICMP echo packet depending on the
        /// protocol family requested.
        /// </summary>
        public void BuildPingPacket()
        {
            // Initialize the socket if it hasn't already been done
            Console.WriteLine("Building the ping packet...");
            Console.WriteLine("Initializing the socket if not done yet...");

            if (pingSocket == null)
            {
                InitializeSocket();
            }

            // Clear any existing headers in the list
            Console.WriteLine("Clearing any existing headers in the list using Clear()...");
            protocolHeaderList.Clear();

            if (destEndPoint.AddressFamily == AddressFamily.InterNetwork)
            {
                // Create the ICMP header and initialize the members
                Console.WriteLine("Creating the ICMP header and initialize the members...");
                icmpHeader = new IcmpHeader();

                icmpHeader.Id = pingId;
                icmpHeader.Sequence = pingSequence;
                icmpHeader.Type = IcmpHeader.EchoRequestType;
                icmpHeader.Code = IcmpHeader.EchoRequestCode;

                // Build the data payload of the ICMP echo request
                Console.WriteLine("Building the data payload of the ICMP echo request...");
                pingPayload = new byte[pingPayloadLength];

                for (int i = 0; i < pingPayload.Length; i++)
                {
                    pingPayload[i] = (byte)'e';
                }

                // Add ICMP header to the list of headers
                Console.WriteLine("Adding ICMP header to the list of headers using Add()...");
                protocolHeaderList.Add(icmpHeader);
            }
            else if (destEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                Ipv6Header ipv6Header;     // Required for pseudo header checksum
                IPEndPoint localInterface;
                byte[] localAddressBytes = new byte[28];

                Console.WriteLine("This part is for IPv6...");
                // An IPv6 header is required since the IPv6 protocol specifies that the
                //    pseudo header checksum needs to be calculated on ICMPv6 packets which
                //    requires the source and destination address that will appear in the
                //    IPv6 packet.
                ipv6Header = new Ipv6Header();

                // We definitely know the destination IPv6 address but the stack will
                //    choose the "appropriate" local v6 interface depending on the
                //    routing table which may be different than the address we bound
                //    the socket to. Because of this we will call the Winsock ioctl
                //    SIO_ROUTING_INTERFACE_QUERY which will return the local interface
                //    for a given destination address by querying the routing table.
                Console.WriteLine("Implementing the IOControl()...");
                pingSocket.IOControl(
                    WinsockIoctl.SIO_ROUTING_INTERFACE_QUERY,
                    SockaddrConvert.GetSockaddrBytes(destEndPoint),
                    localAddressBytes
                    );

                localInterface = SockaddrConvert.GetEndPoint(localAddressBytes);

                // Fill out the fields of the IPv6 header used in the pseudo-header checksum calculation
                Console.WriteLine("Filling out the IPv6 header fields...");
                ipv6Header.SourceAddress = localInterface.Address;
                ipv6Header.DestinationAddress = destEndPoint.Address;
                ipv6Header.NextHeader = 58;     // IPPROTO_ICMP6

                // Initialize the ICMPv6 header
                Console.WriteLine("Initializing the ICMPv6 header...");
                icmpv6Header = new Icmpv6Header(ipv6Header);
                icmpv6Header.Type = Icmpv6Header.Icmpv6EchoRequestType;
                icmpv6Header.Code = Icmpv6Header.Icmpv6EchoRequestCode;

                // Initialize the payload
                Console.WriteLine("Initializing the payload...");
                pingPayload = new byte[pingPayloadLength];

                for (int i = 0; i < pingPayload.Length; i++)
                {
                    pingPayload[i] = (byte)'e';
                }

                // Create the ICMPv6 echo request header
                Console.WriteLine("Creating the ICMPv6 echo request header...");
                icmpv6EchoRequestHeader = new Icmpv6EchoRequest();
                icmpv6EchoRequestHeader.Id = pingId;

                // Add the headers to the protocol header list
                Console.WriteLine("Adding the headers to the protocol header list...");
                protocolHeaderList.Add(icmpv6Header);
                protocolHeaderList.Add(icmpv6EchoRequestHeader);
            }
        }

        /// <summary>
        /// This is the asynchronous callback that is fired when an async ReceiveFrom.
        /// An asynchronous ReceiveFrom is posted by calling BeginReceiveFrom. When this
        /// function is invoked, it calculates the elapsed time between when the ping
        /// packet was sent and when it was completed.
        /// </summary>
        /// <param name="ar">Asynchronous context for operation that completed</param>
        static void PingReceiveCallback(IAsyncResult ar)
        {
            RawSocketPing rawSock = (RawSocketPing)ar.AsyncState;
            TimeSpan elapsedTime;
            int bytesReceived = 0;
            ushort receivedId = 0;

            try
            {
                // Keep a count of how many async operations are outstanding -- one just completed
                //    so decrement the count.
                Interlocked.Decrement(ref rawSock.pingOutstandingReceives);

                // If we're done because ping is exiting and the socket has been closed,
                //    set the done event
                if (rawSock.pingSocket == null)
                {
                    if (rawSock.pingOutstandingReceives == 0)
                        rawSock.pingDoneEvent.Set();
                    return;
                }

                // Complete the receive op by calling EndReceiveFrom. This will return the number
                //    of bytes received as well as the source address of who sent this packet.
                bytesReceived = rawSock.pingSocket.EndReceiveFrom(ar, ref rawSock.castResponseEndPoint);

                // Calculate the elapsed time from when the ping request was sent and a response was
                //    received.
                elapsedTime = DateTime.Now - rawSock.pingSentTime;

                rawSock.responseEndPoint = (IPEndPoint)rawSock.castResponseEndPoint;

                // Here we unwrap the data received back into the respective protocol headers such
                //    that we can find the ICMP ID in the ICMP or ICMPv6 packet to verify that
                //    the echo response we received was really a response to our request.
                if (rawSock.pingSocket.AddressFamily == AddressFamily.InterNetwork)
                {
                    Ipv4Header v4Header;
                    IcmpHeader icmpv4Header;
                    byte[] pktIcmp;
                    int offset = 0;

                    // Remember, raw IPv4 sockets will return the IPv4 header along with all
                    //    subsequent protocol headers
                    v4Header = Ipv4Header.Create(rawSock.receiveBuffer, ref offset);
                    pktIcmp = new byte[bytesReceived - offset];
                    Array.Copy(rawSock.receiveBuffer, offset, pktIcmp, 0, pktIcmp.Length);
                    icmpv4Header = IcmpHeader.Create(pktIcmp, ref offset);

                    /*Console.WriteLine("Icmp.Id = {0}; Icmp.Sequence = {1}",
                        icmpv4Header.Id,
                        icmpv4Header.Sequence
                        );*/

                    receivedId = icmpv4Header.Id;
                }
                else if (rawSock.pingSocket.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    Icmpv6Header icmp6Header;
                    Icmpv6EchoRequest echoHeader;
                    byte[] pktEchoRequest;
                    int offset = 0;

                    // For IPv6 raw sockets, the IPv6 header is never returned along with the
                    //    data received -- the received data always starts with the header
                    //    following the IPv6 header.
                    icmp6Header = Icmpv6Header.Create(rawSock.receiveBuffer, ref offset);
                    pktEchoRequest = new byte[bytesReceived - offset];
                    Array.Copy(rawSock.receiveBuffer, offset, pktEchoRequest, 0, pktEchoRequest.Length);
                    echoHeader = Icmpv6EchoRequest.Create(pktEchoRequest, ref offset);

                    /*Console.WriteLine("Icmpv6.Id = {0}; Icmp.Sequence = {1}",
                        echoHeader.Id,
                        echoHeader.Sequence
                        );*/

                    receivedId = echoHeader.Id;
                }

                if (receivedId == rawSock.pingId)
                {
                    string elapsedString;

                    // Print out the usual statistics for ping
                    if (elapsedTime.Milliseconds < 1)
                        elapsedString = "<1";
                    else
                        elapsedString = "=" + elapsedTime.Milliseconds.ToString();

                    Console.WriteLine("Reply from {0}: byte={1} time{2}ms TTL={3} ",
                        rawSock.responseEndPoint.Address.ToString(),
                        bytesReceived,
                        elapsedString,
                        rawSock.pingTtl
                        );
                }

                // Post another async receive if the count indicates for us to do so.
                if (rawSock.pingCount > 0)
                {
                    rawSock.pingSocket.BeginReceiveFrom(
                        rawSock.receiveBuffer,
                        0,
                        rawSock.receiveBuffer.Length,
                        SocketFlags.None,
                        ref rawSock.castResponseEndPoint,
                        rawSock.receiveCallback,
                        rawSock
                        );

                    // Keep track of outstanding async operations
                    Interlocked.Increment(ref rawSock.pingOutstandingReceives);
                }
                else
                {
                    // If we're done then set the done event
                    if (rawSock.pingOutstandingReceives == 0)
                        rawSock.pingDoneEvent.Set();
                }
                // If this is indeed the response to our echo request then signal the main thread
                //    that we received the response so it can send additional echo requests if
                //    necessary. This is done after another async ReceiveFrom is already posted.
                if (receivedId == rawSock.pingId)
                {
                    rawSock.pingReceiveEvent.Set();
                }
            }
            catch (SocketException err)
            {
                Console.WriteLine("Socket error occurred in async callback: {0}", err.Message);
            }
        }

        /// <summary>
        /// This function performs the actual ping. It sends the ping packets created to
        /// the destination and posts the async receive operation to receive the response.
        /// Once a ping is sent, it waits for the receive handler to indicate a response
        /// was received. If not it times out and indicates this to the user.
        /// </summary>
        public void DoPing()
        {
            // If the packet hasn't already been built, then build it.
            Console.WriteLine("In DoPing() method, do the pinging...");
            Console.WriteLine();
            if (protocolHeaderList.Count == 0)
            {
                BuildPingPacket();
            }

            Console.WriteLine("Pinging {0} with {1} bytes of data ", destEndPoint.Address.ToString(), pingPayloadLength);

            try
            {
                // Post an async receive first to ensure we receive a response to the echo request
                //    in the event of a low latency network.
                pingSocket.BeginReceiveFrom(
                    receiveBuffer,
                    0,
                    receiveBuffer.Length,
                    SocketFlags.None,
                    ref castResponseEndPoint,
                    receiveCallback,
                    this
                    );
                // Keep track of how many outstanding async operations there are
                Interlocked.Increment(ref pingOutstandingReceives);

                // Send an echo request and wait for the response
                while (pingCount > 0)
                {
                    Interlocked.Decrement(ref pingCount);

                    // Increment the sequence count in the ICMP header
                    if (destEndPoint.AddressFamily == AddressFamily.InterNetwork)
                    {
                        icmpHeader.Sequence = (ushort)(icmpHeader.Sequence + (ushort)1);

                        // Build the byte array representing the ping packet. This needs to be done
                        //    before ever send because we change the sequence number (which will affect
                        //    the calculated checksum).
                        pingPacket = icmpHeader.BuildPacket(protocolHeaderList, pingPayload);
                    }
                    else if (destEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        icmpv6EchoRequestHeader.Sequence = (ushort)(icmpv6EchoRequestHeader.Sequence + (ushort)1);

                        // Build the byte array representing the ping packet. This needs to be done
                        //    before ever send because we change the sequence number (which will affect
                        //    the calculated checksum).
                        pingPacket = icmpv6Header.BuildPacket(protocolHeaderList, pingPayload);
                    }

                    // Mark the time we sent the packet
                    pingSentTime = DateTime.Now;

                    // Send the echo request
                    pingSocket.SendTo(pingPacket, destEndPoint);

                    // Wait for the async handler to indicate a response was received
                    if (pingReceiveEvent.WaitOne(pingReceiveTimeout, false) == false)
                    {
                        // timeout occurred
                        Console.WriteLine("Request timed out.");
                    }
                    else
                    {
                        // Reset the event
                        pingReceiveEvent.Reset();
                    }

                    // Sleep for a short time before sending the next request
                    Thread.Sleep(1000);
                }
            }
            catch (SocketException err)
            {
                Console.WriteLine("Socket error occurred: {0}", err.Message);
                throw;
            }
        }
    }
}
