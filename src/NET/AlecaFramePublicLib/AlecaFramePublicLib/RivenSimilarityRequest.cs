namespace AlecaFramePublicLib
{
	public class RivenSimilarityRequest
	{
		public string weaponURLName { get; set; }

		public RivenSimilarityRequestAttribute[] attrs { get; set; }

		public RivenSimilarityRequestFilters filters { get; set; }

		public RivenSimilaritySource source { get; set; }
	}
}
