using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types.Data
{
	public class CustomShardData
	{
		public Dictionary<string, string> shardTypesToSimpleNames = new Dictionary<string, string>();

		public Dictionary<string, string> shardUpgradeMessages = new Dictionary<string, string>();

		public Dictionary<string, string> shardUniqueIDToUpgrade = new Dictionary<string, string>();
	}
}
