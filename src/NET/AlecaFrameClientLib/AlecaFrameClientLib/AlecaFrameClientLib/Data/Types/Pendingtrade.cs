namespace AlecaFrameClientLib.Data.Types
{
	public class Pendingtrade
	{
		public int State { get; set; }

		public bool SelfReady { get; set; }

		public bool BuddyReady { get; set; }

		public Getting Getting { get; set; }

		public int ClanTax { get; set; }

		public int Revision { get; set; }

		public Giving Giving { get; set; }

		public Itemid52 ItemId { get; set; }
	}
}
