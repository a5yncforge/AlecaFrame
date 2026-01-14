using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AlecaFrameClientLib.Data
{
	public static class FavouriteHelper
	{
		private static HashSet<string> FavouriteItems = new HashSet<string>();

		public static bool IsFavourite(string item)
		{
			if (string.IsNullOrEmpty(item))
			{
				return false;
			}
			lock (FavouriteItems)
			{
				return FavouriteItems.Contains(item);
			}
		}

		public static void AddFavourite(string item)
		{
			if (!string.IsNullOrEmpty(item))
			{
				lock (FavouriteItems)
				{
					FavouriteItems.Add(item);
				}
				Save();
				StaticData.overwolfWrappwer?.OnFavouritesUpdateCaller();
			}
		}

		public static void RemoveFavourite(string item)
		{
			if (!string.IsNullOrEmpty(item))
			{
				lock (FavouriteItems)
				{
					FavouriteItems.Remove(item);
				}
				Save();
				StaticData.overwolfWrappwer?.OnFavouritesUpdateCaller();
			}
		}

		public static void Initialize()
		{
			Load();
		}

		public static void Load()
		{
			lock (FavouriteItems)
			{
				try
				{
					string path = StaticData.saveFolder + "favourites.txt";
					if (File.Exists(path))
					{
						FavouriteItems = File.ReadAllLines(path).ToHashSet();
					}
					else
					{
						FavouriteItems = new HashSet<string>();
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to save favourites: " + ex);
					FavouriteItems = new HashSet<string>();
				}
			}
		}

		public static void Save()
		{
			lock (FavouriteItems)
			{
				try
				{
					File.WriteAllLines(StaticData.saveFolder + "favourites.txt", FavouriteItems);
				}
				catch (Exception ex)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to save favourites: " + ex);
				}
			}
		}
	}
}
