﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackfestDataSimulator.SimulatorType
{
    public class QiPartCount
    {
        public DateTime TimeStampId { get; set; }
        public int Count { get; set; }
        public bool IsGood { get; set; }

        public override string ToString()
        {
            return string.Format("{1}   {0}", TimeStampId, Count);
        }
    }
}
