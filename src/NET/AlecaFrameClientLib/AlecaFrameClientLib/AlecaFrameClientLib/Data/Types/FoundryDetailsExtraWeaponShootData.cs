using System;
using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class FoundryDetailsExtraWeaponShootData
	{
		public class FoundryDetailsExtraWeaponShootDataMiniShoot
		{
			public class FoundryDetailsExtraWeaponDataDamage
			{
				public string damageType;

				public float damage;

				[NonSerialized]
				public float value;
			}

			public string name;

			public string fireRate;

			public float critChance;

			public float critDamage;

			public float statusChance;

			public List<FoundryDetailsExtraWeaponDataDamage> damages = new List<FoundryDetailsExtraWeaponDataDamage>();

			public string shotType;
		}

		public string ammo;

		public float accuracy;

		public string[] polarities;

		public int magazineSize;

		public string triggerType;

		public string weaponType;

		public double rivenDisposition;

		public List<FoundryDetailsExtraWeaponShootDataMiniShoot> attacks = new List<FoundryDetailsExtraWeaponShootDataMiniShoot>();

		public string reloadTime;

		public string noise;
	}
}
