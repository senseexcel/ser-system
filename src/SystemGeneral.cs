namespace Ser.Gerneral
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using NLog;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    #endregion

    public class SystemGeneral
    {
        public static string GetLogFileName(string targetName)
        {
            if (LogManager.Configuration != null && LogManager.Configuration.ConfiguredNamedTargets.Count != 0)
            {
                Target target = LogManager.Configuration.FindTargetByName(targetName);
                if (target == null)
                    throw new Exception($"Could not find target name '{targetName}'.");

                FileTarget fileTarget;
                if (!(target is WrapperTargetBase wrapperTarget))
                    fileTarget = target as FileTarget;
                else
                    fileTarget = wrapperTarget.WrappedTarget as FileTarget;

                if (fileTarget == null)
                    throw new Exception($"Could not get a FileTarget from '{target.GetType()}'.");

                var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
                return fileTarget.FileName.Render(logEventInfo);
            }
            else
            {
                throw new Exception("LogManager contains no Configuration or there are no named targets");
            }
        }

        public static string GetAssemblyVersion(Type type)
        {
            return type.Assembly.GetName().Version.ToString();
        }
    }
}