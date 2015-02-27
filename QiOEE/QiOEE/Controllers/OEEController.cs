using QiOEE.Services;
using QiQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QiOEE.Controllers
{
    public class OEEController : ApiController
    {
        public static uint Tenant = 10000;
        public static string Endpoint = "http://historiandev.cloudapp.net:3380";
        public static string TagPrefix = "P6-8";

        public QiQuery.Client QiData { get; set; }
        public OEEController()
        {
            QiData = new Client(Endpoint,Tenant,TagPrefix);
            
        }
        // GET: api/OEE
        public OEEModel Get()
        {
            int[] data = { 1, 2, 3, 4,6,7,8,9,10,11,6,3 };
            var testData = new QiQuery.OEEModel(DateTime.Now.AddHours(-5), DateTime.Now.AddHours(-1));
            testData.Availability = 60.50;
            testData.Throughput = 94.34;
            testData.Quality = 91.35;
            testData.OEE = testData.Availability / 100 * testData.Throughput / 100 * testData.Quality / 100 * 100;
            return testData;// QiData.GetOEE(DateTime.Now.AddHours(-5), DateTime.Now.AddHours(-1), 0);// QiService.DataTest();
        }

        // GET: api/OEE/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/OEE
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/OEE/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/OEE/5
        public void Delete(int id)
        {
        }
    }
}
