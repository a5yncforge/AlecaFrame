namespace AlecaFrameClientLib.Data.Types
{
	public class SENTINEL
	{
		public string PresetIcon { get; set; }

		public bool Favorite { get; set; }

		public string n { get; set; }

		public S2 s { get; set; }

		public L2 l { get; set; }

		public Itemid17 ItemId { get; set; }
	}
	public class Sentinel : ModeableItem
	{
		public int UpgradeVer { get; set; }

		public int Features { get; set; }

		public Polarity5[] Polarity { get; set; }

		public int Polarized { get; set; }
	}
}
