namespace AlecaFrameClientLib.Data.Types
{
	public class Drop
	{
		public DataRelic relic;

		public string location { get; set; }

		public string type { get; set; }

		public string rarity { get; set; }

		public float? chance { get; set; }

		public string rotation { get; set; }

		public bool IsRelic()
		{
			return relic != null;
		}
	}
}
