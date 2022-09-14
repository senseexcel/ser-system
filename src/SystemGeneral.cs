namespace Ser.Gerneral
{
    #region Usings
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
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

        public static bool IsNumeric(string input, NumberStyles numberStyle)
        {
            if (input.StartsWith("0"))
                return false;
            return Double.TryParse(input, numberStyle, CultureInfo.CurrentCulture, out _);
        }

        public static byte[] ImageToByteArray(Image img)
        {
            return (byte[])TypeDescriptor.GetConverter(img).ConvertTo(img, typeof(byte[]));
        }

        public static string GetFullPathFromApp(string path)
        {
            try
            {
                if (String.IsNullOrEmpty(path))
                    return null;
                if (path.StartsWith("/"))
                    return path;
                if (!path.StartsWith("\\\\") && !path.Contains(":") && !path.StartsWith("%"))
                    path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).TrimEnd('\\'), path.TrimStart('\\'));
                path = Environment.ExpandEnvironmentVariables(path);
                return Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                throw new Exception($"The full path '{path}' of the app could not resolve.", ex);
            }
        }
    }
}