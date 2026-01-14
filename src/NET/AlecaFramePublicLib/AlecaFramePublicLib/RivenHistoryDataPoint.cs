using System;
using System.Collections.Generic;

namespace AlecaFramePublicLib
{
	public class RivenHistoryDataPoint
	{
		public double price { get; set; }

		public double basePrice { get; set; }

		public int samples { get; set; }

		public int daysAveraged { get; set; }

		public DateTime endts { get; set; }

		public Dictionary<string, double> attrPriceAvg { get; set; }

		public Dictionary<string, double> attrPopulatity { get; set; }

		public double omegaAtt { get; set; }

		public double tradeVolume { get; set; }
	}
}
