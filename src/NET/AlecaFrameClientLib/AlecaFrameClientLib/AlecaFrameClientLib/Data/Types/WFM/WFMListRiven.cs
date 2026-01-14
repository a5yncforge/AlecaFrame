using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data.Types.WFM
{
	public class WFMListRiven
	{
		public bool visible;

		public int starting_price { get; set; }

		public int? buyout_price { get; set; }

		public int minimal_reputation { get; set; }

		[JsonProperty("private")]
		public bool _private { get; set; }

		public string note { get; set; }

		public WFMListRivenItem item { get; set; }
	}
}
