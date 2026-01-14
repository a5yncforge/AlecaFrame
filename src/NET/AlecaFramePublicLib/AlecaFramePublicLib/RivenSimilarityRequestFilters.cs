namespace AlecaFramePublicLib
{
	public class RivenSimilarityRequestFilters
	{
		public int minPrice { get; set; }

		public int maxPrice { get; set; } = 10000000;

		public int minSimilarity { get; set; }

		public int minRerolls { get; set; }

		public int maxRerolls { get; set; } = 10000000;

		public bool negativeRequired { get; set; }
	}
}
