using Newtonsoft.Json;

namespace AlecaFramePublicLib
{
	public class BasicRemoteDataItemData
	{
		public string name;

		public string pic;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string wiki;
	}
}
