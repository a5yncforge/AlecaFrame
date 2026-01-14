using System.Collections.Generic;
using AF_DamageCalculatorLib.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AlecaFrameClientLib
{
	public class BuildHandlerStatus
	{
		public class BuildHandlerStatusItem
		{
			public class ModeData
			{
				public string internalName;

				public string name;

				public int modeID;

				public bool selected;
			}

			public string name;

			public string picture;

			public string buildName;

			public bool owned;

			[JsonConverter(typeof(StringEnumConverter))]
			public BuildSource buildSource;

			public string author;

			public List<ModeData> modes;

			public List<ModeData> zoomLevels;
		}

		public class BuildHandlerStatusResults
		{
			public class ViewData
			{
				public string name;
			}

			public class BuildHandlerStatusSingleStat
			{
				public string name;

				public string value;

				public string internalName;
			}

			public class BuildHandlerStatusSingleNeededThing
			{
				public string internalName;

				public string amount;

				public string tooltip;
			}

			public BuildHandlerStatusSingleStat[] stats;

			public BuildHandlerStatusSingleNeededThing[] neededThings;

			public ViewData[] views;
		}

		public class BuildHandlerStatusMods
		{
			public class BuildHandlerStatusSingleMod
			{
				public class ModStat
				{
					public string stat;

					public string value;

					public bool conditional;
				}

				public enum PolarityColor
				{
					Red,
					Green,
					White
				}

				public bool used;

				public string name;

				public string internalName;

				public int currentLevel;

				public int maxLevel;

				public int drain;

				[JsonConverter(typeof(StringEnumConverter))]
				public BuildUpgradeData.ModPolarity slotPolarity;

				[JsonConverter(typeof(StringEnumConverter))]
				public BuildUpgradeData.ModPolarity modPolarity;

				[JsonConverter(typeof(StringEnumConverter))]
				public PolarityColor polarityColor;

				public string picture;

				[JsonConverter(typeof(StringEnumConverter))]
				public BuildUpgradeData.ModMaterial modType;

				public string longDescription;

				public ModStat[] stats;

				public bool modStatsShowOther;

				public bool modUnderstood;

				public bool owned;
			}

			public BuildHandlerStatusSingleMod[] mods;

			public BuildHandlerStatusSingleMod exilus;

			public BuildHandlerStatusSingleMod aura;

			public BuildHandlerStatusSingleMod stance;

			public BuildHandlerStatusSingleMod[] arcanes;

			public int modCapacity;

			public int totalCapacity;

			[JsonConverter(typeof(StringEnumConverter))]
			public BuildHandlerStatusSingleMod.PolarityColor capacityColor;
		}

		public class Request
		{
			public bool neededThingsShowTotal;

			public int selectedItemIndex;

			public int statViewIndexSelected;
		}

		public BuildHandlerStatusItem[] items;

		public BuildHandlerStatusResults buildResults;

		public BuildHandlerStatusMods mods;
	}
}
