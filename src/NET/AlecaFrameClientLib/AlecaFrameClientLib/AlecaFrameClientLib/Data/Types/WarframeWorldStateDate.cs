using System;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeWorldStateDate
	{
		[JsonProperty("$numberLong")]
		[JsonConverter(typeof(MicrosecondEpochConverter))]
		public DateTime numberLong { get; set; }
	}
}
