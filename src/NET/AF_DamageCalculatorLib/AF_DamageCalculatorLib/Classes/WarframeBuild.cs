using System;
using System.Collections.Generic;

namespace AF_DamageCalculatorLib.Classes
{
	public class WarframeBuild : BaseBuild
	{
		public UpgradeSlot auraSlot;

		public List<UpgradeSlot> arcaneSlots;

		public UpgradeSlot exilusSlot;

		public override UpgradeSlot GetSlot(string category, int index)
		{
			switch (category)
			{
			case "aura":
				return auraSlot;
			case "exilus":
				return exilusSlot;
			case "arcane":
				return arcaneSlots[index];
			case "mod":
				return modsSlots[index];
			default:
				throw new ArgumentException("Invalid category");
			}
		}
	}
}
