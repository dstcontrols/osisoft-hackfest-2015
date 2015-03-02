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
        private uint _tenant = 10000;
        private string _endpoint = "http://historiantest.cloudapp.net:3380";
        private string _tagPrefix = "P6-8";
        public QiQuery.Client _qiData = null;

        public OEEController()
        {
            //_qiData = new Client(_endpoint,_tenant,_tagPrefix);
            
        }

        [HttpGet]
        [Route("api/oee/today/{shift:int}")]
        public OEEModel GetToday(int shift)
        {
            var tdata = new OEEModel(DateTime.Now,DateTime.Now){OEE=.788,Availability=.879, Throughput=.935, Quality=.805};
            var sOEE = new List<OEEModel>();
            sOEE.Add(new OEEModel(DateTime.Now, DateTime.Now) { IdealParts = 1000, TotalParts = 800, GoodParts = 750 });
            sOEE.Add(new OEEModel(DateTime.Now, DateTime.Now) { IdealParts = 1000, TotalParts = 880, GoodParts = 790 });
            sOEE.Add(new OEEModel(DateTime.Now, DateTime.Now) { IdealParts = 1000, TotalParts = 650, GoodParts = 600 });
            tdata.SubOEEs = sOEE;
            return tdata;// _qiData.GetOEEToday(shift);
        }

        [HttpGet]
        [Route("api/oee/yesterday/{shift:int}")]
        public OEEModel GetYesterday(int shift)
        {
            return _qiData.GetOEEYesterday(shift);
        }

        [HttpGet]
        [Route("api/oee/week/{shift:int}")]
        public OEEModel GetWeek(int shift)
        {
            return _qiData.GetOEEWeek(shift);
        }

    }
}
