using System.IO;

namespace MQTTPlugin
{
  public class PlayItem
  {
    public string plugin;
    public string filename;
    public string artist;
    public string album;
  }

  public class PlayHandler
  {
    public void Play(PlayItem item)
    {
      if (item.plugin.ToLowerInvariant() == "myvideo")
      {
        if (File.Exists(item.filename))
        {
          if (MQTTPlugin.DebugMode) Logger.Debug("Start playing: " + item.filename + " - " + item.plugin);
          MyVideoHelper.PlayMovie(item.filename);
        }
        else
        {
          if (MQTTPlugin.DebugMode) Logger.Debug("File not found: " + item.filename + " - " + item.plugin);
        }
      }

      if (item.plugin.ToLowerInvariant() == "mymusic")
      {
        if (!string.IsNullOrEmpty(item.filename))
        {
          if (File.Exists(item.filename))
          {
            if (MQTTPlugin.DebugMode) Logger.Debug("Start playing: " + item.filename + " - " + item.plugin);
            MyMusicHelper.PlayMusic(item.filename);
          }
        }
        else
        {
          if (MQTTPlugin.DebugMode) Logger.Debug("File not found: " + item.filename + " - " + item.plugin);
        }
      }
      else if (!string.IsNullOrEmpty(item.artist) && !string.IsNullOrEmpty(item.album))
      {
        if (MQTTPlugin.DebugMode) Logger.Debug("Start playing: " + item.artist + " - " + item.album + " - " + item.plugin);
        MyMusicHelper.PlayMusicArtistAlbum(item.artist, item.album);
      }
      else if (!string.IsNullOrEmpty(item.album))
      {
        if (MQTTPlugin.DebugMode) Logger.Debug("Start playing: " + item.album + " - " + item.plugin);
        MyMusicHelper.PlayMusicAlbum(item.album);
      }
    }
  }
}