using System.Collections.Generic;

namespace AlecaFramePublicLib
{
	public class CraftingRemoteDataItem
	{
		public int credits;

		public int time;

		public int num;

		public string uniqueName { get; set; }

		public string name { get; set; }

		public virtual List<CraftingRemoteDataItemComponent> components { get; set; }
	}
}
