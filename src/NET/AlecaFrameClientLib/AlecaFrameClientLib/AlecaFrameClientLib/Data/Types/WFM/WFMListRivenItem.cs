using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types.WFM
{
	public class WFMListRivenItem
	{
		public List<WFMListRivenAttribute> attributes = new List<WFMListRivenAttribute>();

		public string weapon_url_name { get; set; }

		public string name { get; set; }

		public string type { get; set; }

		public int mastery_level { get; set; }

		public int mod_rank { get; set; }

		public int re_rolls { get; set; }

		public string polarity { get; set; }
	}
}
