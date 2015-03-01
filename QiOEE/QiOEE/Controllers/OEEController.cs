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
            _qiData = new Client(_endpoint,_tenant,_tagPrefix);
            
        }

        [HttpGet]
        [Route("api/oee/today/{shift:int}")]
        public OEEModel GetToday(int shift)
        {
            return _qiData.GetOEEToday(shift);
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
