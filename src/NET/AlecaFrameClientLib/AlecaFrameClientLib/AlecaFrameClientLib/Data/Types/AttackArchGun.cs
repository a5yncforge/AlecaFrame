using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class AttackArchGun
	{
		public Dictionary<string, float> damage = new Dictionary<string, float>();

		public string name { get; set; }

		public float speed { get; set; }

		public int crit_chance { get; set; }

		public float crit_mult { get; set; }

		public float status_chance { get; set; }

		public string shot_type { get; set; }

		public Falloff falloff { get; set; }

		public object shot_speed { get; set; }

		public string flight { get; set; }

		public float charge_time { get; set; }
	}
}
