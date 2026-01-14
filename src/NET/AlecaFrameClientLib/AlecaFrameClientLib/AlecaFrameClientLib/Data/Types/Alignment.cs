using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data.Types
{
	public class Alignment
	{
		public int Wisdom { get; set; }

		[JsonProperty("Alignment")]
		public float AlignmentAA { get; set; }
	}
}
