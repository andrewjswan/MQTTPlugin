using MediaPortal.Util;

namespace MQTTPlugin
{
  public static class MountHelper
  {
    public static string CheckMount(ref string SearchFile)
    {
      string MountDrive = "";
      if (MQTTPlugin.DebugMode) Logger.Debug("Getting mount drive (if any)");
      MountDrive = DaemonTools.GetVirtualDrive();
      if (!MountDrive.Equals(""))
      {
        if (MQTTPlugin.DebugMode) Logger.Debug("Found mount drive: " + MountDrive);
        if (DaemonTools.MountedIsoFile != "") // if drive is mounted.
        {
          if (MQTTPlugin.DebugMode) Logger.Debug("An ISO is mounted.");
          if (SearchFile.Contains(MountDrive)) // if the mountdrive is the same as the drive in the Current playing file.
          {
            if (MQTTPlugin.DebugMode) Logger.Debug("Playing file from a mounted drive.");
            SearchFile = DaemonTools.MountedIsoFile;
          }
        }
      }
      if (MQTTPlugin.DebugMode) Logger.Debug("Returning filename: " + SearchFile);
      return SearchFile;
    }
  }
}
