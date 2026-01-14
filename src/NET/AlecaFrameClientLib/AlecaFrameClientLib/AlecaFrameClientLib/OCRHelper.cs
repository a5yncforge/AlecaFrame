using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlecaFrameClientLib.Data;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;

namespace AlecaFrameClientLib
{
	public static class OCRHelper
	{
		public enum DeviceCap
		{
			LOGPIXELSX = 88,
			LOGPIXELSY = 90
		}

		public enum PROCESS_DPI_AWARENESS
		{
			PROCESS_DPI_UNAWARE,
			PROCESS_SYSTEM_DPI_AWARE,
			PROCESS_PER_MONITOR_DPI_AWARE
		}

		public class UINotDetectedException : Exception
		{
			public UINotDetectedException(string message)
				: base(message)
			{
			}
		}

		public static IntPtr lastWarframeWindowHandle = IntPtr.Zero;

		private const int S_OK = 0;

		public static OCRLocationSettings normalUISettings = new OCRLocationSettings
		{
			rectTop = 0.38f,
			rectBottom = 0.427f,
			rectWdith = 0.121f,
			rectSeparation = 0.0053f,
			overallTopScreenPerOne = 0.2f,
			overallBottomScreenPerOne = 0.4625f,
			relicCounterTop = 0.431f,
			relicCounterBottom = 0.458f
		};

		public static OCRLocationSettings LegacyUISettings = new OCRLocationSettings
		{
			rectTop = 0.4027f,
			rectBottom = 0.437f,
			rectWdith = 0.1005f,
			rectSeparation = 0.0052f,
			overallTopScreenPerOne = 0.25f,
			overallBottomScreenPerOne = 0.4685f,
			relicCounterTop = 0.441f,
			relicCounterBottom = 0.464f
		};

		private const float percentEdgesToBeConsideredReward = 1.75f;

		private const float edgeDetectorThreshold = 70f;

		private const float percentEdgesToBeConsideredWithText = 5f;

		private static int lastWFProcessID = -1;

		private static Bitmap bitmap1x1 = new Bitmap(1, 1);

		private static byte[] bitmap1x1BitArray = ImageToByte2(bitmap1x1);

		public static readonly List<char> notAllowedCharsInRelicRewardNames = new List<char>
		{
			'|', '~', '"', '=', '?', '!', '\\', '/', '1', '2',
			'3', '4', '5', '6', '7', '8', '9', '.'
		};

		[DllImport("user32.dll")]
		private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

		[DllImport("user32.dll")]
		internal static extern int GetDpiForWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		[DllImport("shcore.dll")]
		private static extern uint GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
		public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

		[DllImport("Shcore.dll")]
		public static extern int GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS value);

		public static OCRLocationSettings getOCRsettings(bool isLegacy, int screenshotHeight, float scale)
		{
			if (isLegacy && screenshotHeight > 1080)
			{
				scale = 0.74f;
			}
			OCRLocationSettings oCRLocationSettings = (isLegacy ? LegacyUISettings : normalUISettings).Clone();
			oCRLocationSettings.rectTop = 0.5f + scale * (oCRLocationSettings.rectTop - 0.5f);
			oCRLocationSettings.rectBottom = 0.5f + scale * (oCRLocationSettings.rectBottom - 0.5f);
			oCRLocationSettings.rectWdith *= scale;
			oCRLocationSettings.rectSeparation *= scale;
			oCRLocationSettings.overallTopScreenPerOne = 0.5f + scale * (oCRLocationSettings.overallTopScreenPerOne - 0.5f);
			oCRLocationSettings.overallBottomScreenPerOne = 0.5f + scale * (oCRLocationSettings.overallBottomScreenPerOne - 0.5f);
			oCRLocationSettings.relicCounterTop = 0.5f + scale * (oCRLocationSettings.relicCounterTop - 0.5f);
			oCRLocationSettings.relicCounterBottom = 0.5f + scale * (oCRLocationSettings.relicCounterBottom - 0.5f);
			return oCRLocationSettings;
		}

		public static void Initialize()
		{
			DoLogTrackingWork();
		}

		private static void DoLogTrackingWork()
		{
			if (Debugger.IsAttached)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Debugger attached. Not checking log files to avoid crashes");
				return;
			}
			Thread thread = new Thread((ThreadStart)delegate
			{
				while (true)
				{
					try
					{
						using MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("DBWIN_BUFFER", 4096L);
						bool createdNew;
						using EventWaitHandle eventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "DBWIN_BUFFER_READY", out createdNew);
						try
						{
							if (lastWFProcessID != -1)
							{
								bool createdNew2;
								using EventWaitHandle eventWaitHandle2 = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "DBWIN_DATA_READY", out createdNew2);
								StaticData.Log(OverwolfWrapper.LogType.WARN, $"Buffer/data ready events are already being used somewhere else! buffer={!createdNew}, data={!createdNew2}");
								char[] array = new char[5000];
								while (lastWFProcessID != -1)
								{
									eventWaitHandle.Set();
									if (eventWaitHandle2.WaitOne(TimeSpan.FromSeconds(3.0)))
									{
										using MemoryMappedViewStream input = memoryMappedFile.CreateViewStream();
										using BinaryReader binaryReader = new BinaryReader(input, Encoding.UTF8);
										if (binaryReader.ReadUInt32() == lastWFProcessID)
										{
											array = binaryReader.ReadChars(4092);
											EELogProcessor.ProcessLine(new string(array, 0, Array.IndexOf(array, '\0')));
										}
									}
								}
							}
						}
						catch (Exception ex)
						{
							StaticData.Log(OverwolfWrapper.LogType.ERROR, "An error has ocurred in the LogTracking worker inside the log loop: " + ex);
							Thread.Sleep(10000);
						}
						finally
						{
							eventWaitHandle.Set();
						}
					}
					catch (Exception ex2)
					{
						StaticData.Log(OverwolfWrapper.LogType.ERROR, "An error has ocurred in the LogTracking worker before entering the loop: " + ex2);
					}
					finally
					{
						Thread.Sleep(10000);
					}
				}
			});
			thread.IsBackground = true;
			thread.Start();
			Thread thread2 = new Thread((ThreadStart)delegate
			{
				while (true)
				{
					try
					{
						while (!CheckIsWarframeIsOpen())
						{
							Thread.Sleep(5000);
						}
						StaticData.Log(OverwolfWrapper.LogType.INFO, "Warframe is now detected internally as opened.");
						Thread.Sleep(1500);
						while (CheckIsWarframeIsOpen())
						{
							Thread.Sleep(5000);
						}
						StaticData.Log(OverwolfWrapper.LogType.INFO, "Warframe is now detected internally as closed.");
					}
					catch (Exception ex)
					{
						StaticData.Log(OverwolfWrapper.LogType.ERROR, "An error has ocurred in the ProcessTracking worker: " + ex);
						Thread.Sleep(15000);
					}
				}
			});
			thread2.IsBackground = true;
			thread2.Start();
		}

		public static void DoScreenshotRequestWork(bool isUILegacyScale, bool debugMode = false, Bitmap debugBitmap = null, Action<AlecaLogDataLogger.relicLogImageType, Bitmap> otherLoggerMethod = null, bool doNotLog = false, bool doFinalRecognitionStep = true)
		{
			StaticData.overwolfWrappwer.lastRelicStartTime = DateTime.UtcNow;
			using (AlecaLogDataLogger alecaLogDataLogger = new AlecaLogDataLogger(AlecaLogDataLogger.AlecaLogType.relic, StaticData.relicLogFolder, otherLoggerMethod, doNotLog))
			{
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Relic choosing start detected!");
				alecaLogDataLogger.AddString("Starting work");
				if (debugBitmap != null && !debugMode)
				{
					throw new ArgumentException("mainBitmap can only be used for debugging purposes!!!!");
				}
				if (!Misc.IsWarframeInTheForeground())
				{
					alecaLogDataLogger.AddString("Warframe has been detected to be in the background. Starting alternative delay mode");
					while (!Misc.IsWarframeInTheForeground())
					{
						if ((DateTime.UtcNow - StaticData.overwolfWrappwer.lastRelicStartTime).TotalSeconds > 10.0)
						{
							alecaLogDataLogger.AddString("Warframe is still in the background. Exiting early...");
							TryDisposeCurrentRelicScreenshot();
							return;
						}
						Thread.Sleep(100);
					}
					alecaLogDataLogger.AddString("Warframe not longer in the background. Resuming normal mode");
					Thread.Sleep(400);
				}
				if (debugMode)
				{
					alecaLogDataLogger.AddString("In debug mode");
					StaticData.overwolfWrappwer.lastWarframeScreenshot = debugBitmap;
				}
				else
				{
					alecaLogDataLogger.AddString("Normal mode (Non debug)");
					TakeScreenshotOfWarframeInCSharp(alecaLogDataLogger);
				}
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Got screenshot properly, starting to do work now.");
				alecaLogDataLogger.AddString("Screenshot ready");
				StaticData.overwolfWrappwer.isNewRelicDataReady = false;
				StaticData.overwolfWrappwer.newRelicDataOrErrorReady.Reset();
				StaticData.overwolfWrappwer.onRelicOpenRequestCaller();
				if (IsScreenshotAllDark(alecaLogDataLogger))
				{
					alecaLogDataLogger.AddString("Got a black screen, capturing again in a few seconds...");
					if (!debugMode)
					{
						TryDisposeCurrentRelicScreenshot();
					}
					Thread.Sleep(2000);
					if (debugMode)
					{
						StaticData.overwolfWrappwer.lastWarframeScreenshot = debugBitmap;
					}
					else
					{
						TakeScreenshotOfWarframeInCSharp(alecaLogDataLogger);
					}
				}
				alecaLogDataLogger.AddString("Scale mode detected: " + (isUILegacyScale ? "LEGACY" : "NORMAL"));
				alecaLogDataLogger.AddString("Read Scale mode: " + StaticData.WarframeScalingMode);
				alecaLogDataLogger.AddString("UI Scale being used: " + GetScale());
				bool flag = false;
				while (true)
				{
					try
					{
						DoRelicRewardRecognitionWork(isUILegacyScale, debugMode ? debugBitmap : StaticData.overwolfWrappwer.lastWarframeScreenshot, alecaLogDataLogger, doFinalRecognitionStep);
					}
					catch (UINotDetectedException ex)
					{
						if (flag)
						{
							alecaLogDataLogger.AddString("Failed to detect UI twice, giving up :(");
							alecaLogDataLogger.FlagError();
							StaticData.overwolfWrappwer.newRelicDataErrorOccurred = true;
							StaticData.overwolfWrappwer.newRelicDataError = ex.Message;
							StaticData.Log(OverwolfWrapper.LogType.ERROR, "A general error has ocurred when doing relic work: " + ex);
							alecaLogDataLogger.AddString("Signaling the app that the data is ready...");
							StaticData.overwolfWrappwer.isNewRelicDataReady = true;
							StaticData.overwolfWrappwer.newRelicDataOrErrorReady.Set();
							break;
						}
						alecaLogDataLogger.AddString("Failed to detect UI. Waiting and retrying again...");
						flag = true;
						Thread.Sleep(1500);
						if (!debugMode)
						{
							TryDisposeCurrentRelicScreenshot();
							TakeScreenshotOfWarframeInCSharp(alecaLogDataLogger);
						}
						else
						{
							alecaLogDataLogger.AddString("Not taking screenshot again because we are in debug mode!");
						}
						continue;
					}
					break;
				}
				if (!debugMode)
				{
					TryDisposeCurrentRelicScreenshot();
				}
				if (!doNotLog)
				{
					Thread.Sleep(1500);
					if (TakeScreenshotOfWarframeInCSharp(alecaLogDataLogger))
					{
						alecaLogDataLogger.AddBitmap(AlecaLogDataLogger.relicLogImageType.final, StaticData.overwolfWrappwer.lastWarframeScreenshot);
						TryDisposeCurrentRelicScreenshot();
					}
				}
			}
			Task.Run(delegate
			{
				try
				{
					Directory.CreateDirectory(StaticData.relicLogFolder);
					string[] files = Directory.GetFiles(StaticData.relicLogFolder);
					foreach (string text in files)
					{
						FileInfo fileInfo = new FileInfo(text);
						if ((DateTime.UtcNow - fileInfo.CreationTimeUtc).TotalDays > 1.0)
						{
							try
							{
								File.Delete(text);
							}
							catch
							{
							}
						}
					}
				}
				catch (Exception ex2)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to do relic log cleanup! " + ex2);
				}
			});
		}

		private static void TryDisposeCurrentRelicScreenshot()
		{
			if (StaticData.overwolfWrappwer.lastWarframeScreenshot != null)
			{
				try
				{
					StaticData.overwolfWrappwer.lastWarframeScreenshot.Dispose();
					StaticData.overwolfWrappwer.lastWarframeScreenshot = null;
				}
				catch
				{
				}
			}
		}

		private static bool IsScreenshotAllDark(AlecaLogDataLogger logger)
		{
			try
			{
				float num = 0f;
				float num2 = 0f;
				using (Bitmap bitmap = new Bitmap(100, 50))
				{
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						graphics.DrawImage(StaticData.overwolfWrappwer.lastWarframeScreenshot, new Rectangle(0, 0, 100, 50), new Rectangle(150, 250, 100, 50), GraphicsUnit.Pixel);
					}
					num = CalculateTotalLiminosityPercent(bitmap, 10f);
				}
				using (Bitmap bitmap2 = new Bitmap(100, 50))
				{
					using (Graphics graphics2 = Graphics.FromImage(bitmap2))
					{
						graphics2.DrawImage(StaticData.overwolfWrappwer.lastWarframeScreenshot, new Rectangle(0, 0, 100, 50), new Rectangle(StaticData.overwolfWrappwer.lastWarframeScreenshot.Width - 300, 400, 100, 50), GraphicsUnit.Pixel);
					}
					num2 = CalculateTotalLiminosityPercent(bitmap2, 10f);
				}
				logger.AddString("[BLAK_SCREEN] Total lumi 1: " + num);
				logger.AddString("[BLAK_SCREEN] Total lumi 2: " + num2);
				return num == 0f || num2 == 0f;
			}
			catch
			{
				return false;
			}
		}

		public static bool TakeScreenshotOfWarframeInCSharp(AlecaLogDataLogger logger)
		{
			try
			{
				Process[] processesByName = Process.GetProcessesByName("Warframe.x64");
				if (!processesByName.Any())
				{
					if (logger != null)
					{
						logger.AddString("Can not find any warframe processes running to take a screenshot!");
						logger.FlagError();
					}
					return false;
				}
				IntPtr hWnd = (lastWarframeWindowHandle = processesByName.First().MainWindowHandle);
				GetClientRect(hWnd, out var lpRect);
				Point lpPoint = new Point(lpRect.X, lpRect.Y);
				ClientToScreen(hWnd, ref lpPoint);
				try
				{
					GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out var value);
					if (value == PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE)
					{
						logger?.AddString("Process is not DPI aware. Applying DPI fix");
						logger?.AddString("This is not an entirely supported environment and might fail in scenarios where multiple monitors hace different DPIs!");
						float num = (float)GetDpiForWindow(hWnd) / 96f;
						lpPoint.X = (int)((float)lpPoint.X * num);
						lpPoint.Y = (int)((float)lpPoint.Y * num);
						lpRect.Width = (int)((float)lpRect.Width * num);
						lpRect.Height = (int)((float)lpRect.Height * num);
					}
					else
					{
						logger?.AddString("Process is DPI aware. NOT doing the DPI fix");
					}
				}
				catch (DllNotFoundException)
				{
					logger?.AddString("W7 detected. Using alternate fix");
					StaticData.Log(OverwolfWrapper.LogType.WARN, "W7 detected. Using alternate fix");
					Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
					IntPtr hdc = graphics.GetHdc();
					float num2 = (float)GetDeviceCaps(hdc, 88) / 96f;
					float num3 = (float)GetDeviceCaps(hdc, 90) / 96f;
					graphics.ReleaseHdc();
					logger?.AddString("Using DPI multiplier of:" + num2 + "x" + num3);
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Using DPI multiplier of:" + num2 + "x" + num3);
					lpPoint.X = (int)((float)lpPoint.X * num2);
					lpPoint.Y = (int)((float)lpPoint.Y * num3);
					lpRect.Width = (int)((float)lpRect.Width * num2);
					lpRect.Height = (int)((float)lpRect.Height * num3);
				}
				if (lpRect.Width == 0 || lpRect.Height == 0)
				{
					logger?.AddString("Can not get bonds for current window screenshot!");
					return false;
				}
				StaticData.overwolfWrappwer.lastWarframeScreenshot = new Bitmap(lpRect.Width, lpRect.Height);
				using (Graphics graphics2 = Graphics.FromImage(StaticData.overwolfWrappwer.lastWarframeScreenshot))
				{
					graphics2.CopyFromScreen(lpPoint, Point.Empty, lpRect.Size);
				}
				return true;
			}
			catch (Exception ex2)
			{
				if (logger != null)
				{
					logger.AddString("An error has occurred when getting a screenshot: " + ex2);
					logger.FlagError();
				}
				return false;
			}
		}

		public static bool CheckIsWarframeIsOpen()
		{
			Process[] processesByName = Process.GetProcessesByName("Warframe.x64");
			if (processesByName.Length != 0 && processesByName[0].MainWindowTitle == "Warframe")
			{
				lastWFProcessID = processesByName[0].Id;
				return true;
			}
			lastWFProcessID = -1;
			return false;
		}

		public static float GetScale()
		{
			if (StaticData.WarframeScalingMode == ScalingMode.Custom)
			{
				return StaticData.customScale;
			}
			return 1f;
		}

		public static List<Rectangle> GetOCRRegions(Size bitmapSize, int numberOfPlayers, bool isLegacyScaling)
		{
			List<Rectangle> list = new List<Rectangle>();
			OCRLocationSettings oCRsettings = getOCRsettings(isLegacyScaling, bitmapSize.Height, GetScale());
			float num = 1920f * ((float)bitmapSize.Height / 1080f);
			int num2 = (int)(oCRsettings.rectWdith * num);
			int num3 = bitmapSize.Width / 2;
			int num4 = (int)((float)bitmapSize.Height * oCRsettings.rectTop);
			int num5 = (int)((float)bitmapSize.Height * oCRsettings.rectBottom);
			if (bitmapSize.Width == 2560 && bitmapSize.Height == 1600)
			{
				num2 = (int)((double)num2 * 0.9);
				num4 = (int)((double)num4 * 1.04);
				num5 = (int)((double)num5 * 1.013);
			}
			int num6 = (int)(oCRsettings.rectSeparation * num);
			int num7 = num2 * numberOfPlayers + num6 * (numberOfPlayers - 1);
			for (int i = 0; i < numberOfPlayers; i++)
			{
				Rectangle item = new Rectangle(num3 - num7 / 2 + i * (num2 + num6), num4, num2, num5 - num4);
				list.Add(item);
			}
			return list;
		}

		public static Rectangle GetOverallReliqArea(Size bitmapSize, bool isLegacyScaling)
		{
			new List<Rectangle>();
			OCRLocationSettings oCRsettings = getOCRsettings(isLegacyScaling, bitmapSize.Height, GetScale());
			int num = bitmapSize.Width;
			int num2 = bitmapSize.Height;
			if (isLegacyScaling && num2 < 1080)
			{
				num2 = 1080;
				num = 1920;
			}
			int num3 = bitmapSize.Width / 2;
			float num4 = 1920f * ((float)num2 / 1080f);
			int num5 = (int)(oCRsettings.rectWdith * num4);
			int num6 = (int)((float)bitmapSize.Height * oCRsettings.overallTopScreenPerOne);
			int num7 = (int)((float)bitmapSize.Height * oCRsettings.overallBottomScreenPerOne);
			if (num == 2560 && num2 == 1600)
			{
				num5 = (int)((double)num5 * 0.9);
				num6 = (int)((double)num6 * 1.14);
			}
			int num8 = (int)(oCRsettings.rectSeparation * num4);
			int num9 = num5 * 4 + num8 * 3;
			return new Rectangle(num3 - num9 / 2, num6, num9, (int)((double)(num7 - num6) * 0.9));
		}

		public static Rectangle GetNewCounterArea(Size bitmapSize, bool isLegacyScaling)
		{
			new List<Rectangle>();
			OCRLocationSettings oCRsettings = getOCRsettings(isLegacyScaling, bitmapSize.Height, GetScale());
			_ = bitmapSize.Width;
			int num = bitmapSize.Height;
			if (isLegacyScaling && num < 1080)
			{
				num = 1080;
			}
			int num2 = bitmapSize.Width / 2;
			float num3 = 1920f * ((float)num / 1080f);
			int num4 = (int)(oCRsettings.rectWdith * num3);
			int num5 = (int)((float)bitmapSize.Height * oCRsettings.relicCounterTop);
			int num6 = (int)((float)bitmapSize.Height * oCRsettings.relicCounterBottom);
			int num7 = (int)(oCRsettings.rectSeparation * num3);
			int num8 = num4 * 4 + num7 * 3;
			return new Rectangle(num2 - num8 / 2, num5, num8, (int)((double)(num6 - num5) * 0.9));
		}

		public static void DoRelicRewardRecognitionWork(bool isUILegacyScale, Bitmap originalBitmap, AlecaLogDataLogger logger, bool doRecognitionWork = true)
		{
			List<Bitmap> list = new List<Bitmap>();
			Bitmap bitmap = null;
			try
			{
				logger.AddBitmap(AlecaLogDataLogger.relicLogImageType.original, originalBitmap);
				logger.AddString("Screenshot size: width=" + originalBitmap.Width + ", height=" + originalBitmap.Height);
				if (originalBitmap.Height < 950 || (double)((float)originalBitmap.Width / (float)originalBitmap.Height) < 1.7)
				{
					isUILegacyScale = false;
				}
				bitmap = ((!isUILegacyScale) ? GetAspectRatioCutBitmap(originalBitmap) : ((originalBitmap.Width > 1920 || originalBitmap.Height > 1080) ? originalBitmap : GetAspectRatioCutBitmapLEGACY(originalBitmap)));
				Misc.WarframeLanguage warframeLanguage = Misc.GetWarframeLanguage(defaultToEnglish: true);
				logger.AddString("Detected lang: " + warframeLanguage);
				if (warframeLanguage == Misc.WarframeLanguage.UNKNOWN)
				{
					logger.ForceNotLog();
					throw new Exception("Unsupported Warframe language detected. For the time being, only English, French, Spanish and German are supported.");
				}
				int relicCountNew = GetRelicCountNew(bitmap, logger, isUILegacyScale);
				if (relicCountNew == 0)
				{
					logger.AddString("No players detected, switching to alternative scaling mode...");
					isUILegacyScale = !isUILegacyScale;
					relicCountNew = GetRelicCountNew(bitmap, logger, isUILegacyScale);
					if (relicCountNew == 0)
					{
						logger.AddString("Still no players have been detected after switching scaling mode. Erroring out...");
						throw new UINotDetectedException("Please make sure your WARFRAME SCALING SETTINGS are up to date in AlecaFrame or check FAQ T9 in our Discord for more help.");
					}
				}
				logger.AddString("Detected player count (NEW method): " + relicCountNew);
				logger.AddString("Forcing new relic player detection system...");
				List<Rectangle> oCRRegions = GetOCRRegions(bitmap.Size, relicCountNew, isUILegacyScale);
				for (int i = 0; i < oCRRegions.Count; i++)
				{
					Rectangle srcRect = oCRRegions[i];
					srcRect.Height /= 2;
					Bitmap bitmap2 = new Bitmap(srcRect.Width, srcRect.Height);
					using (Graphics graphics = Graphics.FromImage(bitmap2))
					{
						graphics.DrawImage(bitmap, new Rectangle(0, 0, srcRect.Width, srcRect.Height), srcRect, GraphicsUnit.Pixel);
					}
					list.Add(bitmap2);
					logger.AddBitmap(AlecaLogDataLogger.relicLogImageType.ocrBitmap, bitmap2, $"R{i}_1");
					srcRect.Y += srcRect.Height;
					Bitmap bitmap3 = new Bitmap(srcRect.Width, srcRect.Height);
					using (Graphics graphics2 = Graphics.FromImage(bitmap3))
					{
						graphics2.DrawImage(bitmap, new Rectangle(0, 0, srcRect.Width, srcRect.Height), srcRect, GraphicsUnit.Pixel);
					}
					list.Add(bitmap3);
					logger.AddBitmap(AlecaLogDataLogger.relicLogImageType.ocrBitmap, bitmap3, $"R{i}_2");
				}
				if (!doRecognitionWork)
				{
					logger.AddString("Quitting early because doing recognition is disabled!");
					return;
				}
				logger.AddString("Sending OCR images to server...");
				List<string> list2 = SendBitmapsAndGetLabels(list, logger, warframeLanguage);
				if (list2.Count((string p) => p.ToLower().Contains("loading")) >= 2)
				{
					throw new UINotDetectedException("UI loaded too slow");
				}
				logger.AddString("Final OCR server response: " + JsonConvert.SerializeObject(list2));
				RelicOutputDataClass relicOutputDataClass = new RelicOutputDataClass();
				relicOutputDataClass.relicRewards = new SingleRelicRewardData[relicCountNew];
				bool flag = false;
				TextInfo textInfo = new CultureInfo("en-US", useUserOverride: false).TextInfo;
				int num = 0;
				foreach (string item in list2)
				{
					try
					{
						ItemComponent currentBest = null;
						string text = "";
						float num2 = -1f;
						if (StaticData.dataHandler.relicDropsRealNames[warframeLanguage].ContainsKey(item))
						{
							currentBest = StaticData.dataHandler.relicDropsRealNames[warframeLanguage][item];
							text = item;
							num2 = 0f;
							logger.AddString("InstaDetect (equals) of " + item);
						}
						else
						{
							bool flag2 = false;
							foreach (KeyValuePair<string, ItemComponent> item2 in StaticData.dataHandler.relicDropsRealNames[warframeLanguage])
							{
								if (warframeLanguage != Misc.WarframeLanguage.Russian && item.Contains(item2.Key) && (!flag2 || item2.Key.Length > text.Length))
								{
									flag2 = true;
									currentBest = item2.Value;
									text = item2.Key;
									num2 = 0f;
									logger.AddString("InstaDetect (contains) of " + item + " in " + item2.Key);
								}
							}
							if (!flag2)
							{
								foreach (KeyValuePair<string, ItemComponent> item3 in StaticData.dataHandler.relicDropsRealNames[warframeLanguage])
								{
									float num3 = LevenshteinDistance(item3.Key, item);
									if (num3 < num2 || num2 == -1f)
									{
										num2 = num3;
										currentBest = item3.Value;
										text = item3.Key;
									}
								}
							}
						}
						logger.SetNewDelta((int)num2);
						logger.SetFinalResult(item, currentBest.GetRealExternalName(), currentBest.uniqueName, num2);
						SingleRelicRewardData singleRelicRewardData = new SingleRelicRewardData();
						singleRelicRewardData.itemReference = currentBest;
						singleRelicRewardData.internalName = currentBest.uniqueName;
						singleRelicRewardData.englishName = currentBest.GetRealExternalName();
						singleRelicRewardData.isFav = FavouriteHelper.IsFavourite(currentBest.uniqueName) || FavouriteHelper.IsFavourite(currentBest.isPartOf?.uniqueName);
						singleRelicRewardData.name = textInfo.ToTitleCase(text);
						logger.AddString($"Detected from \"{item}\": {text} ({currentBest.uniqueName}) Delta={num2}");
						singleRelicRewardData.detected = true;
						singleRelicRewardData.ducats = currentBest.ducats;
						BigItem isPartOf = currentBest.isPartOf;
						singleRelicRewardData.isPartOfOwned = (isPartOf != null && isPartOf.IsOwned()) || (currentBest.isPartOf?.IsFullyMastered() ?? false);
						try
						{
							singleRelicRewardData.isPartOfOwned = singleRelicRewardData.isPartOfOwned || currentBest.isPartOf?.components?.Any((ItemComponent p) => StaticData.dataHandler?.warframeRootObject?.PendingRecipes?.Any((Pendingrecipe u) => u.ItemType == p.uniqueName) == true) == true;
						}
						catch
						{
						}
						singleRelicRewardData.errorProb = num2;
						singleRelicRewardData.icon = Misc.GetFullImagePath(currentBest.imageName);
						FoundryItemComponent foundryItemComponent = new FoundryItemComponent(currentBest);
						singleRelicRewardData.totalToOwn = foundryItemComponent.neccessaryAmount;
						singleRelicRewardData.countOwned = 0;
						int.TryParse(foundryItemComponent.quantityOwned, out singleRelicRewardData.countOwned);
						BigItem isPartOf2 = currentBest.isPartOf;
						singleRelicRewardData.isItemVaulted = isPartOf2 != null && isPartOf2.vaulted && !singleRelicRewardData.englishName.Contains("Forma Blueprint");
						if (currentBest.isPartOf != null)
						{
							singleRelicRewardData.componentData.AddRange((from p in currentBest.isPartOf.components?.Where(delegate(ItemComponent p)
								{
									string uniqueName = p.uniqueName;
									if (uniqueName != null && !uniqueName.Contains("/MiscItems/"))
									{
										string uniqueName2 = p.uniqueName;
										if (uniqueName2 != null && !uniqueName2.Contains("/Research/"))
										{
											return p.isPartOf != null;
										}
									}
									return false;
								})
								select new FoundryItemComponent(p, currentBest.uniqueName)));
						}
						if (!singleRelicRewardData.name.Contains("Prime"))
						{
							BigItem isPartOf3 = currentBest.isPartOf;
							if (isPartOf3 != null && !isPartOf3.name.Contains("Prime"))
							{
								singleRelicRewardData.componentData.Clear();
							}
						}
						relicOutputDataClass.relicRewards[num] = singleRelicRewardData;
						logger.AddString("Ended detection of \"" + item + "\": " + currentBest.GetRealExternalName() + " (" + currentBest.uniqueName + ")");
					}
					catch (Exception ex)
					{
						StaticData.Log(OverwolfWrapper.LogType.WARN, "An error has ocurred when testing similarity of " + item + " " + ex);
						logger.AddString("An error has ocurred when testing similarity or data gathering of " + item + " " + ex);
						relicOutputDataClass.relicRewards[num] = new SingleRelicRewardData
						{
							detected = false
						};
						logger.FlagError();
					}
					finally
					{
						num++;
					}
				}
				logger.AddString("All rewards have been detected");
				if (logger.results.Count(((string ocrItem, string readableName, string uniqueName, float currentBestDistance) p) => p.currentBestDistance >= StaticData.MIN_DELTA_TO_CONSIDER_BAD_DETECTION || p.ocrItem.Length < 5) >= 3)
				{
					try
					{
						for (int num4 = 0; num4 < relicOutputDataClass.relicRewards.Length; num4++)
						{
							relicOutputDataClass.relicRewards[num4] = new SingleRelicRewardData
							{
								detected = false
							};
						}
					}
					catch (Exception ex2)
					{
						logger.AddString("Failed to remove items in badDetectionDetected! " + ex2);
						logger.FlagError();
					}
					StaticData.overwolfWrappwer.newRelicDataError = "Could not find any rewards. Please make sure your WARFRAME SCALING SETTINGS are up to date in AlecaFrame or ask for help in Discord.";
					logger.ForceNotLog();
					StaticData.overwolfWrappwer.newRelicDataErrorOccurred = true;
				}
				else if (flag)
				{
					try
					{
						for (int num5 = 0; num5 < relicOutputDataClass.relicRewards.Length; num5++)
						{
							relicOutputDataClass.relicRewards[num5] = new SingleRelicRewardData
							{
								detected = false
							};
						}
					}
					catch (Exception ex3)
					{
						logger.AddString("Failed to remove items in isRequiemRelic! " + ex3);
						logger.FlagError();
					}
					StaticData.overwolfWrappwer.newRelicDataError = "Requiem relics are not supported yet";
					logger.ForceNotLog();
					StaticData.overwolfWrappwer.newRelicDataErrorOccurred = true;
				}
				else
				{
					try
					{
						logger.AddString("Requesting price list from the server...");
						List<string> list3 = relicOutputDataClass.relicRewards.Select((SingleRelicRewardData p) => p.englishName).ToList();
						list3.AddRange(relicOutputDataClass.relicRewards.Select((SingleRelicRewardData p) => Misc.GetSetName(p.itemReference?.isPartOf)));
						OverwolfWrapper.ItemPriceSmallResponse[] array = StaticData.overwolfWrappwer.SYNC_GetHugePriceList(list3.ToArray(), TimeSpan.FromSeconds(10.0));
						logger.AddString("Got price list back from the server");
						for (int num6 = 0; num6 < relicOutputDataClass.relicRewards.Length; num6++)
						{
							relicOutputDataClass.relicRewards[num6].platinum = ((array[num6] == null) ? (-1) : (array[num6].post ?? (-1)));
						}
						for (int num7 = relicOutputDataClass.relicRewards.Length; num7 < relicOutputDataClass.relicRewards.Length * 2; num7++)
						{
							relicOutputDataClass.relicRewards[num7 - relicOutputDataClass.relicRewards.Length].setPlat = ((array[num7] == null) ? (-1) : (array[num7].post ?? (-1)));
						}
					}
					catch (Exception ex4)
					{
						for (int num8 = 0; num8 < relicOutputDataClass.relicRewards.Length; num8++)
						{
							relicOutputDataClass.relicRewards[num8].platinum = -1;
						}
						StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to get price list when doing relic work! " + ex4);
						logger.FlagError();
					}
					StaticData.overwolfWrappwer.newRelicData = relicOutputDataClass;
					StaticData.overwolfWrappwer.newRelicDataErrorOccurred = false;
				}
				try
				{
					relicOutputDataClass.globalData = new RelicDataGlobalData();
					relicOutputDataClass.globalData.platinum = StaticData.dataHandler.warframeRootObject?.PremiumCredits ?? 0;
					relicOutputDataClass.globalData.ducats = (StaticData.dataHandler.warframeRootObject?.MiscItems?.FirstOrDefault((Miscitem p) => p.ItemType == "/Lotus/Types/Items/MiscItems/PrimeBucks")?.ItemCount).GetValueOrDefault();
				}
				catch (Exception ex5)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to get global data when doing relic work! " + ex5);
					logger.FlagError();
				}
			}
			catch (UINotDetectedException ex6)
			{
				throw ex6;
			}
			catch (Exception ex7)
			{
				StaticData.overwolfWrappwer.newRelicDataErrorOccurred = true;
				StaticData.overwolfWrappwer.newRelicDataError = ex7.Message;
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "A general error has ocurred when doing relic work: " + ex7);
				logger.AddString("An general error has ocurred when doing relic work: " + ex7);
				logger.FlagError();
			}
			finally
			{
				try
				{
					foreach (Bitmap item4 in list)
					{
						item4.Dispose();
					}
					bitmap?.Dispose();
				}
				catch
				{
				}
			}
			logger.AddString("Signaling the app that the data is ready...");
			StaticData.overwolfWrappwer.isNewRelicDataReady = true;
			StaticData.overwolfWrappwer.newRelicDataOrErrorReady.Set();
		}

		public static void SendNewInGameConversationNotification(string playerName)
		{
			try
			{
				if (string.IsNullOrEmpty(StaticData.discordNotificationTemplate))
				{
					StaticData.Log(OverwolfWrapper.LogType.INFO, "Discord notification template is empty. Skipping...");
					return;
				}
				StaticData.Log(OverwolfWrapper.LogType.INFO, $"Only background: {StaticData.notificationOnlyBackground}. Is WF in the foreground: {Misc.IsWarframeInTheForeground()}");
				if (StaticData.notificationOnlyBackground && Misc.IsWarframeInTheForeground())
				{
					StaticData.Log(OverwolfWrapper.LogType.INFO, "Notification background conditions not met. Skipping...");
					return;
				}
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Notification background conditions met. Sending notification...");
				try
				{
					if (StaticData.windowsNotificationsEnabled)
					{
						string text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "alecaframe_square.png");
						new ToastContentBuilder().AddText("You have a new in-game conversation!").AddText("Player: " + playerName).AddAppLogoOverride(new Uri("file:///" + text, UriKind.Absolute), ToastGenericAppLogoCrop.None, "AlecaFrame")
							.AddAttributionText("You can disable these notifications in the settings menu of AlecaFrame")
							.Show();
					}
				}
				catch (Exception ex)
				{
					StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to send Windows notification! " + ex);
				}
				if (StaticData.discordNotificationsEnabled && !string.IsNullOrEmpty(StaticData.discordNotificationTemplate))
				{
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.UploadValues(StaticData.discordNotificationWebhook, new NameValueCollection { 
					{
						"content",
						StaticData.discordNotificationTemplate.Replace("<PLAYER_NAME>", playerName)
					} });
				}
			}
			catch (Exception ex2)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to send notification! " + ex2);
			}
		}

		public static void SendTimerNotification(string worldName, string phaseName, string timeLeft)
		{
			SendNotificationBase(timeLeft + " until " + phaseName + " in " + worldName + "!");
		}

		public static void SendFissureNotification(string missionType, string planet, string relicType, TimeSpan timeLeft, bool steelPath)
		{
			SendNotificationBase("New " + (steelPath ? "Steel Path" : "Normal") + " " + relicType.ToUpper() + " void fissure: " + missionType + " (" + planet + "). Until " + DateTime.UtcNow.Add(timeLeft).ToLocalTime().ToShortTimeString(), string.Format("New {0} __**{1}**__ void fissure: __**{2} ({3})**__. Ends <t:{4}:R>", steelPath ? "Steel Path" : "Normal", relicType.ToUpper(), missionType, planet, DateTimeOffset.UtcNow.Add(timeLeft).ToUnixTimeSeconds()));
		}

		public static void SendWFMarketNewMessageNotification(string raw_message)
		{
			if (string.IsNullOrEmpty(StaticData.discordWarframeMarketNotificationTemplate))
			{
				StaticData.Log(OverwolfWrapper.LogType.INFO, "Discord notification template (WFMarket) is empty. Skipping...");
				return;
			}
			string discordNotifTextIfDifferent = StaticData.discordWarframeMarketNotificationTemplate.Replace("<WFM_MESSAGE>", raw_message.Replace("<@", "<").Replace("@", ""));
			SendNotificationBase("New WFMarket message received: " + raw_message, discordNotifTextIfDifferent);
		}

		private static void SendNotificationBase(string normalNotifText, string discordNotifTextIfDifferent = "")
		{
			try
			{
				if (StaticData.windowsNotificationsEnabled)
				{
					string text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "alecaframe_square.png");
					new ToastContentBuilder().AddText(normalNotifText).AddAppLogoOverride(new Uri("file:///" + text, UriKind.Absolute), ToastGenericAppLogoCrop.None, "AlecaFrame").AddAttributionText("You can enable/disable these notifications in the setting menu of AlecaFrame")
						.Show();
				}
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to send Windows notification! " + ex);
			}
			try
			{
				if (StaticData.discordNotificationsEnabled && !string.IsNullOrEmpty(StaticData.discordNotificationWebhook))
				{
					MyWebClient myWebClient = new MyWebClient();
					myWebClient.Proxy = null;
					myWebClient.UploadValues(StaticData.discordNotificationWebhook, new NameValueCollection { 
					{
						"content",
						string.IsNullOrWhiteSpace(discordNotifTextIfDifferent) ? normalNotifText : discordNotifTextIfDifferent
					} });
				}
			}
			catch (Exception ex2)
			{
				StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to send notification! " + ex2);
			}
		}

		private static int GetRelicCountNew(Bitmap SrcImage, AlecaLogDataLogger logger, bool isUILegacyScale)
		{
			bool modifyImagesForLogging = true;
			Rectangle newCounterArea = GetNewCounterArea(SrcImage.Size, isUILegacyScale);
			string text = (isUILegacyScale ? "LEG" : "NOR");
			using (Bitmap bitmap = new Bitmap(newCounterArea.Width, newCounterArea.Height))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.DrawImage(SrcImage, new Rectangle(0, 0, newCounterArea.Width, newCounterArea.Height), newCounterArea, GraphicsUnit.Pixel);
				}
				logger.AddBitmap(AlecaLogDataLogger.relicLogImageType.NewPlayerCount, bitmap);
				using (Bitmap bitmap2 = new Bitmap((int)(0.08f * (float)newCounterArea.Width), bitmap.Height))
				{
					using (Graphics graphics2 = Graphics.FromImage(bitmap2))
					{
						graphics2.DrawImage(bitmap, new Rectangle(0, 0, (int)(0.1f * (float)newCounterArea.Width), newCounterArea.Height), new Rectangle((int)(0.01f * (float)newCounterArea.Width), 0, (int)(0.08f * (float)newCounterArea.Width), newCounterArea.Height), GraphicsUnit.Pixel);
					}
					if (CanLineBeFoundInLineImage(logger, bitmap2, "NN4_1_" + text, modifyImagesForLogging))
					{
						return 4;
					}
				}
				using (Bitmap bitmap3 = new Bitmap((int)(0.08f * (float)newCounterArea.Width), bitmap.Height))
				{
					using (Graphics graphics3 = Graphics.FromImage(bitmap3))
					{
						graphics3.DrawImage(bitmap, new Rectangle(0, 0, (int)(0.1f * (float)newCounterArea.Width), newCounterArea.Height), new Rectangle((int)(0.91f * (float)newCounterArea.Width), 0, (int)(0.08f * (float)newCounterArea.Width), newCounterArea.Height), GraphicsUnit.Pixel);
					}
					if (CanLineBeFoundInLineImage(logger, bitmap3, "NN4_2_" + text, modifyImagesForLogging))
					{
						return 4;
					}
				}
				using (Bitmap bitmap4 = new Bitmap((int)(0.08f * (float)newCounterArea.Width), bitmap.Height))
				{
					using (Graphics graphics4 = Graphics.FromImage(bitmap4))
					{
						graphics4.DrawImage(bitmap, new Rectangle(0, 0, (int)(0.1f * (float)newCounterArea.Width), newCounterArea.Height), new Rectangle((int)(0.15f * (float)newCounterArea.Width), 0, (int)(0.08f * (float)newCounterArea.Width), newCounterArea.Height), GraphicsUnit.Pixel);
					}
					if (CanLineBeFoundInLineImage(logger, bitmap4, "NN3_1_" + text, modifyImagesForLogging))
					{
						return 3;
					}
				}
				using (Bitmap bitmap5 = new Bitmap((int)(0.08f * (float)newCounterArea.Width), bitmap.Height))
				{
					using (Graphics graphics5 = Graphics.FromImage(bitmap5))
					{
						graphics5.DrawImage(bitmap, new Rectangle(0, 0, (int)(0.1f * (float)newCounterArea.Width), newCounterArea.Height), new Rectangle((int)(0.78f * (float)newCounterArea.Width), 0, (int)(0.08f * (float)newCounterArea.Width), newCounterArea.Height), GraphicsUnit.Pixel);
					}
					if (CanLineBeFoundInLineImage(logger, bitmap5, "NN3_2_" + text, modifyImagesForLogging))
					{
						return 3;
					}
				}
				using (Bitmap bitmap6 = new Bitmap((int)(0.08f * (float)newCounterArea.Width), bitmap.Height))
				{
					using (Graphics graphics6 = Graphics.FromImage(bitmap6))
					{
						graphics6.DrawImage(bitmap, new Rectangle(0, 0, (int)(0.1f * (float)newCounterArea.Width), newCounterArea.Height), new Rectangle((int)(0.28f * (float)newCounterArea.Width), 0, (int)(0.08f * (float)newCounterArea.Width), newCounterArea.Height), GraphicsUnit.Pixel);
					}
					if (CanLineBeFoundInLineImage(logger, bitmap6, "NN2_1_" + text, modifyImagesForLogging))
					{
						return 2;
					}
				}
				using (Bitmap bitmap7 = new Bitmap((int)(0.08f * (float)newCounterArea.Width), bitmap.Height))
				{
					using (Graphics graphics7 = Graphics.FromImage(bitmap7))
					{
						graphics7.DrawImage(bitmap, new Rectangle(0, 0, (int)(0.1f * (float)newCounterArea.Width), newCounterArea.Height), new Rectangle((int)(0.665f * (float)newCounterArea.Width), 0, (int)(0.08f * (float)newCounterArea.Width), newCounterArea.Height), GraphicsUnit.Pixel);
					}
					if (CanLineBeFoundInLineImage(logger, bitmap7, "NN2_2_" + text, modifyImagesForLogging))
					{
						return 2;
					}
				}
				using (Bitmap bitmap8 = new Bitmap((int)(0.08f * (float)newCounterArea.Width), bitmap.Height))
				{
					using (Graphics graphics8 = Graphics.FromImage(bitmap8))
					{
						graphics8.DrawImage(bitmap, new Rectangle(0, 0, (int)(0.1f * (float)newCounterArea.Width), newCounterArea.Height), new Rectangle((int)(0.4f * (float)newCounterArea.Width), 0, (int)(0.08f * (float)newCounterArea.Width), newCounterArea.Height), GraphicsUnit.Pixel);
					}
					if (CanLineBeFoundInLineImage(logger, bitmap8, "NN1_1_" + text, modifyImagesForLogging))
					{
						return 1;
					}
				}
				using Bitmap bitmap9 = new Bitmap((int)(0.08f * (float)newCounterArea.Width), bitmap.Height);
				using (Graphics graphics9 = Graphics.FromImage(bitmap9))
				{
					graphics9.DrawImage(bitmap, new Rectangle(0, 0, (int)(0.1f * (float)newCounterArea.Width), newCounterArea.Height), new Rectangle((int)(0.535f * (float)newCounterArea.Width), 0, (int)(0.08f * (float)newCounterArea.Width), newCounterArea.Height), GraphicsUnit.Pixel);
				}
				if (CanLineBeFoundInLineImage(logger, bitmap9, "NN1_2_" + text, modifyImagesForLogging))
				{
					return 1;
				}
			}
			return 0;
		}

		private static bool CanLineBeFoundInLineImage(AlecaLogDataLogger logger, Bitmap mini, string imgIdentifier, bool modifyImagesForLogging)
		{
			bool flag = true;
			int num = mini.Height / 2;
			int num2 = 0;
			Color color = mini.GetPixel(mini.Width / 6, num);
			for (int i = mini.Width / 6; i < mini.Width / 6 * 5; i++)
			{
				Color pixel = mini.GetPixel(i, num);
				if (!(Math.Sqrt(Math.Pow(color.R - pixel.R, 2.0) + Math.Pow(color.G - pixel.G, 2.0) + Math.Pow(color.B - pixel.B, 2.0)) < 45.0))
				{
					break;
				}
				color = pixel;
				num2++;
			}
			float num3 = (float)num2 / ((float)mini.Width * 0.6f);
			logger.AddString(imgIdentifier + " hPixels: " + num3.ToString("0.00"));
			if ((double)num3 < 0.9)
			{
				logger.AddString(imgIdentifier + " hPixels too small!");
				flag = false;
			}
			else
			{
				float[] array = new float[mini.Width];
				for (int j = 0; j < mini.Width; j++)
				{
					color = mini.GetPixel(j, num);
					for (int num4 = num; num4 >= 0; num4--)
					{
						Color pixel2 = mini.GetPixel(j, num4);
						if (!(Math.Sqrt(Math.Pow(color.R - pixel2.R, 2.0) + Math.Pow(color.G - pixel2.G, 2.0) + Math.Pow(color.B - pixel2.B, 2.0)) <= 32.0))
						{
							break;
						}
						color = pixel2;
						mini.SetPixel(j, num4, Color.White);
						array[j] += 1f;
					}
					for (int k = num + 1; k < mini.Height; k++)
					{
						Color pixel3 = mini.GetPixel(j, k);
						if (!(Math.Sqrt(Math.Pow(color.R - pixel3.R, 2.0) + Math.Pow(color.G - pixel3.G, 2.0) + Math.Pow(color.B - pixel3.B, 2.0)) <= 32.0))
						{
							break;
						}
						color = pixel3;
						mini.SetPixel(j, k, Color.White);
						array[j] += 1f;
					}
				}
				double averageHeight = array.Sum() / (float)array.Count();
				double num5 = Math.Sqrt(array.OrderByDescending((float p) => p).Skip(3).Sum((float p) => Math.Pow((double)p - averageHeight, 2.0)) / (double)(array.Count() - 3));
				double num6 = averageHeight / (double)mini.Height;
				double num7 = 5.0 * (num5 / (double)mini.Height);
				logger.AddString(imgIdentifier + " vPixels: nAVG: " + num6.ToString("0.00") + "    nSTD: " + num7.ToString("0.00"));
				if ((float)array.Count((float p) => p <= 1f) > (float)array.Length / 3f)
				{
					logger.AddString(imgIdentifier + " vPixels too many 0s!");
					flag = false;
				}
				else if (num6 > 0.27)
				{
					logger.AddString(imgIdentifier + " vPixelsAVG too much!");
					flag = false;
				}
				else if (num6 < 0.05)
				{
					logger.AddString(imgIdentifier + " vPixelsAVG too small!");
					flag = false;
				}
				else if (num7 > 0.36)
				{
					logger.AddString(imgIdentifier + " vPixelsSTD too much!");
					flag = false;
				}
			}
			using (Graphics graphics = Graphics.FromImage(mini))
			{
				graphics.FillRectangle(flag ? Brushes.Green : Brushes.Red, new Rectangle(0, 0, mini.Width, 4));
			}
			logger.AddBitmap(AlecaLogDataLogger.relicLogImageType.playerCount, mini, imgIdentifier);
			return flag;
		}

		private static Bitmap GetAspectRatioCutBitmap(Bitmap originalBitmap)
		{
			float num = (float)originalBitmap.Width / (float)originalBitmap.Height;
			float num2 = 1.7777778f;
			if (num > num2)
			{
				int num3 = (int)((float)originalBitmap.Width - (float)originalBitmap.Height * num2);
				return originalBitmap.Clone(new Rectangle(num3 / 2, 0, originalBitmap.Width - num3, originalBitmap.Height), PixelFormat.Undefined);
			}
			int num4 = (int)((float)originalBitmap.Height - (float)originalBitmap.Width / num2);
			return originalBitmap.Clone(new Rectangle(0, num4 / 2, originalBitmap.Width, originalBitmap.Height - num4), PixelFormat.Undefined);
		}

		private static Bitmap GetAspectRatioCutBitmapLEGACY(Bitmap originalBitmap)
		{
			_ = (float)originalBitmap.Width / (float)originalBitmap.Height;
			Bitmap bitmap = new Bitmap(1920, 1080);
			using Graphics graphics = Graphics.FromImage(bitmap);
			graphics.DrawImage(originalBitmap, new Rectangle
			{
				Width = originalBitmap.Width,
				Height = originalBitmap.Height,
				X = (bitmap.Width - originalBitmap.Width) / 2,
				Y = (bitmap.Height - originalBitmap.Height) / 2
			});
			return bitmap;
		}

		private static int GetRelicCount(Bitmap SrcImage, AlecaLogDataLogger logger, bool isUILegacyScale)
		{
			Rectangle overallReliqArea = GetOverallReliqArea(SrcImage.Size, isUILegacyScale);
			using Bitmap bitmap = new Bitmap(overallReliqArea.Width, overallReliqArea.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(SrcImage, new Rectangle(0, 0, overallReliqArea.Width, overallReliqArea.Height), overallReliqArea, GraphicsUnit.Pixel);
			}
			using Bitmap bitmap2 = bitmap.Laplacian3x3Filter();
			logger.AddBitmap(AlecaLogDataLogger.relicLogImageType.edgeDetected, bitmap2);
			for (int num = 4; num > 0; num--)
			{
				if (HasNumberOfPlayers(bitmap2, num, logger))
				{
					return num;
				}
			}
			logger.AddString("No player count could be detected!");
			return 0;
		}

		private static bool HasNumberOfPlayers(Bitmap edgeDetected, int playerCountGuess, AlecaLogDataLogger logger)
		{
			using Bitmap bitmap = new Bitmap(edgeDetected.Width / 9, edgeDetected.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(edgeDetected, new Rectangle(0, 0, edgeDetected.Width / 9, edgeDetected.Height), new Rectangle((4 - playerCountGuess) * (edgeDetected.Width / 8), 0, edgeDetected.Width / 9, edgeDetected.Height), GraphicsUnit.Pixel);
			}
			float num = CalculateTotalLiminosityPercent(bitmap, 70f);
			logger.AddString("Total lumi for " + playerCountGuess + " players: " + num);
			return num > 1.75f;
		}

		private static float CalculateTotalLiminosityPercent(Bitmap detectingBitmap, float edgeDetectorThreshold)
		{
			float num = 0f;
			for (int i = 0; i < detectingBitmap.Width; i++)
			{
				for (int j = 0; j < detectingBitmap.Height; j++)
				{
					if ((float)(int)detectingBitmap.GetPixel(i, j).R > edgeDetectorThreshold)
					{
						num += 1f;
					}
				}
			}
			num /= (float)(detectingBitmap.Width * detectingBitmap.Height);
			return num * 100f;
		}

		public static Bitmap Laplacian3x3Filter(this Bitmap sourceBitmap, bool grayscale = true)
		{
			double[,] array = new double[3, 3]
			{
				{ -1.0, -1.0, -1.0 },
				{ -1.0, 8.0, -1.0 },
				{ -1.0, -1.0, -1.0 }
			};
			BitmapData bitmapData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			byte[] array2 = new byte[bitmapData.Stride * bitmapData.Height];
			byte[] array3 = new byte[bitmapData.Stride * bitmapData.Height];
			Marshal.Copy(bitmapData.Scan0, array2, 0, array2.Length);
			sourceBitmap.UnlockBits(bitmapData);
			if (grayscale)
			{
				float num = 0f;
				for (int i = 0; i < array2.Length; i += 4)
				{
					num = (float)(int)array2[i] * 0.11f;
					num += (float)(int)array2[i + 1] * 0.59f;
					num += (float)(int)array2[i + 2] * 0.3f;
					array2[i] = (byte)num;
					array2[i + 1] = array2[i];
					array2[i + 2] = array2[i];
					array2[i + 3] = byte.MaxValue;
				}
			}
			int length = array.GetLength(1);
			array.GetLength(0);
			int num2 = (length - 1) / 2;
			for (int j = num2; j < sourceBitmap.Height - num2; j++)
			{
				for (int k = num2; k < sourceBitmap.Width - num2; k++)
				{
					double num3 = 0.0;
					double num4 = 0.0;
					double num5 = 0.0;
					int num6 = j * bitmapData.Stride + k * 4;
					for (int l = -num2; l <= num2; l++)
					{
						for (int m = -num2; m <= num2; m++)
						{
							int num7 = num6 + m * 4 + l * bitmapData.Stride;
							num3 += (double)(int)array2[num7] * array[l + num2, m + num2];
							num4 += (double)(int)array2[num7 + 1] * array[l + num2, m + num2];
							num5 += (double)(int)array2[num7 + 2] * array[l + num2, m + num2];
						}
					}
					if (num3 > 255.0)
					{
						num3 = 255.0;
					}
					else if (num3 < 0.0)
					{
						num3 = 0.0;
					}
					if (num4 > 255.0)
					{
						num4 = 255.0;
					}
					else if (num4 < 0.0)
					{
						num4 = 0.0;
					}
					if (num5 > 255.0)
					{
						num5 = 255.0;
					}
					else if (num5 < 0.0)
					{
						num5 = 0.0;
					}
					array3[num6] = (byte)num3;
					array3[num6 + 1] = (byte)num4;
					array3[num6 + 2] = (byte)num5;
					array3[num6 + 3] = byte.MaxValue;
				}
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			Marshal.Copy(array3, 0, bitmapData2.Scan0, array3.Length);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		public static List<string> SendBitmapsAndGetLabels(List<Bitmap> bitmaps, AlecaLogDataLogger logger, Misc.WarframeLanguage warframeLanguage)
		{
			try
			{
				List<byte[]> list = new List<byte[]>();
				foreach (Bitmap bitmap in bitmaps)
				{
					using Bitmap detectingBitmap = bitmap.Laplacian3x3Filter();
					float num = CalculateTotalLiminosityPercent(detectingBitmap, 70f);
					logger?.AddString("Mini bitmap " + list.Count + " luminosity: " + num);
					if (num < 5f && list.Count % 2 == 0)
					{
						logger?.AddString("Ignoring Mini bitmap " + list.Count + "...");
						list.Add(new byte[0]);
					}
					else
					{
						list.Add(ImageToByte2(bitmap));
					}
				}
				MyWebClient myWebClient = new MyWebClient();
				myWebClient.Proxy = null;
				myWebClient.Headers.Add("ReqDesc", string.Join("|", list.Select((byte[] p) => p.Length)));
				if (warframeLanguage == Misc.WarframeLanguage.Russian)
				{
					myWebClient.Headers.Add("cyrillic", "yes");
				}
				MemoryStream memoryStream = new MemoryStream();
				foreach (byte[] item in list)
				{
					memoryStream.Write(item, 0, item.Length);
				}
				logger?.AddString("OCR Upload start!");
				string text = Encoding.UTF8.GetString(myWebClient.UploadData(StaticData.MLAPIHostname + "/submitImage", memoryStream.ToArray()));
				logger?.AddString("Got raw response: " + text);
				List<string> list2 = (from p in JsonConvert.DeserializeObject<string[]>(text)
					select (!string.IsNullOrEmpty(p)) ? p : "" into p
					select RemoveNotAllowedCharactersInRelicRewardNames(p).Trim()).ToList();
				List<string> list3 = new List<string>();
				for (int num2 = 0; num2 < list2.Count; num2 += 2)
				{
					string[] source = string.Join(" ", list2[num2], list2[num2 + 1]).Split(' ');
					string text2 = string.Join(" ", source.Where((string p) => p.Length > 2 || p.ToLower().Contains("lex") || p == "&" || p.ToLower().Contains("ash") || p.ToLower().Contains("nyx") || p.ToLower().Contains("mag") || (p.ToLower() == "de" && warframeLanguage == Misc.WarframeLanguage.Spanish)));
					text2 = Misc.RemoveDiacritics(text2.Replace("Fanq", "Fang"));
					list3.Add(text2.ToLower());
				}
				return list3;
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "An error has ocurred when submitting pictures to the server: " + ex.Message);
				logger?.AddString("An error has ocurred when submitting pictures to the server: " + ex.Message);
				throw;
			}
		}

		public static string SendBitmapAndGetLabelsRAW(Bitmap bitmap)
		{
			try
			{
				MyWebClient myWebClient = new MyWebClient();
				myWebClient.Proxy = null;
				MemoryStream memoryStream = new MemoryStream();
				byte[] array = ImageToByte2(bitmap);
				myWebClient.Headers.Add("ReqDesc", array.Length.ToString());
				myWebClient.Headers.Add("FullRecognize", "yes");
				memoryStream.Write(array, 0, array.Length);
				List<string> values = (from p in JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(myWebClient.UploadData(StaticData.MLAPIHostname + "/submitImage", memoryStream.ToArray())))
					select (!string.IsNullOrEmpty(p)) ? p : "" into p
					select RemoveNotAllowedCharactersInRelicRewardNames(p).Trim()).ToList();
				return string.Join(" ", values);
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "An error has ocurred when submitting pictures to the server (RAW): " + ex.Message);
				return " ";
			}
		}

		public static string SendRAWRivenOCR(Bitmap bitmap)
		{
			try
			{
				MyWebClient myWebClient = new MyWebClient();
				myWebClient.Proxy = null;
				return JsonConvert.DeserializeObject<OCRResult>(Encoding.UTF8.GetString(myWebClient.UploadData(StaticData.MLAPIHostname + "/new/ocr/single", ImageToByte2(bitmap)))).text;
			}
			catch (Exception ex)
			{
				StaticData.Log(OverwolfWrapper.LogType.ERROR, "An error has ocurred when submitting pictures to the server (Riven RAW): " + ex.Message);
				throw;
			}
		}

		public static string RemoveNotAllowedCharactersInRelicRewardNames(string originalName)
		{
			foreach (char notAllowedCharsInRelicRewardName in notAllowedCharsInRelicRewardNames)
			{
				originalName = originalName.Replace(notAllowedCharsInRelicRewardName.ToString(), "");
			}
			originalName = originalName.Replace("FF", "");
			originalName = originalName.Replace("F ", "");
			originalName = originalName.Replace("LL", "");
			originalName = originalName.Replace("L ", "");
			return originalName;
		}

		public static byte[] ImageToByte2(Image img)
		{
			using MemoryStream memoryStream = new MemoryStream();
			img.Save(memoryStream, ImageFormat.Jpeg);
			return memoryStream.ToArray();
		}

		public static int LevenshteinDistance(string source, string target)
		{
			if (source == target)
			{
				return 0;
			}
			if (source.Length == 0)
			{
				return target.Length;
			}
			if (target.Length == 0)
			{
				return source.Length;
			}
			int[] array = new int[target.Length + 1];
			int[] array2 = new int[target.Length + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = i;
			}
			for (int j = 0; j < source.Length; j++)
			{
				array2[0] = j + 1;
				for (int k = 0; k < target.Length; k++)
				{
					int num = ((source[j] != target[k]) ? 1 : 0);
					array2[k + 1] = Math.Min(array2[k] + 1, Math.Min(array[k + 1] + 1, array[k] + num));
				}
				for (int l = 0; l < array.Length; l++)
				{
					array[l] = array2[l];
				}
			}
			return array2[target.Length];
		}
	}
}
