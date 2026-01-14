using System;

namespace AlecaFrameClientLib.Data.Types
{
	public class Order
	{
		public int rank = -69;

		public int amberStars = -69;

		public int cyanStars = -69;

		public int platinum { get; set; }

		public int quantity { get; set; }

		public string type { get; set; }

		public User user { get; set; }

		public DateTime createdAt { get; set; }

		public DateTime updatedAt { get; set; }

		public string id { get; set; }

		public string subtype { get; set; }
	}
}
