using QiOEE.Services;
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
        // GET: api/OEE
        public IEnumerable<int> Get()
        {
            int[] data = { 1, 2, 3, 4,6,7,8,9,10,11,6,3 };

            return data;// QiService.DataTest();
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
