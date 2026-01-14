namespace AlecaFrameClientLib.Data.Types
{
	public class Kubrowpet : ModeableItem
	{
		public int UpgradeVer { get; set; }

		public Details Details { get; set; }

		public int Polarized { get; set; }

		public Polarity6[] Polarity { get; set; }

		public int Features { get; set; }

		public Infestationdate1 InfestationDate { get; set; }

		public float InfestationDays { get; set; }

		public string InfestationType { get; set; }
	}
}
