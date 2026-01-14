namespace AlecaFrameClientLib.Data.Types
{
	public class Longgun : ModeableItem
	{
		public int UpgradeVer { get; set; }

		public int Polarized { get; set; }

		public Polarity1[] Polarity { get; set; }

		public string FocusLens { get; set; }

		public int Features { get; set; }

		public string UpgradeType { get; set; }

		public string ItemName { get; set; }

		public string[] ModularParts { get; set; }
	}
}
