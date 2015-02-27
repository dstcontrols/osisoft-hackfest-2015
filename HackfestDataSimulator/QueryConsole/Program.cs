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

            var oee = client.GetOEE(DateTime.Now.AddHours(-5), DateTime.Now.AddHours(-1), 0);
        }
    }
}
