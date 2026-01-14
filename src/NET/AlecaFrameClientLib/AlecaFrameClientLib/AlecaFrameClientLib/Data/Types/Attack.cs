using System.Collections.Generic;

namespace AlecaFrameClientLib.Data.Types
{
	public class Attack
	{
		public Dictionary<string, float> damage = new Dictionary<string, float>();

		public string name { get; set; }

		public float speed { get; set; }

		public float crit_chance { get; set; }

		public float crit_mult { get; set; }

		public float status_chance { get; set; }

		public string slide { get; set; }

		public Slam slam { get; set; }

		public float charge_time { get; set; }

		public string shot_type { get; set; }

		public int? shot_speed { get; set; }

		public object flight { get; set; }

		public Falloff falloff { get; set; }

		public string jump { get; set; }
	}
}
