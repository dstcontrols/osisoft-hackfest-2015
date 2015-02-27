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
        public string Get()
        {
            return QiService.DataTest();
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
