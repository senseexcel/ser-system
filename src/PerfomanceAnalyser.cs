namespace Ser.Diagnostics
{
    #region Usings
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Newtonsoft.Json;
    #endregion

    public class PerfomanceAnalyser
    {
        #region Properties && Variables
        private DateTime starttime;
        private readonly static object locker = new object();
        private readonly ConcurrentStack<AnalyserCheckPoint> checkPoints = new ConcurrentStack<AnalyserCheckPoint>();
        public AnalyserOptions Options { get; private set; }
        #endregion

        #region Constructor
        public PerfomanceAnalyser(AnalyserOptions options = null)
        {
            if (options == null)
                options = new AnalyserOptions();

            Options = options;
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            starttime = DateTime.Now;
        }

        public void Stop(string analyserFolder)
        {
            if (!Directory.Exists(analyserFolder))
                throw new Exception($"The perfomance analyser folder {analyserFolder} does not exists.");

            var results = new List<AnalyserCheckPoint>();
            var files = Directory.GetFiles(analyserFolder, "*.perfc", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var json = File.ReadAllText(file, Encoding.UTF8);
                results.AddRange(JsonConvert.DeserializeObject<List<AnalyserCheckPoint>>(json));
                File.Delete(file);
            }

            foreach (var item in results)
                item.SortSpan = (item.Stemp - starttime).TotalSeconds;

            results = results.OrderBy(c => c.SortSpan).ToList();

            var savePath = Path.Combine(analyserFolder, $"analyser.perf");
            using (var csvWriter = new StreamWriter(savePath, false, Encoding.UTF8))
            {
                var headers = new List<string> { "Time", "Modul", "Action", "Message" };
                csvWriter.WriteLine(String.Join(Options.Seperator, headers));
                foreach (var checkpoint in results)
                {
                    var rows = new List<string>
                        {
                            checkpoint.SortSpan.ToString(),
                            checkpoint.Modulname,
                            checkpoint.Action,
                            checkpoint.Message
                        };
                    csvWriter.WriteLine(String.Join(Options.Seperator, rows));
                }
                csvWriter.Flush();
            }
        }

        public void SetCheckPoint(string action, string message)
        {
            checkPoints.Push(new AnalyserCheckPoint()
            {
                Stemp = DateTime.Now,
                Action = action,
                Message = message
            });
        }

        public void SaveCheckPoints(string saveFolder, string modulName)
        {
            if (String.IsNullOrEmpty(saveFolder))
                throw new Exception("The perfomance analyser has no folder path.");
            lock (locker)
            {
                foreach (var checkPoint in checkPoints)
                    checkPoint.Modulname = modulName;
                var savePath = Path.Combine(saveFolder, $"{modulName}.perfc");
                var json = JsonConvert.SerializeObject(checkPoints, Formatting.Indented);
                File.WriteAllText(savePath, json, Encoding.UTF8);
            }
        }
        #endregion
    }

    #region Helper Class
    public class AnalyserOptions
    {
        public char Seperator { get; set; } = '|';
    }

    internal class AnalyserCheckPoint
    {
        internal double SortSpan { get; set; }
        public string Action { get; set; }
        public string Modulname { get; set; }
        public string Message { get; set; }
        public DateTime Stemp { get; set; }
    }
    #endregion
}