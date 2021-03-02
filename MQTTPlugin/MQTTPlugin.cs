using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Video.Database;

using Newtonsoft.Json;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using Timer = System.Threading.Timer;

namespace MQTTPlugin
{

  [PluginIcons("MQTTPlugin.Resources.MQTTPlugin.png", "MQTTPlugin.Resources.MQTTPluginDisabled.png")]
  public class MQTTPlugin : IPlugin, ISetupForm, IPluginReceiver
  {
    public static bool SystemStandby;
    public static bool DialogBusy;
    public static List<QueueRec> Queue = new List<QueueRec>();
    public const string PLUGIN_NAME = "MQTTPlugin";
    public const string BROKER = "Broker";

    public string previousLevel = string.Empty;
    public string previousMediaType = string.Empty;

    public bool WindowChange;

    public int setLevelForMediaDuration;

    public string Host;
    public string Port;
    public string User;
    public string Password;

    public static bool DebugMode;

    public string HostName;
    public string BaseTopic;

    WindowName Windows = new WindowName();

    MqttClient mqttClient;

    private ConcurrentQueue<MQTTMessage> _queueSend;
    private AutoResetEvent _queueSendEvent;
    private Thread _threadSendMessages;
    private bool _threadProcessStop = false;
    private ushort PublishedId = 0;
    private Timer KeepAliveTimer;

    #region IPluginReceiver Members

    InputHandler inputHandler = new InputHandler(PLUGIN_NAME);

    bool IPluginReceiver.WndProc(ref System.Windows.Forms.Message m)
    {
      const int WM_APP = 0x8000;
      const int ID_MESSAGE_COMMAND = 0x18;
      const int WM_POWERBROADCAST = 0x218;
      const int ID_STANDBY = 4;
      const int ID_RESUME = 18;
      bool networkUp;

      switch (m.Msg)
      {
        case WM_POWERBROADCAST:
          if (DebugMode) Logger.Debug("Window Message received: WM_POWERBROADCAST WParam: " + m.WParam.ToString() + " LParam: " + m.LParam.ToString());
          if (m.WParam.ToInt32() == ID_STANDBY)
          {
            SendEvent("status", "Standby");
            SystemStandby = true;
          }
          if (m.WParam.ToInt32() == ID_RESUME)
          {
            networkUp = false;
            while (!networkUp)
            {
              networkUp = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            }
            SystemStandby = false;
            SendEvent("status", "Online");
          }

          return false;

        case WM_APP:
          if (m.WParam.ToInt32() == ID_MESSAGE_COMMAND)
          {
            inputHandler.MapAction(m.LParam.ToInt32() & 0xFFFF);
            return true;
          }
          return false;
        default:
          return false;
      }
    }
    #endregion

    public MQTTPlugin()
    {
      SystemStandby = false;
      PublishedId = 0;
      using (Settings xmlReader = new Settings(Config.GetFile(Config.Dir.Config, "MQTTPlugin.xml")))
      {
        WindowChange = xmlReader.GetValueAsBool(PLUGIN_NAME, "WindowChange", false);

        Host = xmlReader.GetValueAsString(BROKER, "Host", string.Empty);
        Port = xmlReader.GetValueAsString(BROKER, "Port", "1883");
        User = xmlReader.GetValueAsString(BROKER, "User", string.Empty);
        Password = DPAPI.DecryptString(xmlReader.GetValueAsString(BROKER, "Password", string.Empty));

        setLevelForMediaDuration = xmlReader.GetValueAsInt(PLUGIN_NAME, "SetLevelForMediaDuration", 10);

        DebugMode = xmlReader.GetValueAsBool(PLUGIN_NAME, "DebugMode", false);
      }
      HostName = Dns.GetHostName();
      BaseTopic = "Mediaportal/" + HostName + "/";
      Utils.Language = Utils.GetLang().ToLowerInvariant();
      DialogBusy = false;
    }

    #region ISetupForm Members

    public string Author()
    {
      return "ajs";
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public string Description()
    {
      return "Interact with MQTT";
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = strButtonImage = strButtonImageFocus = strPictureImage = null;
      return false;
    }

    public int GetWindowId()
    {
      return 555555;
    }

    public bool HasSetup()
    {
      return true;
    }

    public string PluginName()
    {
      return PLUGIN_NAME;
    }

    public void ShowPlugin()
    {
      var frm = new FormSettings();
      frm.ShowDialog();
    }

    #endregion

    #region IPlugin Members

    void IPlugin.Start()
    {
      Logger.Info("Starting " + PLUGIN_NAME + " version " + Assembly.GetExecutingAssembly().GetName().Version);

      var port = Convert.ToInt32(Port);
      mqttClient = new MqttClient(Host, port, false, null);
      mqttClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
      mqttClient.MqttMsgPublished += Client_MqttMsgPublished;

      string clientId = "MP-" + HostName.Replace(" ", "-");
      if (string.IsNullOrEmpty(User))
      {
        // mqttClient.Connect(clientId);
        mqttClient.Connect(clientId, null, null, false, MqttMsgConnect.QOS_LEVEL_AT_MOST_ONCE, true, BaseTopic + "status", "Offline", true, 60);
      }
      else
      {
        // mqttClient.Connect(clientId, User, Password);
        mqttClient.Connect(clientId, User, Password, false, MqttMsgConnect.QOS_LEVEL_AT_MOST_ONCE, true, BaseTopic + "status", "Offline", true, 60);
      }

      if (mqttClient.IsConnected)
      {
        if (DebugMode) Logger.Debug("MQTT Broker connected: " + Host + ":" + Port);
        mqttClient.Subscribe(new string[] { BaseTopic + "Command/button", BaseTopic + "Command/message", BaseTopic + "Command/window" }, 
                             new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
      }
      else
      {
        if (DebugMode) Logger.Debug("MQTT Broker connect to " + Host + ":" + Port + " failed.");
      }

      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnVideoStarted);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(OnVideoEnded);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnVideoStopped);

      GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
      GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);

      GUIPropertyManager.OnPropertyChanged += new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);

      QueueHandler.OnMessageDisplay += new QueueHandler.DisplayedMessage(QueueHandler_OnMessageDisplay);
      QueueHandler.OnMessageClose += new QueueHandler.DisplayedMessage(QueueHandler_OnMessageClose);

      _threadProcessStop = false;
      _queueSendEvent = new AutoResetEvent(false);

      _queueSend = new ConcurrentQueue<MQTTMessage>();
      _threadSendMessages = new Thread(ThreadSendMessages);
      _threadSendMessages.Priority = ThreadPriority.Lowest;
      _threadSendMessages.IsBackground = true;
      _threadSendMessages.Name = "GUIPictures GetPicturesInfo";
      _threadSendMessages.Start();

      SendEvent("status", "Online");
      SendEvent("version", FileVersionInfo.GetVersionInfo(Application.ExecutablePath).ProductVersion);
      // SendEvent("Version", FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion);

      SendEvent("holiday");
      GUIWindowManager_OnActivateWindow(GUIWindowManager.ActiveWindow);

      Logger.Info("Started");

      KeepAliveTimer = new Timer( new TimerCallback(KeepAliveEvent), null, 60000, 60000);
    }

    void IPlugin.Stop()
    {
      KeepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);

      SendEvent("status", "Offline");

      if (_threadSendMessages != null && _threadSendMessages.IsAlive)
      {
        _queueSendEvent.Set();
        _threadProcessStop = true;
        _queueSendEvent.Set();
      }

      // Wait for thread ended ...
      if (_threadSendMessages != null)
      {
        _threadSendMessages.Join();
        _threadSendMessages = null;
      }
      _queueSendEvent.Dispose();

      mqttClient.Disconnect();

      if (DebugMode) Logger.Debug("MQTT Broker disconnected.");
      Logger.Info("Stopping");
    }

    #endregion

    #region Timers

    private void KeepAliveEvent(object state)
    {
      SendEvent("status", "Online");
    }

    #endregion

    #region Thread Messages

    private void ThreadSendMessages()
    {
      if (DebugMode) Logger.Debug("ThreadSendMessages started...");
      try
      {
        while (!_threadProcessStop || _queueSend.Count > 0)
        {
          MQTTMessage message;
          while (_queueSend.TryDequeue(out message))
          {
            if (message.IsEmpty)
            {
              continue;
            }

            ushort id = mqttClient.Publish(message.Topic, Encoding.UTF8.GetBytes(message.Message), message.QoS, message.Retain);
            if (DebugMode) Logger.Debug("Message sended: " + id.ToString() + ": " + message.Topic + " - " + message.Message + " - " + message.QoS + " - " + message.Retain);

            int wait = 0;
            while (message.Retain && id != PublishedId && wait < 3000)
            {
              Thread.Sleep(1);
              wait++;
            }

            if (DebugMode) 
            { 
              if (!message.Retain || id == PublishedId) 
              { 
                Logger.Debug("Message " + id.ToString() + " sended."); 
              } 
              else 
              { 
                Logger.Debug("Message " + id.ToString() + " not sended."); 
              } 
            };

            PublishedId = 0;
          }
          _queueSendEvent.WaitOne();
        }
      }
      catch (Exception ex)
      {
        Logger.Error("ThreadSendMessages: " + ex.ToString());
      }
      if (DebugMode) Logger.Debug("ThreadSendMessages ended.");
    }

    #endregion

    #region MQTT Messages

    void Client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
    {
      if (DebugMode) Logger.Debug("Message with Id: " + e.MessageId.ToString() + " published.");
      PublishedId = e.MessageId;
    }

    void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
      string ReceivedTopic = e.Topic;
      string ReceivedMessage = Encoding.UTF8.GetString(e.Message);

      if (DebugMode) Logger.Debug("Message received: [" + ReceivedTopic + "]: " + ReceivedMessage);
      if (string.IsNullOrEmpty(ReceivedMessage))
      {
        return;
      }

      if (ReceivedTopic.Contains("/button"))
      {
        if (DebugMode) Logger.Debug("Remote Button Received: " + ReceivedMessage);
        inputHandler.MapAction(ReceivedMessage);
      }
      else if (ReceivedTopic.Contains("/message"))
      {
        try
        {
          QueueRec rec = JsonConvert.DeserializeObject<QueueRec>(ReceivedMessage);
          if (DebugMode) Logger.Debug("Add Message to Queue...");

          Queue.Add(rec);
          if (!MQTTPlugin.DialogBusy)
          {
            if (DebugMode) Logger.Debug("Dialog is not busy, fire ShowQueue Thread");
            var Q = new QueueHandler();
            var QThread = new Thread(Q.ShowQueue);
            QThread.Start();
          }
        }
        catch (WebException we)
        {
          Logger.Error("Client_MqttMsgPublishReceived: " + we);
        }
      }
      else if (ReceivedTopic.Contains("/window"))
      {
        if (DebugMode) Logger.Debug("Activate Window Received: " + ReceivedMessage);
      }
    }

    #endregion

    #region On events
    public void OnVideoStarted(g_Player.MediaType type, string s)
    {
      if (DebugMode) Logger.Debug("Action is Play");
      SetLevelForPlayback(type.ToString(), "Play");
    }

    public void OnVideoEnded(g_Player.MediaType type, string s)
    {
      if (DebugMode) Logger.Debug("Action is End");
      SetLevelForPlayback(type.ToString(), "End");
    }

    public void OnVideoStopped(g_Player.MediaType type, int i, string s)
    {
      if (DebugMode) Logger.Debug("Action is Stop");
      SetLevelForPlayback(GetCurrentMediaType(), "Stop");
    }

    void GUIWindowManager_OnNewAction(MediaPortal.GUI.Library.Action action)
    {
      if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_PAUSE)
      {
        if (DebugMode) Logger.Debug("Action is Pause, detecting if playing or paused");
        if (g_Player.Paused)
        {
          if (DebugMode) Logger.Debug("Action is Pause");
          SetLevelForPlayback(GetCurrentMediaType(), "Pause");
        }
        else if (g_Player.Playing)
        {
          if (DebugMode) Logger.Debug("Action is Resume");
          if (g_Player.Playing) SetLevelForPlayback(GetCurrentMediaType(), "Play");
        }
      }
      else if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_PLAY || action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MUSIC_PLAY)
      {
        if (DebugMode) Logger.Debug("Action is Play");
        SetLevelForPlayback(GetCurrentMediaType(), "Play");
      }
    }

    public void QueueHandler_OnMessageDisplay(string Message)
    {
      SendEvent("Message/Displaying", "message:" + Message);
      if (DebugMode) Logger.Debug("Displaying Message Trigger: " + Message);
    }

    public void QueueHandler_OnMessageClose(string Message)
    {
      SendEvent("Message/Closed", "message:" + Message);
      if (DebugMode) Logger.Debug("Closed Message Trigger: " + Message);
    }

    public void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
    {
      // Latests Media
      if (tag.Equals("#latestMediaHandler.scanned"))
      {
        List<string> items = LatestsMediaHandlerHelper.LatestsMedia();
        foreach (string item in items)
        {
          SendEvent("Latests", item);
        }
      }

      // Fanart Handler Holidays
      if (tag.Equals("#fanarthandler.holiday.current"))
      {
        SendEvent("holiday", tagValue);
      }
    }

    private class ActiveWindow
    {
      public int Id { get; set; }
      public string Name { get; set; }
    }

    public void GUIWindowManager_OnActivateWindow(int windowID)
    {
      if (WindowChange)
      {
        ActiveWindow window = new ActiveWindow();
        window.Id = windowID;
        window.Name = Windows.GetName(windowID);

        if (DebugMode) Logger.Debug("Window Activated: " + window.Id.ToString());
        if (DebugMode) Logger.Debug("Window name: " + window.Name);

        SendEvent("Window", "Activate:" + JsonConvert.SerializeObject(window, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new Utils.LowercaseContractResolver() }));
      }
    }

    #endregion

    #region Playback

    private class Playback
    {
      public string Title { get; set; }
      public string FileName { get; set; }
      public string Genre { get; set; }
      public string Poster { get; set; }
      public string Fanart { get; set; }
      public string LengthString { get; set; }
    }

    private string GetCurrentMediaType()
    {
      if (g_Player.IsMusic)
      {
        if (DebugMode) Logger.Debug("Media is Music");
        return "Music";
      }
      else if (g_Player.IsRadio)
      {
        if (DebugMode) Logger.Debug("Media is Radio");
        return "Radio";
      }
      else if (g_Player.IsTVRecording)
      {
        if (DebugMode) Logger.Debug("Media is Recording");
        return "Recording";
      }
      else if (g_Player.IsTV)
      {
        if (DebugMode) Logger.Debug("Media is TV");
        return "TV";
      }
      else
      {
        if (DebugMode) Logger.Debug("Media is Video");
        return "Video";
      }
    }

    private void SetLevelForPlayback(string mediaType, string Level)
    {
      if ((previousLevel == "Play" || previousLevel == "Pause") && (Level == "Play") && (previousMediaType != mediaType))
      {
        previousLevel = "Stop";
      }

      Playback playback = new Playback()
      {
        Title = string.Empty,
        LengthString = string.Empty,
        Genre = string.Empty,
        FileName = string.Empty,
        Poster = string.Empty,
        Fanart = string.Empty
      };

      if (!mediaType.Equals("Plugin"))
      {
        if (g_Player.IsDVD)
        {
          if (DebugMode) Logger.Debug("Length is Long (Media is DVD)");
          playback.LengthString = "Long";
        }
        else if ((mediaType == g_Player.MediaType.Video.ToString()) || mediaType == g_Player.MediaType.Recording.ToString())
        {
          if (g_Player.Duration < (setLevelForMediaDuration * 60))
          {
            if (DebugMode) Logger.Debug("Length is Short");
            playback.LengthString = "Short";
          }
          else
          {
            if (DebugMode) Logger.Debug("Length is Long");
            playback.LengthString = "Long";
          }
        }

        if (Level == "Play")
        {
          playback.FileName = g_Player.Player.CurrentFile;

          if (g_Player.IsMusic)
          {
            Song song = new Song();
            MusicDatabase musicDatabase = MusicDatabase.Instance;
            musicDatabase.GetSongByFileName(playback.FileName, ref song);
            if (song != null)
            {
              playback.Genre = song.Genre;
              playback.Title = song.Artist + " - " + song.Album + " - " + song.Title;
            }
          }

          if (g_Player.IsVideo)
          {
            if (!playback.FileName.StartsWith("http://localhost/")) // Online Video is not in DB so skip DB Search
            {
              LatestMediaHandler.MQTTItem item;
              try
              {
                if (DebugMode) Logger.Debug("Check to see if the video is a mounted disc.");

                string filename = playback.FileName;
                filename = MountHelper.CheckMount(ref filename);
                playback.FileName = filename;
              }
              catch
              {
                Logger.Warning("Daemontools not installed/configured");
              }

              item = MyVideoHelper.CheckDB(playback.FileName);
              if (!string.IsNullOrEmpty(item.Filename))
              {
                playback.Genre = item.Genres;
                playback.Title = item.Title;
                playback.Poster = item.Poster;
                playback.Fanart = item.Fanart;
              }
              else // Movie not in MyVideo's DB
              {
                if (DebugMode) Logger.Debug("Video is not in MyVideos database.");
                try
                {
                  item = TVSeriesHelper.CheckDB(playback.FileName);
                  if (!string.IsNullOrEmpty(item.Filename))
                  {
                    playback.Genre = item.Genres;
                    playback.Title = item.Title;
                    playback.Poster = item.Poster;
                    playback.Fanart = item.Fanart;
                  }
                }
                catch
                {
                  Logger.Warning("Error while searching TVSeries Database, probaly not installed");
                }

                if (string.IsNullOrEmpty(item.Filename))
                {
                  try
                  {
                    item = MovingPicturesHelper.CheckDB(playback.FileName);
                    if (!string.IsNullOrEmpty(item.Filename))
                    {
                      playback.Genre = item.Genres;
                      playback.Title = item.Title;
                      playback.Poster = item.Poster;
                      playback.Fanart = item.Fanart;
                    }
                  }
                  catch
                  {
                    Logger.Warning("Error while searching MovingPictures Database, probaly not installed");
                  }
                }
              }
            }
            else
            {
              if (DebugMode) Logger.Debug("Media is OnlineVideo");
            }
          }
        }

        SendEvent("Player", new string[] { "type:" + mediaType, "state:" + Level });
      }

      playback.Genre = !string.IsNullOrEmpty(playback.Genre) ? playback.Genre.Trim('|').Replace("|", " / ") : "";
      if (DebugMode)
      {
        Logger.Debug("ACTION " + Level + " Media: " + mediaType);
        if (!string.IsNullOrEmpty(playback.Title))
        {
          Logger.Debug("Title: " + playback.Title);
        }
        if (!string.IsNullOrEmpty(playback.FileName))
        {
          Logger.Debug("Filename: " + playback.FileName);
        }
        if (!string.IsNullOrEmpty(playback.Genre))
        {
          Logger.Debug("Genre: " + playback.Genre);
        }
        if (!string.IsNullOrEmpty(playback.LengthString))
        {
          Logger.Debug("Length: " + playback.LengthString);
        }
      }

      if (Level.Equals("Play"))
      {
        SendEvent("Player/" + mediaType, new string[] { "action:" + Level, 
                                                        "data:" + JsonConvert.SerializeObject(playback, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new Utils.LowercaseContractResolver() }) });
      }
      else
      {
        SendEvent("Player/" + mediaType, "action:" + Level);
      }

      previousLevel = Level;
      previousMediaType = mediaType;
    }

    #endregion

    #region Send Event to MQTT Broker

    private void SendEvent(string Event)
    {
      SendEvent(Event, new string[] { null });
    }

    private void SendEvent(string Event, string payload)
    {
      SendEvent(Event, new string[] { payload });
    }

    private void SendEvent(string Event, string[] payload)
    {
      if (!SystemStandby)
      {
        if (mqttClient.IsConnected)
        {
          Event = BaseTopic + Event;

          if (payload != null)
          {
            foreach (string s in payload)
            {
              if (!string.IsNullOrEmpty(s))
              {
                string message = s;
                string topic = Event;
                int index = s.IndexOf(":");
                if (index > 0)
                {
                  topic = topic + "/" + s.Substring(0, index);
                  message = s.Substring(index + 1);
                }

                if (DebugMode) Logger.Debug("Message to be sent: " + topic + " - " + message);
                MQTTMessage item = new MQTTMessage
                {
                  Topic = topic,
                  Message = message
                };
                _queueSend.Enqueue(item);
              }
              else
              {
                if (DebugMode) Logger.Debug("Message to be reset: " + Event);
                MQTTMessage item = new MQTTMessage
                {
                  Topic = Event,
                  Message = string.Empty,
                  QoS = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
                  Retain = false
                };
                _queueSend.Enqueue(item);
              }
            }
          }
          else
          {
            if (DebugMode) Logger.Debug("Message to be reset: " + Event);
            MQTTMessage item = new MQTTMessage
            {
              Topic = Event,
              Message = string.Empty,
              QoS = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
              Retain = false
            };
            _queueSend.Enqueue(item);
          }
        }
        else
        {
          if (DebugMode) Logger.Debug("No connection to MQTT Broker, skipping send.");
        }
      }
      else
      {
        if (DebugMode) Logger.Debug("System in Standy, skipping send.");
      }
      _queueSendEvent.Set();
    }

    #endregion

  }
}
