using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class FoundryDetailsExtraWeaponMeleeData
	{
		public class FoundryDetailsExtraWeaponMeleeDataMiniShoot
		{
			public string name;

			public float fireRate;

			public float critChance;

			public float critDamage;

			public float statusChance;

			public List<FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage> damages = new List<FoundryDetailsExtraWeaponShootData.FoundryDetailsExtraWeaponShootDataMiniShoot.FoundryDetailsExtraWeaponDataDamage>();

			public string shotType;
		}

		public float blockingAngle;

		public int comboDuration;

		public string heavyAttackTime;

		public string range;

		public string[] polarities;

		public int rivenDisposition;

		public List<FoundryDetailsExtraWeaponMeleeDataMiniShoot> attacks = new List<FoundryDetailsExtraWeaponMeleeDataMiniShoot>();

		public string auraPolarity;
	}
}
