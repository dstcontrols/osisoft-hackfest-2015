using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSIsoft.Qi;
using OSIsoft.Qi.Http;
using System.Threading.Tasks;
using HackfestDataSimulator.SimulatorType;
using System.Collections.Concurrent;

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

        public OEEModel GetOEE(DateTime startTime, DateTime endTime, int shift, int resolution)
        {
            var primaryModel = new OEEModel(startTime, endTime);

            var generalRunTimes = GetGeneralRunTimes(startTime, endTime, resolution);

            GetScheduledAvailability(primaryModel, shift);

            var tasks = new List<Task>();

            tasks.Add(Task.Run(() => { GetTotalParts(primaryModel); }));
            tasks.Add(Task.Run(() => { GetGoodParts(primaryModel); }));
            tasks.Add(Task.Run(() => { GetIdealCycleTime(primaryModel); }));
            tasks.Add(Task.Run(() => { GetMachineState(primaryModel); }));

            tasks.Add(Task.Run(() =>
            {
                var bag = new ConcurrentBag<OEEModel>();
                Parallel.ForEach(generalRunTimes, runTime =>
                    {
                        try
                        {
                            var model = new OEEModel(runTime.Item1, runTime.Item2);
                            bag.Add(model);

                            GetScheduledAvailability(model, shift);

                            var subTasks = new List<Task>();

                            subTasks.Add(Task.Run(() => { GetTotalParts(model); }));
                            subTasks.Add(Task.Run(() => { GetGoodParts(model); }));
                            subTasks.Add(Task.Run(() => { GetIdealCycleTime(model); }));
                            subTasks.Add(Task.Run(() => { GetMachineState(model); }));

                            Task.WaitAll(subTasks.ToArray());
                            model.Calculate();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    });

                primaryModel.SubOEEs = bag.ToList();
            }));

            Task.WaitAll(tasks.ToArray());

            primaryModel.Calculate();
            return primaryModel;
        }

        private IList<Tuple<DateTime, DateTime>> GetGeneralRunTimes(DateTime startTime, DateTime endTime, int resolution)
        {
            var spanHours = (endTime - startTime).TotalHours;
            var fullChunks = Math.Floor(spanHours / resolution);

            var results = new List<Tuple<DateTime, DateTime>>();

            for (int i = 0; i < fullChunks; i++)
            {
                results.Add(new Tuple<DateTime, DateTime>(startTime.AddHours(i * resolution), startTime.AddHours((i + 1) * resolution)));
            }

            if (results.Count == 0)
            {
                results.Add(new Tuple<DateTime, DateTime>(startTime, endTime));
            }
            else
            {
                results.Add(new Tuple<DateTime, DateTime>(results.Last().Item2, endTime));
            }

            return results;
        }

        private IList<Tuple<DateTime,DateTime>> GetRunTimes(DateTime startTime, DateTime endTime, int shift, int resolution)
        {
            var ranges = new List<Tuple<DateTime,DateTime>>();

            var server = GetQiServer();

            var schedAvail = server.GetWindowValues<QiPointSched>(sAvailStreamId, startTime.AddHours(-8).ToString("o"), endTime.AddHours(-8).ToString("o"), QiBoundaryType.Outside).ToList();
            schedAvail.First().TimeStampId = startTime;
            schedAvail.Last().TimeStampId = endTime;


            for (int i = 0; i < schedAvail.Count -1; i++)
            {
                var currentSch = schedAvail[i];
                var nextSch = schedAvail[i + 1];

                if (shift != 0)
                {
                    if (currentSch.Shift == shift)
                    {
                        ranges.Add(new Tuple<DateTime, DateTime>(currentSch.TimeStampId, nextSch.TimeStampId));
                    }
                }
                else
                {
                    if (currentSch.Shift != 0)
                    {
                        ranges.Add(new Tuple<DateTime, DateTime>(currentSch.TimeStampId, nextSch.TimeStampId));
                    }
                }
                
            }

            return ranges.Where(z => (z.Item2 - z.Item1).TotalSeconds != 0).SelectMany(j => SplitRangeIntoResolution(j, resolution)).ToList();
        }

        private IList<Tuple<DateTime,DateTime>> SplitRangeIntoResolution(Tuple<DateTime,DateTime> range, int resolution)
        {
            var result = new List<Tuple<DateTime, DateTime>>();

            var span = (range.Item2 - range.Item1).TotalHours;

            if ( span <= resolution)
            {
                result.Add(range);
            }
            else
            {
                var x = 0;
                for (int i = 0; i < Math.Floor(span) - 1; i++)
                {
                    var startTime = range.Item1.AddHours(i);
                    var endTime = range.Item1.AddHours(i + 1);
                    result.Add(new Tuple<DateTime, DateTime>(startTime, endTime));
                    x = i;
                }
                
                result.Add(new Tuple<DateTime,DateTime>(range.Item1.AddHours(x),range.Item2));
            }

            return result;
        }

        private void GetMachineState(OEEModel model)
        {
            try
            {
                var server = GetQiServer();

                var machineStates = server.GetWindowValues<QiPointMachineState>(mStateStreamId, model.StartTime.AddHours(-8).ToString("o"), model.EndTime.AddHours(-8).ToString("o"), QiBoundaryType.Outside).OrderBy(x => x.TimeStampId).ToList();

                machineStates[0].TimeStampId = model.StartTime;
                machineStates[machineStates.Count - 1].TimeStampId = model.EndTime;

                var totalRunningTime = 0.0;

                for (int i = 0; i < machineStates.Count - 1; i++)
                {
                    var currentMS = machineStates[i];
                    var nextMS = machineStates[i + 1];

                    var currentMSRange = model.Spans.Where(z => z.Item1 <= currentMS.TimeStampId && z.Item2 > currentMS.TimeStampId).Select(z => z).FirstOrDefault();
                    var nextMSRange = model.Spans.Where(z => z.Item1 <= nextMS.TimeStampId && z.Item2 >= nextMS.TimeStampId).Select(z => z).FirstOrDefault();

                    if (currentMSRange == null && nextMSRange != null)
                    {
                        currentMS.TimeStampId = nextMSRange.Item1;
                    }
                    else if (currentMSRange != null && nextMSRange == null)
                    {
                        nextMS.TimeStampId = currentMSRange.Item2;
                    }
                    else if (currentMSRange == null && nextMSRange == null)
                    {
                        continue;
                    }

                    if (currentMS.Running)
                    {
                        totalRunningTime += (nextMS.TimeStampId - currentMS.TimeStampId).TotalSeconds;
                    }
                }

                model.RunningTime = totalRunningTime;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GetScheduledAvailability(OEEModel model, int shift)
        {
            try
            {
                var server = GetQiServer();

                var schedAvail = server.GetWindowValues<QiPointSched>(sAvailStreamId, model.StartTime.AddHours(-8).ToString("o"), model.EndTime.AddHours(-8).ToString("o"), QiBoundaryType.Outside).OrderBy(x => x.TimeStampId).ToList();

                schedAvail[0].TimeStampId = model.StartTime;
                schedAvail[schedAvail.Count - 1].TimeStampId = model.EndTime;

                var scheduledSeconds = 0.0;

                model.Spans = new List<Tuple<DateTime, DateTime>>();

                for (int i = 0; i < schedAvail.Count - 1; i++)
                {
                    var currentSched = schedAvail[i];
                    var nextSched = schedAvail[i + 1];

                    if (currentSched.Shift != 0)
                    {
                        if (shift == 0 || currentSched.Shift == shift || currentSched.Shift == shift)
                        {
                            model.Spans.Add(new Tuple<DateTime, DateTime>(currentSched.TimeStampId, nextSched.TimeStampId));
                            var span = nextSched.TimeStampId - currentSched.TimeStampId;
                            scheduledSeconds += span.TotalSeconds;
                        }
                    }
                }

                model.Spans = model.Spans.Where(x => (x.Item2 - x.Item1).TotalSeconds >= 0).Select(y => y).ToList();

                model.ScheduledSeconds = scheduledSeconds;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GetIdealCycleTime(OEEModel model)
        {
            try
            {
                var server = GetQiServer();

                var cycleTimes = server.GetWindowValues<QiPointCycleTime>(cTimeStreamId, model.StartTime.AddHours(-8).ToString("o"), model.EndTime.AddHours(-8).ToString("o"), QiBoundaryType.Outside).OrderBy(x => x.TimeStampId).ToList();

                var totalIdealParts = 0.0;
                var totalSeconds = 0.0;

                for (int i = 0; i < cycleTimes.Count() - 1; i++)
                {
                    var currentCT = cycleTimes[i];
                    var nextCT = cycleTimes[i + 1];

                    var currentCTRange = model.Spans.Where(z => z.Item1 <= currentCT.TimeStampId && z.Item2 > currentCT.TimeStampId).Select(z => z).FirstOrDefault();
                    var nextCTRange = model.Spans.Where(z => z.Item1 <= nextCT.TimeStampId && z.Item2 >= nextCT.TimeStampId).Select(z => z).FirstOrDefault();

                    if (currentCTRange == null && nextCTRange != null)
                    {
                        currentCT.TimeStampId = nextCTRange.Item1;
                    }
                    else if (currentCTRange != null && nextCTRange == null)
                    {
                        nextCT.TimeStampId = currentCTRange.Item2;
                    }
                    else if (currentCTRange == null && nextCTRange == null)
                    {
                        continue;
                    }

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
            catch (Exception)
            {
                throw;
            }
        }

        private void GetGoodParts(OEEModel model)
        {
            try
            {
                var server = GetQiServer();

                var GoodPartValues = server.GetWindowValues<QiPartCount>(gPartStreamId, model.StartTime.AddHours(-8).ToString("o"), model.EndTime.AddHours(-8).ToString("o"), QiBoundaryType.Outside);

                var goodParts = 0;

                foreach (var range in model.Spans)
                {
                    var orderedParts = GoodPartValues.Where(x => x.TimeStampId >= range.Item1 && x.TimeStampId <= range.Item2).OrderBy(j => j.TimeStampId).Select(y => y);

                    if (orderedParts.Count() == 0)
                    {
                        continue;
                    }

                    var firstTCount = orderedParts.First().Count;
                    var lastTCount = orderedParts.Last().Count;

                    goodParts += lastTCount - firstTCount;
                }

                model.GoodParts = goodParts;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GetTotalParts(OEEModel model)
        {
            try
            {
                var server = GetQiServer();

                var totalPartValues = server.GetWindowValues<QiPartCount>(tPartStreamId, model.StartTime.AddHours(-8).ToString("o"), model.EndTime.AddHours(-8).ToString("o"), QiBoundaryType.Outside).ToList();

                var totalParts = 0;

                foreach (var range in model.Spans)
                {
                    var orderedParts = totalPartValues.Where(x => x.TimeStampId >= range.Item1 && x.TimeStampId <= range.Item2).OrderBy(j => j.TimeStampId).Select(y => y);

                    if (orderedParts.Count() == 0)
                    {
                        continue;
                    }

                    var firstTCount = orderedParts.First().Count;
                    var lastTCount = orderedParts.Last().Count;

                    totalParts += lastTCount - firstTCount;
                }

                model.TotalParts = totalParts;
            }
            catch (Exception)
            {
                throw;
            }
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

        public IQiServer _server { get; set; }
        #endregion

        public OEEModel GetOEEWeek(int shift)
        {
            var startOfWeek = DateTime.Now.Date.AddDays(-6);
            var endOfWeek = DateTime.Now;

            return GetOEE(startOfWeek, endOfWeek, shift, 24);
        }

        public OEEModel GetOEEToday(int shift)
        {
            var startOfDay = DateTime.Now.Date;
            var endOfDay = DateTime.Now;

            return GetOEE(startOfDay, endOfDay, shift, 1);
        }

        public OEEModel GetOEEYesterday(int shift)
        {
            var startOfDay = DateTime.Now.Date.AddDays(-1);
            var endOfDay = DateTime.Now.Date;

            return GetOEE(startOfDay, endOfDay, shift, 1);
        }
    }
}
