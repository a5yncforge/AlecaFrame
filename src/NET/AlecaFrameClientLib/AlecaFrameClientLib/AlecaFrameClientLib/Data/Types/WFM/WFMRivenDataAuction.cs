using System;

namespace AlecaFrameClientLib.Data.Types.WFM
{
	public class WFMRivenDataAuction
	{
		public string note { get; set; }

		public int starting_price { get; set; }

		public bool _private { get; set; }

		public int? buyout_price { get; set; }

		public WFMRivenDataItem item { get; set; }

		public int minimal_reputation { get; set; }

		public bool visible { get; set; }

		public string platform { get; set; }

		public bool closed { get; set; }

		public object top_bid { get; set; }

		public object winner { get; set; }

		public object is_marked_for { get; set; }

		public object marked_operation_at { get; set; }

		public DateTime created { get; set; }

		public DateTime updated { get; set; }

		public string note_raw { get; set; }

		public bool is_direct_sell { get; set; }

		public string id { get; set; }
	}
}
