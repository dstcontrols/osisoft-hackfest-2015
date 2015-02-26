using HackfestDataSimulator.SimulatorType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackfestDataSimulator.Simulator
{
    public class MachineState
    {
        public MachineState()
        {
            _random = new Random();
        }

        internal QiPointMachineState CreateIdleState(DateTime timeIndex)
        {
            var value = new QiPointMachineState()
            {
                IsGood = true,
                StateNum = 0,
                State = "Idle",
                TimeStampId = timeIndex,
                Running = false
            };
            values.Add(value);
            return value;
        }

        internal QiPointMachineState Simulate(DateTime timeIndex)
        {
            var change = _random.Next(101) % 12;

            if (change == 0)
            {
                return CreateNewMachineState(timeIndex);
            }
            else
            {
                return values.Last();
            }
        }

        private QiPointMachineState CreateNewMachineState(DateTime timeIndex)
        {
            var num = _random.Next(100);

            if (num <= 80)
            {
                var value = new QiPointMachineState()
                {
                    IsGood = true,
                    StateNum = 1,
                    State = "Running",
                    TimeStampId = timeIndex,
                    Running = true
                };
                values.Add(value);
                return value;
            }
            else if (num <= 85)
            {
                var value = new QiPointMachineState()
                {
                    IsGood = true,
                    StateNum = 2,
                    State = "E-Stop",
                    TimeStampId = timeIndex,
                    Running = false
                };
                values.Add(value);
                return value;
            }
            else if (num <= 95)
            {
                var value = new QiPointMachineState()
                {
                    IsGood = true,
                    StateNum = 3,
                    State = "Changeover",
                    TimeStampId = timeIndex,
                    Running = false
                };
                values.Add(value);
                return value;
            }
            else
            {
                return CreateIdleState(timeIndex);
            }
        }

        public void WriteToQi(QiClient.QiClient qi)
        {
            // filter the values
            var valuesCondensed = new List<QiPointMachineState>();

            valuesCondensed.Add(values[0]);

            for (int i = 0; i < values.Count - 1; i++)
            {
                if (values[i + 1].StateNum != values[i].StateNum)
                {
                    valuesCondensed.Add(values[i + 1]);
                }
            }

            //write data ti qi
            var type = qi.BuildOrCreateType<QiPointMachineState>();
            var stream = qi.GetOrCreateStream(_tag, type);
            qi.WriteToStream(stream, valuesCondensed);
        }

        private const string TagName = "Machine_State";

        private void CreateTagName()
        {
            _tag = string.Format("{0}.{1}", _tagPrefix, TagName);
        }

        private string _tag = string.Empty;

        private string _tagPrefix = "Poucher";
        public string TagPrefix
        {
            get
            {
                return _tagPrefix;
            }
            set
            {
                _tagPrefix = value;
                CreateTagName();
            }
        }

        private IList<QiPointMachineState> values = new List<QiPointMachineState>();

        public Random _random { get; set; }
    }
}
