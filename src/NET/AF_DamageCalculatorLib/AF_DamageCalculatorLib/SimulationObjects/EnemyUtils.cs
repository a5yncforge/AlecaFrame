using System;
using AF_DamageCalculatorLib.Classes;

namespace AF_DamageCalculatorLib.SimulationObjects
{
	public static class EnemyUtils
	{
		public const int ENEMY_SHIELD_GATE_MS = 100;

		public const double ENEMY_SHIELD_GATE_MULTIPLIER = 0.05000000074505806;

		private static readonly double healthF2FunctionCoefficient = 24.0 * Math.Sqrt(5.0) / 5.0;

		public static double GetEnemyHealth(double baseHealth, int level, int baseLevel, bool steelPath, bool isEximus)
		{
			if (steelPath)
			{
				baseHealth *= 2.5;
			}
			int num = level - baseLevel;
			if (num < 0)
			{
				return baseHealth;
			}
			double num2 = 1.0 + 0.014999999664723873 * Math.Pow(num, 2.0);
			if (!isEximus && num < 70)
			{
				return baseHealth * num2;
			}
			double num3 = 1.0 + healthF2FunctionCoefficient * Math.Pow(num, 0.5);
			if (!isEximus && num > 80)
			{
				return baseHealth * num3;
			}
			double smoothstepValue = GetSmoothstepValue(num);
			if (isEximus)
			{
				if (num <= 25)
				{
					return baseHealth * num2;
				}
				if (num <= 35)
				{
					return baseHealth * (1.0 + 0.05 * (double)(num - 25)) * num2;
				}
				if (num <= 50)
				{
					return baseHealth * (1.5 + 0.1 * (double)(num - 35)) * num2;
				}
				if (num <= 100)
				{
					return baseHealth * (3.0 + 0.02 * (double)(num - 50)) * (num2 * (1.0 - smoothstepValue) + num3 * smoothstepValue);
				}
				return baseHealth * 4.0 * num3;
			}
			return baseHealth * (num2 * (1.0 - smoothstepValue) + num3 * smoothstepValue);
		}

		public static double GetEnemyShield(double baseShield, int level, int baseLevel, bool steelPath, bool isEximus)
		{
			if (steelPath)
			{
				baseShield *= 6.25;
			}
			int num = level - baseLevel;
			if (num <= 0)
			{
				return baseShield;
			}
			double num2 = 1.0 + 0.019999999552965164 * Math.Pow(num, 1.75);
			if (!isEximus && num < 70)
			{
				return baseShield * num2;
			}
			double num3 = 1.0 + 1.600000023841858 * Math.Pow(num, 0.75);
			if (!isEximus && num > 80)
			{
				return baseShield * num3;
			}
			double smoothstepValue = GetSmoothstepValue(num);
			if (isEximus)
			{
				if (num <= 25)
				{
					return baseShield * num2;
				}
				if (num <= 35)
				{
					return baseShield * (1.0 + 0.05 * (double)(num - 25)) * num2;
				}
				if (num <= 50)
				{
					return baseShield * (1.5 + 0.1 * (double)(num - 35)) * num2;
				}
				if (num <= 100)
				{
					return baseShield * (3.0 + 0.02 * (double)(num - 50)) * (num2 * (1.0 - smoothstepValue) + num3 * smoothstepValue);
				}
				return baseShield * 4.0 * num3;
			}
			return baseShield * (num2 * (1.0 - smoothstepValue) + num3 * smoothstepValue);
		}

		public static double GetEnemyArmor(double baseArmor, int level, int baseLevel, bool steelPath, bool isEximus)
		{
			if (steelPath)
			{
				baseArmor *= 2.5;
			}
			int num = level - baseLevel;
			if (num <= 0)
			{
				return baseArmor;
			}
			double num2 = 1.0 + 0.004999999888241291 * Math.Pow(num, 1.75);
			if (!isEximus && num < 70)
			{
				return baseArmor * num2;
			}
			double num3 = 1.0 + 0.4000000059604645 * Math.Pow(num, 0.75);
			if (!isEximus && num > 80)
			{
				return baseArmor * num3;
			}
			double smoothstepValue = GetSmoothstepValue(num);
			if (isEximus)
			{
				if (num <= 25)
				{
					return baseArmor * num2;
				}
				if (num <= 35)
				{
					return baseArmor * (1.0 + 0.0125 * (double)(num - 25)) * num2;
				}
				if (num <= 50)
				{
					return baseArmor * (1.125 + 0.06667 * (double)(num - 35)) * num2;
				}
				if (num <= 100)
				{
					return baseArmor * (1.375 + 0.005 * (double)(num - 50)) * (num2 * (1.0 - smoothstepValue) + num3 * smoothstepValue);
				}
				return baseArmor * 1.625 * num3;
			}
			return baseArmor * (num2 * (1.0 - smoothstepValue) + num3 * smoothstepValue);
		}

		public static double GetArmorDamageMultiplier(double amountAmount, double armorMultiplier)
		{
			return 300.0 / (300.0 + amountAmount * (2.0 - armorMultiplier));
		}

		private static double GetSmoothstepValue(double levelDelta)
		{
			if (levelDelta < 70.0)
			{
				return 0.0;
			}
			if (levelDelta > 80.0)
			{
				return 1.0;
			}
			double x = (levelDelta - 70.0) / 10.0;
			return 3.0 * Math.Pow(x, 2.0) - 2.0 * Math.Pow(x, 3.0);
		}

		public static WeaponInstance.WeaponStat GetFactionModifierFromFactionName(BuildEnemyData.FilteringGroup group)
		{
			switch (group)
			{
			case BuildEnemyData.FilteringGroup.Grineer:
				return WeaponInstance.WeaponStat.DamageToGrineer;
			case BuildEnemyData.FilteringGroup.Corpus:
				return WeaponInstance.WeaponStat.DamageToCorpus;
			case BuildEnemyData.FilteringGroup.Infested:
				return WeaponInstance.WeaponStat.DamageToInfested;
			case BuildEnemyData.FilteringGroup.Murmur:
				return WeaponInstance.WeaponStat.DamageToMurmur;
			case BuildEnemyData.FilteringGroup.Corrupted:
				return WeaponInstance.WeaponStat.DamageToCorrupted;
			default:
				return WeaponInstance.WeaponStat.None;
			}
		}
	}
}
