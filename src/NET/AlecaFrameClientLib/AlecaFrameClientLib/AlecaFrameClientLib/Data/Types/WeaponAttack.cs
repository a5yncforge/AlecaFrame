using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class WeaponAttack
	{
		public string name;

		public float speed = -1f;

		public float crit_chance = -1f;

		public float crit_mult = -1f;

		public float status_chance = -1f;

		public string shot_type;

		public Dictionary<string, float> damage = new Dictionary<string, float>();
	}
}
