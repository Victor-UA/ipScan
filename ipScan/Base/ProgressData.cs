using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ipScan.Base
{
    class ProgressData : IProgressData
    {
        public ProgressData()
        {
            TimePassed = TimeSpan.MinValue;
            TimeLeft = TimeSpan.MinValue;
        }

        public int Progress { get; set; }
        public int TasksCount { get; set; }
        public int SubTasksCount { get; set; }
        public TimeSpan TimePassed { get; set; }
        public TimeSpan TimeLeft { get; set; }
        public int PauseTime { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
