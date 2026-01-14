using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONVoidtrader
	{
		public string id { get; set; }

		public DateTime activation { get; set; }

		public string startString { get; set; }

		public DateTime expiry { get; set; }

		public bool active { get; set; }

		public string character { get; set; }

		public string location { get; set; }

		public object[] inventory { get; set; }

		public string psId { get; set; }

		public string endString { get; set; }

		public DateTime initialStart { get; set; }

		public object[] schedule { get; set; }
	}
}
