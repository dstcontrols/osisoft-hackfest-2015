using OSIsoft.Qi;
using OSIsoft.Qi.Http;
using OSIsoft.Qi.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackfestDataSimulator.QiClient
{
    public class QiClient
    {
        public QiClient(uint tenant, string endpoint)
        {
            // create tenant, endpoint, factory
            _tennant = new QiTenant(tenant);
            _uri = new Uri(endpoint);
            _factory = new QiHttpClientFactory<IQiServer>();
            _factory.OnCreated(x =>
                {
                    x.DefaultRequestHeaders.Add("QiTenant", _tennant.Id.ToString());
                });

            //get the server
            _server = _factory.CreateChannel(_uri);
            _server.PostTenant(_tennant);

            //get type builder
            _typeBuilder = new QiTypeBuilder();
        }

        public QiType BuildOrCreateType<T>() where T : new()
        {
            T temp = new T();
            QiType qiType = null;
            var success = _typeBuilder.Library.TryGetQiType(temp.GetType(),out qiType);

            if (!success)
            {
                qiType = _typeBuilder.Create<T>();
                _server.PostType(qiType);
            }

            return qiType;
        }

        public QiStream GetOrCreateStream(string streamName, QiType type)
        {
            try
            {
                return _server.GetStream(streamName);
            }
            catch (Exception)
            {
                var stream = new QiStream()
                {
                    Id = streamName,
                    TypeId = type.Id
                };

                return _server.PostStream(stream);
            }
        }

        internal void WriteToStream<T>(QiStream stream, IList<T> values)
        {
            _server.UpdateValues<T>(stream.Id, values);
        }

        private QiTenant _tennant { get; set; }
        private Uri _uri { get; set; }
        private QiHttpClientFactory<IQiServer> _factory { get; set; }
        private IQiServer _server { get; set; }
        private QiTypeBuilder _typeBuilder { get; set; }

        
    }
}
