namespace Ser.Diagnostics
{
    #region Usings
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            try
            {
                starttime = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw new Exception("The perfomance analyser could not start.", ex);
            }
        }

        public void Stop()
        {
            try
            {
                var analyserFolder = Path.Combine(Options.AnalyserFolder, "PerfAnalyser");
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

                Directory.Delete(analyserFolder, true);
                foreach (var item in results)
                    item.SortSpan = (item.Stemp - starttime).TotalSeconds;

                results = results.OrderBy(c => c.SortSpan).ToList();

                var savePath = Path.Combine(Options.AnalyserFolder, $"analyser.perf");
                using (var csvWriter = new StreamWriter(savePath, false, Encoding.UTF8))
                {
                    var headers = new List<string> { "Time", "Modul", "Action", "Message", "CPUTime", "Memory" };
                    csvWriter.WriteLine(String.Join(Options.Seperator, headers));
                    foreach (var checkpoint in results)
                    {
                        var rows = new List<string>
                        {
                            checkpoint.SortSpan.ToString(),
                            checkpoint.Modulname,
                            checkpoint.Action,
                            checkpoint.Message,
                            checkpoint.CPUTime.ToString(),
                            checkpoint.Memory.ToString()
                        };
                        csvWriter.WriteLine(String.Join(Options.Seperator, rows));
                    }
                    csvWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("The perfomance analyser could not stop.", ex);
            }
        }

        public void SetCheckPoint(string action, string message)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                checkPoints.Push(new AnalyserCheckPoint()
                {
                    Stemp = DateTime.Now,
                    Action = action,
                    Message = message,
                    CPUTime = process.TotalProcessorTime.TotalSeconds,
                    Memory = process.WorkingSet64
                });
            }
            catch (Exception ex)
            {
                throw new Exception("The perfomance analyser could not add the checkpiont.", ex);
            }
        }

        public void SaveCheckPoints(string modulName)
        {
            try
            {
                var analyserFolder = Path.Combine(Options.AnalyserFolder, "PerfAnalyser");
                lock (locker)
                {
                    Directory.CreateDirectory(analyserFolder);
                    foreach (var checkPoint in checkPoints)
                        checkPoint.Modulname = modulName;
                    var savePath = Path.Combine(analyserFolder, $"{modulName}.perfc");
                    var json = JsonConvert.SerializeObject(checkPoints, Formatting.Indented);
                    File.WriteAllText(savePath, json, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("The perfomance analyser could not save checkpionts.", ex);
            }
        }
        #endregion
    }

    #region Helper Class
    public class AnalyserOptions
    {
        public char Seperator { get; set; } = '|';
        public string AnalyserFolder { get; set; } = Path.Combine(Path.GetTempPath(), "PerfAnalyser");
    }

    internal class AnalyserCheckPoint
    {
        internal double SortSpan { get; set; }
        public string Action { get; set; }
        public string Modulname { get; set; }
        public string Message { get; set; }
        public DateTime Stemp { get; set; }
        public long Memory { get; set; }
        public double CPUTime { get; set; }
    }
    #endregion
}