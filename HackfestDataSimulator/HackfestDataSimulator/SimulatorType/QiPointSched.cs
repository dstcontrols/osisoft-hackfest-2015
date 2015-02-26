using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackfestDataSimulator.SimulatorType
{
    public class QiPointSched
    {
        public DateTime TimeStampId { get; set; }
        public int Shift { get; set; }
        public string ShiftName { get; set; }
        public bool IsGood { get; set; }

        public override string ToString()
        {
            return string.Format("{1}   {0}", TimeStampId,ShiftName);
        }
    }
}
