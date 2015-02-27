using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSIsoft.Qi;
using OSIsoft.Qi.Http;
using System.Threading.Tasks;
using HackfestDataSimulator.SimulatorType;

namespace QiQuery
{
    public class Client
    {
        #region Fields
        private QiTenant _tennant;
        private Uri _uri;
        private string _tagPrefix;
        #endregion

        public Client(string endpoint, uint tenant, string tagPrefix)
        {
            _tagPrefix = tagPrefix;

            // create tenant, endpoint, factory
            _tennant = new QiTenant(tenant);
            _uri = new Uri(endpoint);


            var server = GetQiServer();

            GetStreams();
        }

        public OEEModel GetOEE(DateTime startTime, DateTime endTime)
        {
            var model = new OEEModel(startTime, endTime);

            var tasks = new List<Task>();

            tasks.Add(Task.Run(() => { GetTotalParts(model); }));
            tasks.Add(Task.Run(() => { GetGoodParts(model); }));
            tasks.Add(Task.Run(() => { GetIdealCycleTime(model); }));
            tasks.Add(Task.Run(() => { GetScheduledAvailability(model); }));
            tasks.Add(Task.Run(() => { GetMachineState(model); }));

            Task.WaitAll(tasks.ToArray());

            model.Calculate();
            return model;
        }

        private void GetMachineState(OEEModel model)
        {
            var server = GetQiServer();

            var machineStates = server.GetWindowValues<QiPointMachineState>(mStateStreamId,model.StartTime.ToString("o"), model.EndTime.ToString("o"), QiBoundaryType.Outside).OrderBy(x => x.TimeStampId).ToList();

            machineStates[0].TimeStampId = model.StartTime.ToUniversalTime();
            machineStates[machineStates.Count - 1].TimeStampId = model.EndTime.ToUniversalTime();

            var totalRunningTime = 0.0;

            for (int i = 0; i < machineStates.Count -1; i++)
            {
                var currentMS = machineStates[i];
                var nextMS = machineStates[i + 1];

                if (currentMS.Running)
                {
                    totalRunningTime += (nextMS.TimeStampId - currentMS.TimeStampId).TotalSeconds;
                }
            }

            model.RunningTime = totalRunningTime;

        }

        private void GetScheduledAvailability(OEEModel model)
        {
            var server = GetQiServer();

            var schedAvail = server.GetWindowQuery<QiPointSched>(sAvailStreamId, model.StartTime.ToString("o"), model.EndTime.ToString("o"), QiBoundaryType.Outside).OrderBy(x => x.TimeStampId).ToList();

            schedAvail[0].TimeStampId = model.StartTime.ToUniversalTime();
            schedAvail[schedAvail.Count - 1].TimeStampId = model.EndTime.ToUniversalTime();

            var scheduledSeconds = 0.0;

            for (int i = 0; i < schedAvail.Count - 1; i++)
            {
                var currentSched = schedAvail[i];
                var nextSched = schedAvail[i + 1];

                if (currentSched.Shift != 0)
                {
                    var span = nextSched.TimeStampId - currentSched.TimeStampId;
                    scheduledSeconds += span.TotalSeconds;
                }
            }

            model.ScheduledSeconds = scheduledSeconds;
        }

        private void GetIdealCycleTime(OEEModel model)
        {
            var server = GetQiServer();

            var cycleTimes = server.GetWindowQuery<QiPointCycleTime>(cTimeStreamId, model.StartTime.ToString("o"), model.EndTime.ToString("o"), QiBoundaryType.Outside).OrderBy(x => x.TimeStampId).ToList();

            var totalIdealParts = 0.0;
            var totalSeconds = 0.0;

            for (int i = 0; i < cycleTimes.Count() - 1; i++)
            {
                var currentCT = cycleTimes[i];
                var nextCT = cycleTimes[i + 1];

                var span = nextCT.TimeStampId - currentCT.TimeStampId;

                if (currentCT.CycleTime != 0.0)
                {
                    totalSeconds = totalSeconds + span.TotalSeconds;
                    totalIdealParts = totalIdealParts + (span.TotalSeconds * currentCT.CycleTime);
                }
            }

            model.IdealCycleTime = totalIdealParts / totalSeconds;
            model.IdealParts = totalIdealParts;
        }

        private void GetGoodParts(OEEModel model)
        {
            var server = GetQiServer();

            var GoodPartValues = server.GetWindowQuery<QiPartCount>(gPartStreamId, model.StartTime.ToString("o"), model.EndTime.ToString("o"), QiBoundaryType.Outside);

            var sGoodParts = GoodPartValues.Where(x => x.TimeStampId <= model.StartTime.ToUniversalTime()).OrderByDescending(y => y.TimeStampId).First().Count;
            var eGoodParts = GoodPartValues.Where(x => x.TimeStampId >= model.EndTime.ToUniversalTime()).OrderBy(y => y.TimeStampId).First().Count;

            model.GoodParts = eGoodParts - sGoodParts;
        }

        private void GetTotalParts(OEEModel model)
        {
            var server = GetQiServer();

            var totalPartValues = server.GetWindowQuery<QiPartCount>(tPartStreamId, model.StartTime.ToString("o"), model.EndTime.ToString("o"), QiBoundaryType.Outside);

            var sTotalParts = totalPartValues.Where(x => x.TimeStampId <= model.StartTime.ToUniversalTime()).OrderByDescending(y => y.TimeStampId).First().Count;
            var eTotalParts = totalPartValues.Where(x => x.TimeStampId >= model.EndTime.ToUniversalTime()).OrderBy(y => y.TimeStampId).First().Count;

            model.TotalParts = eTotalParts - sTotalParts;
        }

        public IQiServer GetQiServer()
        {
            if (_server == null)
            {
                var _factory = new QiHttpClientFactory<IQiServer>();
                _factory.OnCreated(x =>
                {
                    x.DefaultRequestHeaders.Add("QiTenant", _tennant.Id.ToString());
                });

                //get the server
                _server = _factory.CreateChannel(_uri);
                _server.PostTenant(_tennant);
                _factory.Dispose();
            }
             
            return _server;
        }

        private void GetStreams()
        {
            tPartStreamId = string.Format("{0}.{1}", _tagPrefix, "Total_Parts");
            gPartStreamId = string.Format("{0}.{1}", _tagPrefix, "Good_Parts");
            cTimeStreamId = string.Format("{0}.{1}", _tagPrefix, "Ideal_CycleTime");
            mStateStreamId = string.Format("{0}.{1}", _tagPrefix, "Machine_State");
            sAvailStreamId = string.Format("{0}.{1}", _tagPrefix, "Scheduled_Availability");
        }

        #region properties
        public string tPartStreamId { get; set; }

        public string gPartStreamId { get; set; }

        public string cTimeStreamId { get; set; }

        public string mStateStreamId { get; set; }

        public string sAvailStreamId { get; set; }
        #endregion



        public IQiServer _server { get; set; }
    }
}
