using System.Collections.Generic;

namespace AlecaFramePublicLib
{
	public class CraftingRemoteDataItemComponent
	{
		public int credits;

		public int time;

		public int num;

		public string uniqueName { get; set; }

		public int neededCount { get; set; }

		public List<CraftingRemoteDataItemComponent> components { get; set; }

		public bool tradeable { get; set; }

		public ComponentType componentType { get; set; }
	}
}
