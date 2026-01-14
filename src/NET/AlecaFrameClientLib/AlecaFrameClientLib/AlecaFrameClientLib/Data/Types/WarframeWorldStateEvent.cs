namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateEvent
	{
		public MiscItemItemId _id { get; set; }

		public WarframeWorldStateDateMessage[] Messages { get; set; }

		public string Prop { get; set; }

		public WarframeWorldStateDateInside Date { get; set; }

		public string Icon { get; set; }

		public bool Priority { get; set; }

		public bool MobileOnly { get; set; }

		public bool Community { get; set; }

		public string ImageUrl { get; set; }

		public WarframeWorldStateDateLink[] Links { get; set; }

		public WarframeWorldStateDateInside EventEndDate { get; set; }
	}
}
