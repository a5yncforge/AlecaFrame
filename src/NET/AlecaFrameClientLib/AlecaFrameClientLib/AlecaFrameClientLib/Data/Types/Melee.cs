namespace AlecaFrameClientLib.Data.Types
{
	public class Melee : ModeableItem
	{
		public int UpgradeVer { get; set; }

		public int Features { get; set; }

		public string FocusLens { get; set; }

		public string[] ModularParts { get; set; }

		public string ItemName { get; set; }

		public Polarity3[] Polarity { get; set; }

		public int Polarized { get; set; }

		public string UpgradeType { get; set; }
	}
}
