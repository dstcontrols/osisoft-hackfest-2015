using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackfestDataSimulator.SimulatorType;

namespace HackfestDataSimulator.Simulator
{
    public class Schedule
    {
        #region constants
        private const string TagName = "Scheduled_Availability";
        #endregion

        public QiPointSched Simulate(DateTime timeIndex)
        {
            var hour = timeIndex.Hour;

            if (hour >= 6 && hour < 15 && hour != 10)
            {
                var value = new QiPointSched() 
                { 
                    TimeStampId = timeIndex, 
                    IsGood = true,
                    Shift = 1,
                    ShiftName = "Shift 1" 
                };

                values.Add(value);
                return value;

            }
            else if (hour >= 15 && hour < 24 && hour != 19)
            {
                var value = new QiPointSched()
                {
                    TimeStampId = timeIndex,
                    IsGood = true,
                    Shift = 2,
                    ShiftName = "Shift 2"
                };
                values.Add(value);
                return value;
            }
            else
            {
                var value = new QiPointSched()
                {
                    TimeStampId = timeIndex,
                    IsGood = true,
                    Shift = 0,
                    ShiftName = "No Shift"
                };
                values.Add(value);
                return value;
            }
        }

        public void WriteToQi(QiClient.QiClient qi)
        {
            // filter the values
            var valuesCondensed = new List<QiPointSched>();

            valuesCondensed.Add(values[0]);

            for (int i = 0; i < values.Count -1; i++)
            {
                if (values[i + 1].Shift != values[i].Shift)
                {
                    valuesCondensed.Add(values[i + 1]);
                }
            }

            //write data ti qi
            var type = qi.BuildOrCreateType<QiPointSched>();
            var stream = qi.GetOrCreateStream(_tag, type);
            qi.WriteToStream(stream, valuesCondensed);
        }

        private void CreateTagName()
        {
            _tag = string.Format("{0}.{1}",_tagPrefix,TagName);
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

        private IList<QiPointSched> values = new List<QiPointSched>();
    }
}
