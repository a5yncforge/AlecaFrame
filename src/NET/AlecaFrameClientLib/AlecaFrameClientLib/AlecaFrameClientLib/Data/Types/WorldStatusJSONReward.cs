namespace AlecaFrameClientLib.Data.Types
{
	public class WorldStatusJSONReward
	{
		public object[] items { get; set; }

		public WorldStatusJSONCounteditem1[] countedItems { get; set; }

		public int credits { get; set; }

		public string asString { get; set; }

		public string itemString { get; set; }

		public string thumbnail { get; set; }

		public int color { get; set; }
	}
}
