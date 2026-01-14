namespace AlecaFramePublicLib
{
	public class RivenSimilarityResponseRivenAttribute
	{
		public float val { get; set; }

		public string name { get; set; }

		public bool positive { get; set; }

		public bool matches { get; set; }

		public RivenSimilarityResponseRivenAttribute()
		{
		}

		public RivenSimilarityResponseRivenAttribute(AFDBRivenDataPointAttribute attribute, bool matchesRequest)
		{
			val = attribute.val;
			name = attribute.name;
			positive = attribute.positive;
			matches = matchesRequest;
		}
	}
}
