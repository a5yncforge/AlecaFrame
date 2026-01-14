namespace AlecaFrameClientLib.Data.Types
{
	public class Details
	{
		public string Name { get; set; }

		public bool IsPuppy { get; set; }

		public bool HasCollar { get; set; }

		public int Loyalty { get; set; }

		public float Integrity { get; set; }

		public int InteractionBonusCap { get; set; }

		public int MaintenanceCap { get; set; }

		public int PrintsRemaining { get; set; }

		public Lastinteractiondate LastInteractionDate { get; set; }

		public string Status { get; set; }

		public Hatchdate HatchDate { get; set; }

		public Dominanttraits DominantTraits { get; set; }

		public Recessivetraits RecessiveTraits { get; set; }

		public bool IsMale { get; set; }

		public float Size { get; set; }
	}
}
