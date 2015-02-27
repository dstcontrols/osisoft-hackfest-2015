using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace QiOEE.Services
{
    public static class QiService
    {
        static public string DataTest()
        {

            string url = " http://historiandev.cloudapp.net:3380/Qi/Streams/p6-1.scheduled_availability/Data/GetWindowValues?startIndex=2-25-2015&endIndex=2-27-2015";
            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(url);
            authRequest.Method = "GET";
            authRequest.ContentType = "application/json";
            authRequest.Headers["QiTenant"] = "10000";

            try
            {
                var response = (HttpWebResponse)authRequest.GetResponse();

                string result = "null";

                using (Stream stream = response.GetResponseStream())
                {

                    StreamReader sr = new StreamReader(stream);
                    result = sr.ReadToEnd();
                    sr.Close();

                }
                return result;


            }
            catch (Exception e)
            {
                Console.WriteLine();
                System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.ToString());
                throw (e);
            }
        }
    }

}