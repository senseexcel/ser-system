namespace Ser.Diagnostics
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    #endregion

    public class PerfomanceAnalyser
    {
        #region Properties && Variables
        private static DateTime starttime;
        private static readonly object locker = new object();
        private static readonly List<AnalyserMessage> messages = new List<AnalyserMessage>();
        public static string FilePath { get; set; }
        public static char Seperator { get; set; } = '|';
        #endregion

        #region Public Methods
        public static void Start()
        {
            if (String.IsNullOrEmpty(FilePath))
                throw new Exception("The perfomance analyser has no filepath.");
            if (Path.GetExtension(FilePath) != ".perf")
                throw new Exception("The perfomance analyser file extention must be '.perf'.");
            starttime = DateTime.Now;
        }

        public static void Stop()
        {
            lock (locker)
            {
                using (var csvWriter = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    csvWriter.WriteLine($"Time{Seperator}Modul{Seperator}StatusNum{Seperator}Message");
                    foreach (var message in messages)
                    {
                        var span = message.Stemp - starttime;
                        csvWriter.WriteLine($"{span.TotalSeconds}{Seperator}{message.Modulname}{Seperator}{message.StatusNumber}{Seperator}{message.Value}");
                    }
                    csvWriter.Flush();
                }
            }
        }

        public static void WriteMessage(int statusNumber, string modulName, string message)
        {
            messages.Add(new AnalyserMessage() 
            { 
                Stemp = DateTime.Now, 
                StatusNumber = statusNumber, 
                Modulname = modulName, 
                Value = message 
            });
        }
        #endregion
    }

    #region Helper Class
    internal class AnalyserMessage
    {
        public int StatusNumber { get; set; }
        public string Modulname { get; set; }
        public string Value { get; set; }
        public DateTime Stemp { get; set; }
    }
    #endregion
}