using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AlecaFramePublicLib
{
	public class PlayerStatsTrade
	{
		public DateTime ts { get; set; }

		public List<PlayerStatsTradeTradedObjectInfo> tx { get; set; } = new List<PlayerStatsTradeTradedObjectInfo>();

		public List<PlayerStatsTradeTradedObjectInfo> rx { get; set; } = new List<PlayerStatsTradeTradedObjectInfo>();

		public string user { get; set; }

		public TradeClassification type { get; set; }

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public int? totalPlat { get; set; }

		public override string ToString()
		{
			return $"ts: {ts}, tx: {tx.Count}, rx: {rx.Count}, user: {user}";
		}
	}
}
