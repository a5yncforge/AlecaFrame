using System.Collections.Generic;

namespace AlecaFrameClientLib.Data
{
	public class WorldStateCircuitData
	{
		public class WorldStateCircuitDataItem
		{
			public string uniqueID;

			public string name;

			public string picture;

			public bool owned;
		}

		public List<WorldStateCircuitDataItem> normal = new List<WorldStateCircuitDataItem>();

		public List<WorldStateCircuitDataItem> steel = new List<WorldStateCircuitDataItem>();
	}
}
