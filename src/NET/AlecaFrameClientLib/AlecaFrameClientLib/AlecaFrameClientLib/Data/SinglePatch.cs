using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace AlecaFrameClientLib.Data
{
	public class SinglePatch
	{
		public enum PathType
		{
			Add,
			Remove,
			Replace,
			Patch
		}

		public string uniqueName;

		[JsonConverter(typeof(StringEnumConverter))]
		public PathType type;

		public JObject data;
	}
}
