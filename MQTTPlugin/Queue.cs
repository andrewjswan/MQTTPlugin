using System.IO;

using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;

namespace MQTTPlugin
{
  public class QueueRec
  {
    public string header;
    public string line1;
    public string line2;
    public int timeout;
    public string image;
  }

  public class QueueHandler
  {
    public delegate void DisplayedMessage(string Message);
    public static event DisplayedMessage OnMessageDisplay;
    public static event DisplayedMessage OnMessageClose;

    public void ShowQueue()
    {
      if (MQTTPlugin.DebugMode) Logger.Debug("Mark message dialog busy");

      MQTTPlugin.DialogBusy = true;
      var QI = new QueueRec();
      while (MQTTPlugin.Queue.Count > 0)
      {
        if (MQTTPlugin.DebugMode) Logger.Debug("Number of messages in queue: " + MQTTPlugin.Queue.Count);

        QI = MQTTPlugin.Queue[0];
        var pDlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);

        if (MQTTPlugin.DebugMode) Logger.Debug("Message Header: " + QI.header);
        pDlgNotify.SetHeading(QI.header);

        if (MQTTPlugin.DebugMode) Logger.Debug("Message Line 1: " + QI.line1);
        if (MQTTPlugin.DebugMode) Logger.Debug("Message Line 2: " + QI.line2);
        pDlgNotify.SetText(QI.line1 + "\n" + QI.line2);

        if (MQTTPlugin.DebugMode) Logger.Debug("Message given image: " + QI.image);
        if (string.IsNullOrEmpty(QI.image))
        {
          QI.image = SkinInfo.GetMPThumbsPath() + "MQTTPlugin\\MQTTPluginIcon.png";
        }
        else
        {
          if (!File.Exists(QI.image))
          {
            QI.image = SkinInfo.GetMPThumbsPath() + "MQTTPlugin\\MQTTPluginIcon.png";
          }
        }
        if (MQTTPlugin.DebugMode) Logger.Debug("Message processed image: " + QI.image);
        pDlgNotify.SetImage(QI.image);

        if (MQTTPlugin.DebugMode) Logger.Debug("Message timeout: " + QI.timeout.ToString());
        pDlgNotify.TimeOut = QI.timeout;

        if (MQTTPlugin.DebugMode) Logger.Debug("Showing Message Dialog");
        OnMessageDisplay(QI.header);

        pDlgNotify.DoModal(GUIWindowManager.ActiveWindow);
        OnMessageClose(QI.header);

        if (MQTTPlugin.DebugMode) Logger.Debug("Message dialog closed");
        Logger.Info("Message shown.");
        MQTTPlugin.Queue.Remove(QI);

        if (MQTTPlugin.DebugMode) Logger.Debug("Message removed");
      }
      MQTTPlugin.DialogBusy = false;
      if (MQTTPlugin.DebugMode) Logger.Debug("Mark message dialog free");
    }
  }
}