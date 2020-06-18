namespace Ser.Diagnostics
{
    #region Usings
    using NLog;
    using System;
    using System.Diagnostics;
    using System.Threading;
    #endregion

    public class SystemHealth
    {
        #region Logger
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Public Methods
        public static int GetCoreCountForCalc(int taskCoreCount, int taskCount)
        {
            try
            {
                var systemCoreCount = Environment.ProcessorCount;
                systemCoreCount--;
                var result = 1;
                if (taskCoreCount <= 0)
                    result = Convert.ToInt32(Math.Ceiling((Convert.ToDouble(systemCoreCount) / Convert.ToDouble(taskCount)))) - 3;
                if (result <= 0)
                    result = 1;
                if (taskCoreCount > 0 && systemCoreCount - 1 > taskCoreCount)
                    result = taskCoreCount;
                logger.Info($"The calculation uses {result} CPU cores.");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ">>>The calculation cores of the System could not be read. The calculation runs with 1 core.<<<");
                return 1;
            }
        }

        public static int GetCoreCountForTask(int taskCoreCount, int taskCount)
        {
            try
            {
                var systemCoreCount = Environment.ProcessorCount;
                logger.Debug($"The system has {systemCoreCount} CPU cores.");
                systemCoreCount--;
                var result = 1;
                if (taskCoreCount <= -1)
                    taskCoreCount = systemCoreCount;

                if (taskCoreCount > 1)
                {
                    if (systemCoreCount >= taskCount * 2)
                        result = taskCount;
                    else
                    {
                        result = Convert.ToInt32(Math.Round(Convert.ToDouble(systemCoreCount) / Convert.ToDouble(taskCount), 0));
                        if (result <= 1)
                            result = 1;
                    }
                }

                logger.Info($"The engine uses {result} CPU cores.");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ">>>The CPU cores of the System could not be read. The engine runs with 1 core.<<<");
                return 1;
            }
        }

        public static bool UseMemory(double userMemoryLimit = -1)
        {
            try
            {
                if (userMemoryLimit == -1)
                {
                    logger.Debug("Use the full memory of the system. No memory limit.");
                    return true;
                }

                logger.Debug("Memory limit is use.");
                var engineProc = Process.GetCurrentProcess();
                double procGbSize = Convert.ToDouble(engineProc?.PrivateMemorySize64) / 1024 / 1024 / 1024;
                double procent = procGbSize * 100 / userMemoryLimit;
                logger.Trace($"Use {procent} of memory.");
                if (procent >= 90)
                {
                    logger.Debug($"The memory size for the engine is limited by {userMemoryLimit} GB.");
                    return false;
                }
                logger.Trace("Used memory is O.K.");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ">>>The memory limit could not be calculated.<<<");
                return true;
            }
        }

        public static void ThrowIfCanceledByTimoutorUser(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                throw new OperationCanceledException("Task was terminated by timeout or user.");
        }
        #endregion
    }
}