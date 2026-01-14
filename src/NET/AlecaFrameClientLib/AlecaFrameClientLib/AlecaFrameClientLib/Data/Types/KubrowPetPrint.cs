namespace AlecaFrameClientLib.Data.Types
{
	public class KubrowPetPrint : Miscitem
	{
		public string Name { get; set; }

		public bool IsMale { get; set; }

		public float Size { get; set; }

		public Dominanttraits DominantTraits { get; set; }

		public Recessivetraits RecessiveTraits { get; set; }
	}
}
