using System;
using System.IO;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MQTTPlugin
{
  public static class Logger
  {
    private static string logFilename = Config.GetFile(Config.Dir.Log, "MQTTPlugin.log");
    private static string backupFilename = Config.GetFile(Config.Dir.Log, "MQTTPlugin.bak");
    private static object lockObject = new object();

    static Logger()
    {
      if (File.Exists(logFilename))
      {
        if (File.Exists(backupFilename))
        {
          try
          {
            File.Delete(backupFilename);
          }
          catch
          {
            Error("Failed to remove old backup log");
          }
        }
        try
        {
          File.Move(logFilename, backupFilename);
        }
        catch
        {
          Error("Failed to move logfile to backup");
        }
      }
    }

    public static void Info(String log)
    {
      writeToFile(String.Format(createPrefix(), "Info", log));
    }

    public static void Debug(String log)
    {
      writeToFile(String.Format(createPrefix(), "Debug", log));
    }

    public static void Error(String log)
    {
      writeToFile(String.Format(createPrefix(), "Error", log));
      Log.Error("MQTTPlugin: " + log);
    }

    public static void Warning(String log)
    {
      writeToFile(String.Format(createPrefix(), "Warning", log));
    }

    private static String createPrefix()
    {
      return DateTime.Now + "[{0}] {1}";
    }

    private static void writeToFile(String log)
    {
      try
      {
        lock (lockObject)
        {
          StreamWriter sw = File.AppendText(logFilename);
          sw.WriteLine(log);
          sw.Close();
        }
      }
      catch
      {
        Error("Failed to write out to log");
      }
    }
  }
}