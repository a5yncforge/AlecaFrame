using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using SevenZip;

namespace AlecaFrameClientLib.Utils
{
	public static class UpdateHandler
	{
		public static string CreateFileMD5(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return "|==FILE-NOT-FOUND==|";
			}
			using MD5 mD = MD5.Create();
			using FileStream inputStream = File.OpenRead(filePath);
			return BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", "").ToLowerInvariant();
		}

		public static void DownloadGlobalSettingsNonBlocking()
		{
		}

		public static void CheckLocalDataAndUpdateIfNeccessary(bool reAttempt = false)
		{
			StaticData.Log(OverwolfWrapper.LogType.INFO, "Checking local data MD5 hashes...");
			bool flag = true;
			string text = CreateFileMD5(StaticData.saveFolder + "/cachedData/json.7z");
			if (text.Contains("|==FILE-NOT-FOUND==|"))
			{
				flag = false;
				text = CreateFileMD5(StaticData.saveFolder + "/cachedData/json.zip");
				if (text.Contains("|==FILE-NOT-FOUND==|"))
				{
					StaticData.Log(OverwolfWrapper.LogType.INFO, "No local MD5 found, defaulting to V2 approach");
					flag = true;
				}
			}
			StaticData.Log(OverwolfWrapper.LogType.INFO, "Got local data MD5: " + text);
			string text2 = ((!flag) ? HTTPHandler.MakeGETRequest("https://" + StaticData.CDNdomain + "/warframeData/json.md5") : HTTPHandler.MakeGETRequest("https://" + StaticData.CDNdomain + "/warframeData/json7z.md5"));
			string text3 = text2.Split(' ')[0].Trim();
			StaticData.Log(OverwolfWrapper.LogType.INFO, "Got remote data MD5: " + text3 + " refersToV2: " + flag);
			File.WriteAllText(StaticData.saveFolder + "/cachedData/receivedMD5.txt", text2);
			if (text != text3)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Local and remote data MD5 are different, trying to download data again...");
				if (reAttempt)
				{
					StaticData.Log(OverwolfWrapper.LogType.INFO, "Attempting to download and cache data with V1...");
					DownloadAndCacheData();
					try
					{
						if (File.Exists(StaticData.saveFolder + "/cachedData/json.7z"))
						{
							File.Delete(StaticData.saveFolder + "/cachedData/json.7z");
						}
						return;
					}
					catch (Exception ex)
					{
						StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to delete old data file! " + ex);
						return;
					}
				}
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Attempting to download and cache data with V2...");
				DownloadAndCacheDataV2(text3);
				try
				{
					if (File.Exists(StaticData.saveFolder + "/cachedData/json.zip"))
					{
						File.Delete(StaticData.saveFolder + "/cachedData/json.zip");
					}
					return;
				}
				catch (Exception ex2)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to delete old data file! " + ex2);
					return;
				}
			}
			StaticData.Log(OverwolfWrapper.LogType.INFO, "Local and remote data MD5 hashes are the same, no data download is needed!");
		}

		public static void DownloadAndCacheData()
		{
			try
			{
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Downloading new data from CDN... (json.zip)");
				Stopwatch stopwatch = Stopwatch.StartNew();
				HTTPHandler.MakeFileRequest("https://" + StaticData.CDNdomain + "/warframeData/json.zip", StaticData.saveFolder + "/cachedData/json.zip", 60000);
				stopwatch.Stop();
				StaticData.Log(OverwolfWrapper.LogType.INFO, $"Data download finished in {stopwatch.Elapsed}");
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Removing old data...");
				if (Directory.Exists(StaticData.saveFolder + "/cachedData/json"))
				{
					Directory.Delete(StaticData.saveFolder + "/cachedData/json", recursive: true);
				}
				if (Directory.Exists(StaticData.saveFolder + "/cachedData/custom"))
				{
					Directory.Delete(StaticData.saveFolder + "/cachedData/custom", recursive: true);
				}
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Extracting new data...");
				ZipFile.ExtractToDirectory(StaticData.saveFolder + "/cachedData/json.zip", StaticData.saveFolder + "/cachedData/");
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to download and cache data: " + ex.Message);
			}
		}

		public static void DownloadAndCacheDataV2(string fileMD5)
		{
			try
			{
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Downloading new data from CDN... (json.7z)");
				Stopwatch stopwatch = Stopwatch.StartNew();
				try
				{
					StaticData.Log(OverwolfWrapper.LogType.INFO, "Trying Github raw download link...");
					HTTPHandler.MakeFileRequest("https://raw.githubusercontent.com/alecamaracm/AlecaFrame-WFDataHistory/main/archives/" + fileMD5 + ".7z", StaticData.saveFolder + "/cachedData/json.7z", 15000, 0);
				}
				catch (Exception arg)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, $"Github download link failed ({arg}), trying the normal one!");
					HTTPHandler.MakeFileRequest("https://" + StaticData.CDNdomain + "/warframeData/json.7z", StaticData.saveFolder + "/cachedData/json.7z", 60000);
				}
				stopwatch.Stop();
				StaticData.Log(OverwolfWrapper.LogType.INFO, $"Data download finished in {stopwatch.Elapsed}");
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Removing old data...");
				if (Directory.Exists(StaticData.saveFolder + "/cachedData/json"))
				{
					Directory.Delete(StaticData.saveFolder + "/cachedData/json", recursive: true);
				}
				if (Directory.Exists(StaticData.saveFolder + "/cachedData/custom"))
				{
					Directory.Delete(StaticData.saveFolder + "/cachedData/custom", recursive: true);
				}
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Setting up 7z paths...");
				SevenZipBase.SetLibraryPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\7z.dll");
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Extracting new data...");
				new SevenZipExtractor(StaticData.saveFolder + "/cachedData/json.7z").ExtractArchive(StaticData.saveFolder.Replace("/", "\\") + "\\cachedData\\");
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to download and cache data: " + ex.Message);
			}
		}
	}
}
