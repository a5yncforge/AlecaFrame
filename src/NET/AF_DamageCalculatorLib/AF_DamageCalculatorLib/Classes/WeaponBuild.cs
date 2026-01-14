using System;
using System.Collections.Generic;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib.Classes
{
	public class WeaponBuild : BaseBuild
	{
		public List<UpgradeSlot> arcane;

		public UpgradeSlot exilusSlot;

		public UpgradeSlot stance;

		public Dictionary<DamageType, double> innateDamages;

		public override UpgradeSlot GetSlot(string category, int index)
		{
			switch (category)
			{
			case "stance":
				return stance;
			case "exilus":
				return exilusSlot;
			case "arcane":
				return arcane[index];
			case "mod":
				return modsSlots[index];
			default:
				throw new ArgumentException("Invalid category");
			}
		}
	}
}
