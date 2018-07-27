using System;
using System.IO;
using Harmony;

namespace CharlesB
{
    public class Logger
    {
        private static string LogFilePath     => $"{Core.ModDirectory}/{Core.ModName}.log";
        private static string FileLogFilePath => $"{Core.ModDirectory}/{Core.ModName}.harmony.log";

        public static void Setup()
        {
            FileLog.logPath = FileLogFilePath;
        }

        public static void Error(Exception ex)
        {
            using (var writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine($"Message: {ex.Message}");
                writer.WriteLine($"StackTrace: {ex.StackTrace}");
                WriteLogFooter(writer);
            }
        }

        public static void Debug(String line)
        {
            if (!Core.ModSettings.Debug) return;
            using (var writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine(line);
                WriteLogFooter(writer);
            }
        }

        private static void WriteLogFooter(StreamWriter writer)
        {
            writer.WriteLine($"Date: {DateTime.Now}");
            writer.WriteLine(new string(c: '-', count: 50));
        }
    }
}