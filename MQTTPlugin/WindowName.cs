using System;
using System.Collections.Generic;
using System.Xml;

namespace MQTTPlugin
{
  class WindowName
  {
    public class Window
    {
      public int id;
      public string name;
      public Window(int id, string name)
      {
        this.id = id;
        this.name = name;
      }
    }
    public List<Window> WindowNames = new List<Window>();

    public WindowName()
    {
      int count = 0;
      new SkinInfo();
      string tempstr = "";
      string[] files = System.IO.Directory.GetFiles(SkinInfo.mpPaths.configuredSkinPath);
      if (MQTTPlugin.DebugMode) Logger.Debug("Loading names of windows to memory.");
      foreach (string file in files)
      {
        try
        {
          if (file.StartsWith("common") == false && file.Contains("Dialog") == false && file.Contains("dialog") == false && file.Contains("wizard") == false && file.Contains("xml.backup") == false)
          {
            count++;
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            XmlNode node = doc.DocumentElement.SelectSingleNode("/window/id");
            try
            {
              tempstr = file.Remove(0, file.LastIndexOf(@"\") + 1).Replace(".xml", "");
              WindowNames.Add(new Window(Convert.ToInt32(node.InnerText), tempstr));
            }
            catch { }
          }
        }
        catch { }
      }
      if (MQTTPlugin.DebugMode) Logger.Debug("Loaded " + count + " window names in memory.");
    }

    public string GetName(int WinID)
    {
      string tempstr = "";
      List<Window> wnd = WindowNames.FindAll(delegate (Window w) { return w.id == WinID; });
      wnd.ForEach(delegate (Window w) { tempstr = w.name; });
      return tempstr;
    }

  }
}
