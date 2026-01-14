using System.Collections.Generic;
using AlecaFramePublicLib;

namespace AF_DamageCalculatorLib.Classes
{
	public class BuildSourceDataFile
	{
		public Dictionary<string, BuildUpgradeData> mods = new Dictionary<string, BuildUpgradeData>();

		public Dictionary<string, BuildUpgradeData> arcanes = new Dictionary<string, BuildUpgradeData>();

		public Dictionary<string, BuildUpgradeData> modSets = new Dictionary<string, BuildUpgradeData>();

		public Dictionary<string, BuildUpgradeData> shards = new Dictionary<string, BuildUpgradeData>();

		public Dictionary<string, BuildEnemyData> enemies = new Dictionary<string, BuildEnemyData>();

		public Dictionary<string, BuildWeaponData> weapons = new Dictionary<string, BuildWeaponData>();

		public string[] parentList;

		public Dictionary<string, BuildWarframeData> warframes = new Dictionary<string, BuildWarframeData>();

		public Dictionary<BuildEnemyData.ProtectionType, Dictionary<DamageType, double>> damageTable;

		public DamageType shieldBypassDamageTypes;
	}
}
