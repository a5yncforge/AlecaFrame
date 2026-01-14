namespace AlecaFrameClientLib.Data.Types
{
	public class AttackSentinelWeapon
	{
		public string name { get; set; }

		public float speed { get; set; }

		public float crit_chance { get; set; }

		public float crit_mult { get; set; }

		public float status_chance { get; set; }

		public Damagetypes damage { get; set; }

		public string shot_type { get; set; }

		public object shot_speed { get; set; }

		public string flight { get; set; }

		public int charge_time { get; set; }
	}
}
