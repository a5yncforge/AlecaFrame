using System.Collections.Generic;

namespace AlecaFramePublicLib
{
	public class BasicRemoteData
	{
		public Dictionary<string, BasicRemoteDataItemData> items = new Dictionary<string, BasicRemoteDataItemData>();

		public Dictionary<string, int> nodeXP = new Dictionary<string, int>();

		public Dictionary<string, int> maxLevelOverrides;
	}
}
