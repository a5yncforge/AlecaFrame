namespace AlecaFramePublicLib.DataTypes
{
	public class WFMItemListItem
	{
		public class i18nCollection
		{
			public class i18nEntry
			{
				public string name { get; set; }

				public string icon { get; set; }

				public string thumb { get; set; }
			}

			public i18nEntry en { get; set; }
		}

		public string item_name => i18n?.en?.name;

		public string id { get; set; }

		public string gameRef { get; set; }

		public string slug { get; set; }

		public string[] tags { get; set; }

		public string[] subtypes { get; set; }

		public int ducats { get; set; }

		public bool vaulted { get; set; }

		public string maxRank { get; set; }

		public i18nCollection i18n { get; set; }

		public bool bulkTradable { get; set; }
	}
}
