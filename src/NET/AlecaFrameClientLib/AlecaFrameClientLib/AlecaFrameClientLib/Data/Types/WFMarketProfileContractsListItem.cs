namespace AlecaFrameClientLib.Data.Types
{
	public class WFMarketProfileContractsListItem
	{
		public int re_rolls { get; set; }

		public int mod_rank { get; set; }

		public string weapon_url_name { get; set; }

		public WFMarketProfileContractsListAttribute[] attributes { get; set; }

		public string name { get; set; }

		public int mastery_level { get; set; }

		public string polarity { get; set; }

		public string type { get; set; }
	}
}
