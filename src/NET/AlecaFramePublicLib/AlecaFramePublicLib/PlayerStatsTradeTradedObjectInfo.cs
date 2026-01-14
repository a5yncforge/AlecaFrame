using Newtonsoft.Json;

namespace AlecaFramePublicLib
{
	public class PlayerStatsTradeTradedObjectInfo
	{
		public string name { get; set; }

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string displayName { get; set; }

		public int cnt { get; set; }

		public int rank { get; set; }
	}
}
