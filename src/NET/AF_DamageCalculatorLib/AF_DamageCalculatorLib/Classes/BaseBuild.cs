using System.Collections.Generic;

namespace AF_DamageCalculatorLib.Classes
{
	public class BaseBuild
	{
		public class UpgradeSlot
		{
			public string uniqueName;

			public int level;

			public BuildUpgradeData.ModPolarity formaPolarity;

			public bool full;

			public UpgradeSlot(string uniqueName, int level = -1, BuildUpgradeData.ModPolarity modPolarity = BuildUpgradeData.ModPolarity.Unchanged)
			{
				this.uniqueName = uniqueName;
				this.level = level;
				formaPolarity = modPolarity;
				full = true;
			}

			public UpgradeSlot()
			{
				uniqueName = "";
				level = 0;
				formaPolarity = BuildUpgradeData.ModPolarity.Unchanged;
				full = false;
			}

			public void ClearMod()
			{
				uniqueName = "";
				level = 0;
				full = false;
			}
		}

		public class BuildMetadata
		{
			public string name;

			public string author;

			public string itemUID;

			private BuildMetadata()
			{
				name = "";
				author = "";
				itemUID = "";
			}

			public BuildMetadata(string itemUID)
			{
				name = "";
				author = "";
				this.itemUID = itemUID;
			}

			public BuildMetadata(string itemUID, string author)
			{
				name = "";
				this.author = author;
				this.itemUID = itemUID;
			}

			public BuildMetadata(string itemUID, string description, string author)
			{
				name = description;
				this.author = author;
				this.itemUID = itemUID;
			}
		}

		public List<UpgradeSlot> modsSlots;

		public BuildMetadata metadata;

		public int itemLevel;

		public virtual UpgradeSlot GetSlot(string category, int index)
		{
			return modsSlots[index];
		}

		public void SetNewModInSlot(string category, int selectedModIndex, string newModUID, int level = -1)
		{
			UpgradeSlot slot = GetSlot(category, selectedModIndex);
			if (slot == null)
			{
				slot = new UpgradeSlot(newModUID, level);
				return;
			}
			slot.uniqueName = newModUID;
			slot.level = level;
			slot.full = true;
		}
	}
}
