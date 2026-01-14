using System.Collections.Generic;

namespace AF_DamageCalculatorLib.Classes
{
	public class BuildWarframeData
	{
		public string name;

		public string uniqueName;

		public double health;

		public double shield;

		public double armor;

		public double energy;

		public double sprintSpeed;

		public List<BuildUpgradeData.BuildModBuff> levelUpBuffs;

		public BuildUpgradeData.ModPolarity[] polarities;

		public BuildUpgradeData.ModPolarity exilusPolarity;

		public BuildUpgradeData.ModPolarity auraPolarity;

		public int[] parents;
	}
}
