using System.Collections.Generic;
using AlecaFrameClientLib.Data.Types;

namespace AlecaFramePublicLib
{
	public class ExtendedCraftingRemoteDataItem
	{
		public BigItem bigItem;

		public string uniqueName { get; set; }

		public string name { get; set; }

		public virtual List<ExtendedCraftingRemoteDataItemComponent> components { get; set; }

		public int credits { get; set; }

		public int time { get; set; }

		public int num { get; set; } = 1;

		public ExtendedCraftingRemoteDataItemComponent ToComponentData()
		{
			return new ExtendedCraftingRemoteDataItemComponent
			{
				uniqueName = uniqueName,
				components = components,
				itemComponentReference = new ItemComponent
				{
					name = name,
					uniqueName = uniqueName,
					imageName = bigItem.imageName
				},
				neededCount = 1,
				parentItem = this,
				credits = credits,
				time = time,
				num = num
			};
		}
	}
}
