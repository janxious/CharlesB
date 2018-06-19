using System;
using System.IO;
using static CharlesB.CharlesB;

namespace CharlesB
{
    public class Logger
    {
        private static string LogFilePath => $"{ModDirectory}/{Settings.ModName}.log";

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
            if (!CharlesB.ModSettings.debug) return;
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