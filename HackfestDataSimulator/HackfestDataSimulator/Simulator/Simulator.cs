using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackfestDataSimulator.Simulator
{
    class Simulator
    {
        public Simulator(string tagPrefix)
        {
            _sched = new Schedule() { TagPrefix = tagPrefix };
            _machine = new MachineState() { TagPrefix = tagPrefix };
            _parts = new parts() { TagPrefix = tagPrefix };
        }

        public void Simulate(DateTime timeIndex)
        {
            var schedule = _sched.Simulate(timeIndex);

            if (schedule.Shift == 0)
            {
                _machine.CreateIdleState(timeIndex);
                _parts.CreateStoppedMachine(timeIndex);
            }
            else
            {
                var stateResult = _machine.Simulate(timeIndex);
                if (stateResult.Running)
                {
                    _parts.Simulate(timeIndex);
                }
                else
                {
                    _parts.CreateStoppedMachine(timeIndex);
                }
            }
        }

        internal void Commit(QiClient.QiClient qi)
        {
            _sched.WriteToQi(qi);
            _machine.WriteToQi(qi);
            _parts.WriteToQi(qi);
        }

        public Schedule _sched { get; set; }

        public MachineState _machine { get; set; }

        public parts _parts { get; set; }
    }
}
