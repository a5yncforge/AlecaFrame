using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data.Types
{
	public class RelicPlannerCustomDetails
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string expectedPlat;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string expectedDucats;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<GroupedDetails> groupedDetails;

		[NonSerialized]
		public float plat;

		[NonSerialized]
		public float ducats;

		public List<FoundryItemComponent> rewards;

		[NonSerialized]
		public string tier;

		public float intact2radPlatDiff;

		public float intact2radDucatDiff;

		public string bestForPlat;

		public string bestForDucats;

		public bool isFav;
	}
}
