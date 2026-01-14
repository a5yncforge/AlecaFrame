using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace AlecaFrameClientLib
{
    public class AlecaLogDataLogger : IDisposable
    {
        public enum AlecaLogType
        {
            relic,
            riven
        }

        public enum relicLogImageType
        {
            original,
            edgeDetected,
            playerCount,
            ocrBitmap,
            final,
            NewPlayerCount
        }

        private readonly string outputPath;

        private readonly Action<relicLogImageType, Bitmap> otherLoggerMethod;

        private readonly bool doNotLog;

        private Dictionary<string, MemoryStream> bitmapsToLog = new Dictionary<string, MemoryStream>();

        private StreamWriter logWriter;

        private MemoryStream logData;

        private Stopwatch stopwatch = new Stopwatch();

        private bool alreadyDisposed;

        private bool errorExists;

        private int worstDelta;

        private bool forceNotLog;

        private AlecaLogType logType;

        public List<(string ocrItem, string readableName, string uniqueName, float currentBestDistance)> results = new List<(string, string, string, float)>();

        public AlecaLogDataLogger(AlecaLogType logType, string outputPath, Action<relicLogImageType, Bitmap> _otherLoggerMethod, bool _doNotLog)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            logData = new MemoryStream();
            logWriter = new StreamWriter(logData);
            alreadyDisposed = false;
            this.outputPath = outputPath;
            otherLoggerMethod = _otherLoggerMethod;
            doNotLog = _doNotLog;
            this.logType = logType;
            logWriter.WriteLine("Initialized RelicDataLogger");
        }

        public void AddBitmap(relicLogImageType imageType, Bitmap bitmap, string extraLogData = "")
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Jpeg);
            string key = $"{imageType}" + (string.IsNullOrEmpty(extraLogData) ? "" : ("_" + extraLogData)) + ".jpeg";
            if (bitmapsToLog.ContainsKey(key))
            {
                bitmapsToLog[key] = memoryStream;
            }
            else
            {
                bitmapsToLog.Add(key, memoryStream);
            }
            if (otherLoggerMethod != null)
            {
                otherLoggerMethod(imageType, bitmap);
            }
        }

        public void AddString(string toAdd)
        {
            string text = string.Format("[{0}.{1}] {2}", stopwatch.Elapsed.Seconds, stopwatch.ElapsedMilliseconds.ToString("000"), toAdd);
            Console.WriteLine("Adding to relic log: " + text);
            logWriter.WriteLine(text);
        }

        public void Dispose()
        {
            if (alreadyDisposed)
            {
                return;
            }
            alreadyDisposed = true;
            AddString("-|Results:");
            foreach (var result in results)
            {
                AddString($"{result.ocrItem}|{result.readableName}|{result.uniqueName}|{result.currentBestDistance}");
            }
            if (worstDelta >= StaticData.DELTA_BAD_ENOUGH_TO_LOG)
            {
                AddString($"Worst delta ({worstDelta}) exceeded. This relic will probably be logged.");
                FlagError();
            }
            try
            {
                stopwatch.Stop();
                logWriter.WriteLine("Disposing RelicDataLogger...");
                logWriter.Flush();
                logData.Seek(0L, SeekOrigin.Begin);
                using (StreamReader streamReader = new StreamReader(logData, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 4096, leaveOpen: true))
                {
                    StaticData.lastRelicLogText = streamReader.ReadToEnd();
                }
                if (doNotLog)
                {
                    StaticData.shouldLogLastRelic = false;
                    return;
                }
                DateTime utcNow = DateTime.UtcNow;
                string text = utcNow.Year.ToString("0000") + "_" + utcNow.Month.ToString("00") + "_" + utcNow.Day.ToString("00") + "_" + utcNow.Hour.ToString("00") + "_" + utcNow.Minute.ToString("00") + "_" + utcNow.Second.ToString("00");
                Directory.CreateDirectory(outputPath);
                string text2 = ((logType == AlecaLogType.relic) ? "RELIC" : "RIVEN");
                string text3 = outputPath + "/" + text + "_" + text2 + ".alecalog";
                using (FileStream fileStream = File.Open(text3, FileMode.Create))
                {
                    ZipArchive val = new ZipArchive((Stream)fileStream, (ZipArchiveMode)1);
                    try
                    {
                        foreach (KeyValuePair<string, MemoryStream> item in bitmapsToLog)
                        {
                            using Stream destination = val.CreateEntry(item.Key).Open();
                            item.Value.Seek(0L, SeekOrigin.Begin);
                            item.Value.CopyTo(destination);
                        }
                        using Stream destination2 = val.CreateEntry("log.txt").Open();
                        logData.Seek(0L, SeekOrigin.Begin);
                        logData.CopyTo(destination2);
                    }
                    finally
                    {
                        ((IDisposable)val)?.Dispose();
                    }
                }
                StaticData.lastRelicLogFilePath = text3;
                StaticData.shouldLogLastRelic = errorExists && !forceNotLog;
                StaticData.lastRelicLogWorstDelta = worstDelta;
            }
            catch (Exception ex)
            {
                StaticData.Log(OverwolfWrapper.LogType.WARN, "Failed to save alecalog: " + ex);
            }
            finally
            {
                logData.Dispose();
                foreach (KeyValuePair<string, MemoryStream> item2 in bitmapsToLog)
                {
                    item2.Value.Dispose();
                }
            }
        }

        public void FlagError()
        {
            AddString("- - - - Error detected!");
            errorExists = true;
        }

        public void SetNewDelta(int newDelta)
        {
            if (newDelta > worstDelta)
            {
                worstDelta = newDelta;
            }
        }

        public void ForceNotLog()
        {
            forceNotLog = true;
        }

        public void SetFinalResult(string ocrItem, string readableName, string uniqueName, float currentBestDistance)
        {
            results.Add((ocrItem, readableName, uniqueName, currentBestDistance));
        }
    }
}
