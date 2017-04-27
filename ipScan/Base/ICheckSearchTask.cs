using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ipScan.Classes.IP;

namespace ipScan.Base
{
    interface ICheckSearchTask
    {
        DateTime lastTime { get; }

        void Check();
        void Start();
        void Stop();
    }
}
