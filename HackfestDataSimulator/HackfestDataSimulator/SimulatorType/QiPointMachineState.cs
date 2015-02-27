using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackfestDataSimulator.SimulatorType
{
    public class QiPointMachineState
    {
        public DateTime TimeStampId { get; set; }
        public int StateNum { get; set; }
        public string State { get; set; }
        public bool Running { get; set; }
        public bool IsGood { get; set; }

        public override string ToString()
        {
            return string.Format("{1}   {0}", TimeStampId, StateNum);
        }
    }
}
