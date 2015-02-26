using HackfestDataSimulator.SimulatorType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackfestDataSimulator.Simulator
{
	public class parts
	{
		public parts()
		{
			_random = new Random();
			totalParts = 0;
			goodParts = 0;
		}

		internal void CreateStoppedMachine(DateTime timeIndex)
		{
			var value = new QiPointCycleTime()
			{
				TimeStampId = timeIndex,
				IsGood = true,
				CycleTime = 0
			};
			rateValues.Add(value);

			var totalValue = new QiPartCount()
			{
				IsGood = true,
				TimeStampId = timeIndex,
				Count = totalParts
			};
			totalPartValues.Add(totalValue);

			var goodValue = new QiPartCount()
			{
				IsGood = true,
				TimeStampId = timeIndex,
				Count = goodParts
			};
			goodPartValues.Add(goodValue);
		}

		internal void Simulate(DateTime timeIndex)
		{
			//pick cycle time
			var cycleTime = GenerateCycleTime(timeIndex);
			rateValues.Add(cycleTime);

			GeneratePartCounts(cycleTime.CycleTime, timeIndex);

		}

		private void GeneratePartCounts(double idealCycleTime, DateTime timeIndex)
		{
			var idealParts = idealCycleTime * 5;

			var chance = _random.Next(4);
			var tParts = 0;
			switch (chance)
			{
				case 0:
					tParts = (int)idealParts + 1;
					break;
				case 1:
					tParts = (int)idealParts;
					break;
				case 2:
					tParts = (int)idealParts - 1;
					break;
				case 3:
					tParts = (int)idealParts - 2;
					break;
				default:
					tParts = (int)idealParts - 1;
					break;
			}

			if (tParts < 0 || idealParts == 0) tParts = 0;

			totalParts = totalParts + tParts;

			var tPartValue = new QiPartCount()
			{
				Count = totalParts,
				IsGood = true,
				TimeStampId = timeIndex
			};
			totalPartValues.Add(tPartValue);

			var qChance = _random.Next(4);
			var gParts = 0;

			switch (qChance)
			{
				case 0:
				case 1:
					gParts = tParts;
					break;
				case 2:
					gParts = tParts - 1;
					break;
				case 3:
					gParts = tParts - 2;
					break;
				default:
					gParts = 0;
					break;
			}

			if (gParts < 0 || idealParts == 0) gParts = 0;

			goodParts = goodParts + gParts;

			var gPartValue = new QiPartCount()
			{
				Count = goodParts,
				TimeStampId = timeIndex,
				IsGood = true
			};

		}

		private QiPointCycleTime GenerateCycleTime(DateTime timeIndex)
		{
			var change = _random.Next(101);

			if (change <= 1)
			{
				return new QiPointCycleTime()
				{
					CycleTime = 1,
					IsGood= true,
					TimeStampId = timeIndex
				};
			}
			else if (change <= 2)
			{
				return new QiPointCycleTime()
				{
					CycleTime = 2,
					IsGood = true,
					TimeStampId = timeIndex
				};
			}
			else
			{
				return rateValues.LastOrDefault();
			}

		}

		private void CreateTagNames()
		{
			_rateTag = string.Format("{0}.{1}", _tagPrefix, RateTagName);
			_goodTag = string.Format("{0}.{1}", _tagPrefix, GoodTagName);
			_totalTag = string.Format("{0}.{1}", _tagPrefix, TotalTagName);
		}

		private string _rateTag = string.Empty;
		private string _goodTag = string.Empty;
		private string _totalTag = string.Empty;

		private string _tagPrefix = "Poucher";
		public string TagPrefix
		{
			get
			{
				return _tagPrefix;
			}
			set
			{
				_tagPrefix = value;
				CreateTagNames();
			}
		}

		private IList<QiPointCycleTime> rateValues = new List<QiPointCycleTime>();
		private IList<QiPartCount> totalPartValues = new List<QiPartCount>();
		private IList<QiPartCount> goodPartValues = new List<QiPartCount>();

		private const string RateTagName = "Ideal_CycleTime";
		private const string GoodTagName = "Good_Parts";
		private const string TotalTagName = "Total_Parts";

		private int totalParts;
		private int goodParts;
		private Random _random;

		internal void WriteToQi(QiClient.QiClient qi)
		{
			//////////////////////////////////////////
			// filter the values
			var cTimesCondensed = new List<QiPointCycleTime>();

			cTimesCondensed.Add(rateValues[0]);

			for (int i = 0; i < rateValues.Count - 1; i++)
			{
				if (rateValues[i + 1].CycleTime != rateValues[i].CycleTime)
				{
					cTimesCondensed.Add(rateValues[i + 1]);
				}
			}

			//write data ti qi
			qi.WriteToStream(qi.GetOrCreateStream(_rateTag, qi.BuildOrCreateType<QiPointCycleTime>()), cTimesCondensed);

			//////////////////////////////////////////
			// filter the values
			var tPartsCondensed = new List<QiPartCount>();

			tPartsCondensed.Add(totalPartValues[0]);

			for (int i = 0; i < totalPartValues.Count - 1; i++)
			{
				if (totalPartValues[i + 1].Count != totalPartValues[i].Count)
				{
					tPartsCondensed.Add(totalPartValues[i + 1]);
				}
			}

			//write data ti qi
			qi.WriteToStream(qi.GetOrCreateStream(_totalTag, qi.BuildOrCreateType<QiPartCount>()), tPartsCondensed);

			//////////////////////////////////////////////
			// filter the values
			var gPartsCondensed = new List<QiPartCount>();

			gPartsCondensed.Add(goodPartValues[0]);

			for (int i = 0; i < goodPartValues.Count - 1; i++)
			{
				if (goodPartValues[i + 1].Count != goodPartValues[i].Count)
				{
					gPartsCondensed.Add(goodPartValues[i + 1]);
				}
			}

			//write data ti qi
			qi.WriteToStream(qi.GetOrCreateStream(_goodTag, qi.BuildOrCreateType<QiPartCount>()), gPartsCondensed);
		}

	}
}
