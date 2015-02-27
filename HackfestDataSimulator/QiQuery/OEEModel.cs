using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiQuery
{
    public class OEEModel
    {
        public OEEModel(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
            Shift = string.Empty;
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Shift { get; set; }

        internal void Calculate()
        {
            Downtime = ScheduledSeconds - RunningTime;

            //availability
            Availability = RunningTime / ScheduledSeconds;

            //throughput
            Throughput = TotalParts / IdealParts;

            //quality
            Quality = (double)GoodParts / (double)TotalParts;

            //OEE
            OEE = Availability * Throughput * Quality;
        }

        public int TotalParts { get; set; }

        public int GoodParts { get; set; }

        public double IdealParts { get; set; }

        public double IdealCycleTime { get; set; }

        public double ScheduledSeconds { get; set; }

        public double RunningTime { get; set; }

        public double Downtime { get; set; }

        public double Throughput { get; set; }

        public double Availability { get; set; }

        public double Quality { get; set; }

        public double OEE { get; set; }
    }
}
