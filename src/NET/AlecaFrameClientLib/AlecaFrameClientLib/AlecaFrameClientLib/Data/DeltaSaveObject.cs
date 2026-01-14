using System;
using System.Collections.Generic;
using System.IO;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using Newtonsoft.Json;

namespace AlecaFrameClientLib.Data
{
	public class DeltaSaveObject
	{
		public bool savedCleanly;

		public List<Miscitem> previousMiscState = new List<Miscitem>();

		public Dictionary<string, int> currentDeltas = new Dictionary<string, int>();

		public static void Save(DeltaSaveObject deltaSaveObject)
		{
			try
			{
				Misc.WriteAllTextEncrypted(StaticData.saveFolder + "/deltas.dat", JsonConvert.SerializeObject(deltaSaveObject));
			}
			catch (Exception arg)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, $"Failed to save deltas! {arg}");
			}
		}

		public static DeltaSaveObject Load()
		{
			if (File.Exists(StaticData.saveFolder + "/deltas.dat"))
			{
				try
				{
					return JsonConvert.DeserializeObject<DeltaSaveObject>(Misc.ReadAllTextEncrypted(StaticData.saveFolder + "/deltas.dat"));
				}
				catch (Exception arg)
				{
					StaticData.Log(OverwolfWrapper.LogType.ERROR, $"Failed to load deltas file {arg}. Reverting to an empty one...");
					return new DeltaSaveObject();
				}
			}
			return new DeltaSaveObject();
		}
	}
}
