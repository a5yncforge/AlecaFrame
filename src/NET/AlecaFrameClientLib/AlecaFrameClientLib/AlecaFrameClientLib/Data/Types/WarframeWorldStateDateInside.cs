using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateDateInside
	{
		[JsonProperty("$date")]
		public WarframeWorldStateDate date { get; set; }
	}
}
