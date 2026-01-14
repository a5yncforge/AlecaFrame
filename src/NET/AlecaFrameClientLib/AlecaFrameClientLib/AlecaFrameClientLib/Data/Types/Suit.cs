namespace AlecaFrameClientLib.Data.Types
{
	public class Suit : ModeableItem
	{
		public int UpgradeVer { get; set; }

		public Infestationdate InfestationDate { get; set; }

		public int Polarized { get; set; }

		public Polarity[] Polarity { get; set; }

		public int Features { get; set; }

		public string FocusLens { get; set; }

		public SuitArchonCrystalUpgrades[] ArchonCrystalUpgrades { get; set; }
	}
}
