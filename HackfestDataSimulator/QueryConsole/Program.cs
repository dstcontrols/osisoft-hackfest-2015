using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QiQuery;

namespace QueryConsole
{
    class Program
    {
        #region Constants
        public static uint Tenant = 10000;
        public static string Endpoint = "http://historiandev.cloudapp.net:3380";
        public static string TagPrefix = "P6-8";
        #endregion

        static void Main(string[] args)
        {
            var client = new QiQuery.Client(Endpoint,Tenant,TagPrefix);

            //var oee = client.GetOEE(DateTime.Now.Date, DateTime.Now.AddDays(1).Date, 0, 1);
            //week queryies
            var oeeWeek = client.GetOEEWeek(0);
            var oeeWeekShift1 = client.GetOEEWeek(1);
            var oeeWeekShift2 = client.GetOEEWeek(2);

            //today query
            var OEEToday = client.GetOEEToday(0);
            var OEETodayShift1 = client.GetOEEToday(1);
            var OEETodayShift2 = client.GetOEEToday(2);

            //var oeeToday = client.GetOEEToday();
            var OEEYesterday = client.GetOEEYesterday(0);
            var OEEYesterdayShift1 = client.GetOEEYesterday(1);
            var OEEYesterdayShift2 = client.GetOEEYesterday(2);
        
        }
    }
}
