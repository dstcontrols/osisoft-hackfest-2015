using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.Qi;
using Qi = HackfestDataSimulator.QiClient;
using HackfestDataSimulator.Simulator;

namespace HackfestDataSimulator
{
    class Program
    {
        #region Constants
        public static uint Tenant = 10000;
        public static string Endpoint = "http://historiandev.cloudapp.net:3380";

        #endregion

        static void Main(string[] args)
        {
            var qi = new Qi.QiClient(Tenant, Endpoint);

            var SimulationStart = new DateTime(2015, 2, 20);
            var SimulationEnd = new DateTime(2015, 3, 1);

            //create simulators
            var simulator = new Simulator.Simulator("P6-8");

            //simulate times
            var timeIndex = SimulationStart;

            while (timeIndex <= SimulationEnd)
            {
                simulator.Simulate(timeIndex);

                timeIndex = timeIndex.AddSeconds(5);
            }

            //write to Stream
            simulator.Commit(qi);

        }
    }
}
