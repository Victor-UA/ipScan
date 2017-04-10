using System.Threading;

namespace ipScan.Classes
{
    class Looking4HostNamesTimer
    {
        private IPInfo ipInfo { get; set; }
        public Looking4HostNamesTimer(IPInfo IPInfo, int Delay)
        {
            ipInfo = IPInfo;
            Thread.Sleep(Delay);
        }
    }
}
