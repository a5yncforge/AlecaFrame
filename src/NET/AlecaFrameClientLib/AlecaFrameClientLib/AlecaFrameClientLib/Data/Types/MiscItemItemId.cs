using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data.Types
{
	public class MiscItemItemId
	{
		[JsonProperty("$oid")]
		public string oid { get; set; }
	}
}
