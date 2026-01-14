using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data.Types
{
	public class Created
	{
		[JsonProperty("$date")]
		public WarframeWorldStateDate date { get; set; }
	}
}
