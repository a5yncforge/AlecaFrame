using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace AlecaFramePublicLib
{
	public static class ExtensionMethods
	{
		public static string FromTo(this string str, string from, string to, int initialIndex = 0)
		{
			string result = "";
			if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
			{
				return result;
			}
			int num = str.IndexOf(from, initialIndex);
			int num2 = str.IndexOf(to, num + from.Length);
			if (num != -1 && num2 != -1 && num <= num2)
			{
				result = str.Substring(num + from.Length, num2 - num - from.Length);
			}
			return result;
		}

		public static string FirstToUpper(this string input)
		{
			if (input == null)
			{
				return "";
			}
			if (input.Length == 1)
			{
				return input.ToUpper();
			}
			return input.First().ToString().ToUpper() + input.Substring(1);
		}

		public static string OnlyFirstToUpper(this string input)
		{
			if (input == null)
			{
				return "";
			}
			if (input.Length == 1)
			{
				return input.ToUpper();
			}
			return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
		}

		public static string ToShortByteString(this long input)
		{
			if (input > 1000000000)
			{
				return Math.Round((double)input / 1000000000.0, 2) + " GB";
			}
			if (input > 1000000)
			{
				return Math.Round((double)input / 1000000.0, 2) + " MB";
			}
			if (input > 1000)
			{
				return Math.Round((double)input / 1000.0, 2) + " KB";
			}
			return input + " B";
		}

		public static string ToShortByteString(this int input)
		{
			return ((long)input).ToShortByteString();
		}

		public static string getFileAndLine(this Exception ex)
		{
			StackTrace stackTrace = new StackTrace(ex, fNeedFileInfo: true);
			if (stackTrace.FrameCount == 0)
			{
				return "Uknown error file & line";
			}
			StackFrame frame = stackTrace.GetFrame(0);
			return $"Error in file \"{frame.GetFileName()}\" at line {frame.GetFileLineNumber()} ({frame.GetFileColumnNumber()})";
		}

		public static string toHEX(this Color c)
		{
			return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
		}

		public static long ToUnixTimestamp(this DateTime value)
		{
			return (long)Math.Truncate(value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
		}

		public static bool readNumberOfBytes(this Stream stream, ref byte[] buffer, int bytesToRead, int timeoutMiliseconds = 5000)
		{
			DateTime utcNow = DateTime.UtcNow;
			try
			{
				int num = 0;
				while (num < bytesToRead || (DateTime.UtcNow - utcNow).TotalMilliseconds > (double)timeoutMiliseconds)
				{
					num += stream.Read(buffer, num, bytesToRead - num);
					Thread.Sleep(10);
				}
				return (DateTime.UtcNow - utcNow).TotalMilliseconds < (double)timeoutMiliseconds;
			}
			catch
			{
				return false;
			}
		}

		public static string getPublicIP(this WebClient webClient)
		{
			try
			{
				return webClient.DownloadString("http://api.ipify.org/");
			}
			catch
			{
				return "0.0.0.0";
			}
		}

		public static bool ContainsCaseInsensitive(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
		{
			return text.IndexOf(value, stringComparison) >= 0;
		}

		public static int ToSafeInt(this string str, int onErrorValue = 0)
		{
			if (string.IsNullOrEmpty(str))
			{
				return onErrorValue;
			}
			int result = onErrorValue;
			int.TryParse(str, out result);
			return result;
		}

		public static long ToSafeLong(this string str, long onErrorValue = 0L)
		{
			if (string.IsNullOrEmpty(str))
			{
				return onErrorValue;
			}
			long result = onErrorValue;
			long.TryParse(str, out result);
			return result;
		}

		public static double ToSafeDouble(this string str, double onErrorValue = 0.0)
		{
			if (string.IsNullOrEmpty(str))
			{
				return onErrorValue;
			}
			double result = onErrorValue;
			double.TryParse(str, out result);
			return result;
		}

		public static V GetOrDefault<K, V>(this Dictionary<K, V> dict, K key)
		{
			if (key == null)
			{
				return default(V);
			}
			if (dict.TryGetValue(key, out var value))
			{
				return value;
			}
			return default(V);
		}

		public static V GetOrDefaultExplicit<K, V>(this Dictionary<K, V> dict, K key, V defaultValue)
		{
			if (dict.TryGetValue(key, out var value))
			{
				return value;
			}
			return defaultValue;
		}
	}
}
