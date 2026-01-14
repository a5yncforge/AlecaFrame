using System.Collections.Generic;

namespace AlecaFrameClientLib.Data
{
	public class RecourcesTabOutputData
	{
		public class UsedInData
		{
			public string uniqueName;

			public string name;

			public string picture;
		}

		public class ResourcesTabOutputDataItem
		{
			public string uniqueName;

			public string name;

			public string picture;

			public int totalNeeded;

			public int owned;

			public float percentOwned;

			public bool hasEnough;

			public List<UsedInData> usedInList = new List<UsedInData>();
		}

		public class ResourceTabOutputArchonShard
		{
			public string uniqueName;

			public string uniqueNameMythic;

			public string name;

			public string picture;

			public int inventoryNormal;

			public int inventoryMythic;

			public int equippedNormal;

			public int equippedMythic;

			public List<ShardUsedInData> equippedUsedInList = new List<ShardUsedInData>();
		}

		public class ShardUsedInData
		{
			public string uniqueName;

			public string name;

			public string picture;

			public int numNormal;

			public int numMythic;
		}

		public List<ResourcesTabOutputDataItem> resources = new List<ResourcesTabOutputDataItem>();

		public List<ResourceTabOutputArchonShard> shards = new List<ResourceTabOutputArchonShard>();
	}
}
