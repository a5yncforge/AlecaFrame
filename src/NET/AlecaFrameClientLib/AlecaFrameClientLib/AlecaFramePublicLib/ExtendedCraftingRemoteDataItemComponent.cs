using System.Collections.Generic;
using AlecaFrameClientLib.Data.Types;

namespace AlecaFramePublicLib
{
	public class ExtendedCraftingRemoteDataItemComponent
	{
		public ExtendedCraftingRemoteDataItemComponent parentComponent;

		public ExtendedCraftingRemoteDataItem parentItem;

		public ItemComponent itemComponentReference;

		internal BasicRemoteDataItemData basicInfo;

		public string uniqueName { get; set; }

		public int neededCount { get; set; }

		public List<ExtendedCraftingRemoteDataItemComponent> components { get; set; }

		public bool tradeable { get; set; }

		public ComponentType componentType { get; set; }

		public int credits { get; set; }

		public int time { get; set; }

		public int num { get; set; } = 1;

		public ExtendedCraftingRemoteDataItem GetItem()
		{
			if (parentItem != null)
			{
				return parentItem;
			}
			if (parentComponent != null)
			{
				return parentComponent.GetItem();
			}
			return null;
		}

		public ItemComponent GetItemComponentComponentProblemAware()
		{
			if (itemComponentReference != null)
			{
				return itemComponentReference;
			}
			if (componentType == ComponentType.SubBlueprint)
			{
				return parentComponent?.itemComponentReference;
			}
			return null;
		}
	}
}
