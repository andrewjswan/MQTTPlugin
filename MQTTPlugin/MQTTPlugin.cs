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
using MediaPortal.Player;
using MediaPortal.Profile;
using Microsoft.Win32;
using Newtonsoft.Json;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Action = MediaPortal.GUI.Library.Action;
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
    private bool IsReconected = false;

    private string CurrentMediaType = string.Empty;

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
      HostName = Dns.GetHostName().ToUpperInvariant().Replace(" ", "-");
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

      MQTTReconnect();

      if (mqttClient.IsConnected)
      {
        if (DebugMode) Logger.Debug("MQTT Broker connected: " + Host + ":" + Port);
        mqttClient.Subscribe(new string[] { BaseTopic + "Command/button", BaseTopic + "Command/message", BaseTopic + "Command/window", BaseTopic + "Command/play" }, 
                             new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
      }
      else
      {
        if (DebugMode) Logger.Debug("MQTT Broker connect to " + Host + ":" + Port + " failed.");
      }

      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackChanged += new g_Player.ChangedHandler(OnPlayBackChanged);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);

      GUIWindowManager.OnNewAction += new OnActionHandler(GUIWindowManager_OnNewAction);
      GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);
      GUIWindowManager.Receivers += new SendMessageHandler(GUIWindowManager_OnNewMessage);

      GUIPropertyManager.OnPropertyChanged += new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);

      QueueHandler.OnMessageDisplay += new QueueHandler.DisplayedMessage(QueueHandler_OnMessageDisplay);
      QueueHandler.OnMessageClose += new QueueHandler.DisplayedMessage(QueueHandler_OnMessageClose);

      _threadProcessStop = false;
      _queueSendEvent = new AutoResetEvent(false);

      _queueSend = new ConcurrentQueue<MQTTMessage>();
      _threadSendMessages = new Thread(ThreadSendMessages);
      _threadSendMessages.Priority = ThreadPriority.Lowest;
      _threadSendMessages.IsBackground = true;
      _threadSendMessages.Name = "MQTTPlugin SendMessage";
      _threadSendMessages.Start();

      SystemEvents.PowerModeChanged += OnPowerChange;

      SendEvent("status", "Online");
      SendEvent("version", FileVersionInfo.GetVersionInfo(Application.ExecutablePath).ProductVersion);
      // SendEvent("Version", FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion);

      SendEvent("holiday", "");
      GUIWindowManager_OnActivateWindow(GUIWindowManager.ActiveWindow);
      UpdateVolumeProperties();

      Logger.Info("Started");

      KeepAliveTimer = new Timer( new TimerCallback(KeepAliveEvent), null, 60000, 60000);
    }

    public void MQTTReconnect()
    {
      if (mqttClient == null)
      {
        return;
      }

      if (mqttClient.IsConnected)
      {
        return;
      }

      if (IsReconected)
      {
        return;
      }
      IsReconected = true;

      try
      {
#if DEBUG
        string clientId = "MP-DEBUG-" + HostName;
#else
        string clientId = "MP-" + HostName;
#endif
        if (string.IsNullOrEmpty(User))
        {
          mqttClient.Connect(clientId, null, null, true, MqttMsgConnect.QOS_LEVEL_AT_MOST_ONCE, true, BaseTopic + "status", "Offline", true, 60);
        }
        else
        {
          mqttClient.Connect(clientId, User, Password, true, MqttMsgConnect.QOS_LEVEL_AT_MOST_ONCE, true, BaseTopic + "status", "Offline", true, 60);
        }
      }
      catch { };
      IsReconected = false;
    }

    void IPlugin.Stop()
    {
      SystemEvents.PowerModeChanged -= OnPowerChange;

      KeepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);

      SendEvent("status", "Offline");
      Thread.Sleep(1000);

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

    private void OnPowerChange(object s, PowerModeChangedEventArgs e)
    {
      switch (e.Mode)
      {
        case PowerModes.Resume:
          KeepAliveTimer.Change(60000, 60000);
          break;
        case PowerModes.Suspend:
          KeepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
          SendEvent("status", "Offline");
          Thread.Sleep(1000);
          _queueSendEvent.Set();
          break;
      }
    }

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

            if (mqttClient == null || !mqttClient.IsConnected)
            {
              break;
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
        if (DebugMode) Logger.Debug("Message to received...");

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

        int id;
        if (Int32.TryParse(ReceivedMessage, out id))
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, id, 0, null);
          GUIWindowManager.SendThreadMessage(msg);
        }
      }
      else if (ReceivedTopic.Contains("/play"))
      {
        if (DebugMode) Logger.Debug("Play Command Received...");

        try
        {
          PlayItem rec = JsonConvert.DeserializeObject<PlayItem>(ReceivedMessage);

          PlayHandler play = new PlayHandler();
          play.Play(rec);
        }
        catch (WebException we)
        {
          Logger.Error("Client_MqttMsgPublishReceived: " + we);
        }
      }

    }

    #endregion

    #region On events

    public void OnPlayBackStarted(g_Player.MediaType type, string s)
    {
      if (DebugMode) Logger.Debug("Action is Play");
      SetLevelForPlayback(GetCurrentMediaType(type.ToString()), "Play");
      UpdateVolumeProperties();
    }


    public void OnPlayBackEnded(g_Player.MediaType type, string s)
    {
      if (DebugMode) Logger.Debug("Action is End");
      SetLevelForPlayback(CurrentMediaType, "End");
    }

    private void OnPlayBackChanged(g_Player.MediaType type, int stoptime, string filename)
    {
      // if (DebugMode) Logger.Debug("Action is Play Back Changed");
      // if (g_Player.Playing)
      // {
      //   SetLevelForPlayback(GetCurrentMediaType(type.ToString()), "Play");
      // }
    }

    public void OnPlayBackStopped(g_Player.MediaType type, int i, string s)
    {
      if (DebugMode) Logger.Debug("Action is Stop");
      SetLevelForPlayback(CurrentMediaType, "Stop");
    }

    void GUIWindowManager_OnNewAction(MediaPortal.GUI.Library.Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PAUSE)
      {
        if (DebugMode) Logger.Debug("Action is Pause, detecting if playing or paused");
        if (g_Player.Paused)
        {
          if (DebugMode) Logger.Debug("Action is Pause");
          SetLevelForPlayback(CurrentMediaType, "Pause");
        }
        else if (g_Player.Playing)
        {
          if (DebugMode) Logger.Debug("Action is Resume");
          SetLevelForPlayback(CurrentMediaType, "Play");
        }
      }
      else if (action.wID == Action.ActionType.ACTION_PLAY || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
      {
        if (DebugMode) Logger.Debug("Action is Play");
        SetLevelForPlayback(GetCurrentMediaType("Video"), "Play");
      }

      switch (action.wID)
      {
        // mute or unmute audio
        case Action.ActionType.ACTION_VOLUME_MUTE:
          UpdateVolumeProperties();
          break;

        // decrease volume 
        case Action.ActionType.ACTION_VOLUME_DOWN:
        // increase volume 
        case Action.ActionType.ACTION_VOLUME_UP:
          UpdateVolumeProperties();
          break;
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

      // Playing
      if (tag.Equals("#currentplaytime") || tag.Equals("#currentremaining") || tag.Equals("#duration"))
      {
        UpdatePlayingProperties();
      }

      // Volume
      if (tag.Equals("#volume.percent") || tag.Equals("#volume.mute"))
      {
        UpdateVolumeProperties();
      }
    }

    private void UpdatePlayingProperties()
    {
      if (g_Player.Playing || g_Player.Paused)
      {
        SendEvent("Player/playtime", ((int)g_Player.CurrentPosition).ToString());
        SendEvent("Player/remainingtime", ((int)(g_Player.Duration - g_Player.CurrentPosition)).ToString());
        SendEvent("Player/totaltime", ((int)g_Player.Duration).ToString());
      }
      else
      {
        SendEvent("Player/playtime", "");
        SendEvent("Player/remainingtime", "");
        SendEvent("Player/totaltime", "");
      }
    }

    private void UpdateVolumeProperties()
    {
      float fRange = (float)(VolumeHandler.Instance.Maximum - VolumeHandler.Instance.Minimum);
      float fPos = (float)(VolumeHandler.Instance.Volume - VolumeHandler.Instance.Minimum);
      float fPercent = (fPos / fRange) * 100.0f;
      SendEvent("volume", ((int)Math.Round(fPercent)).ToString());
      SendEvent("volume/mute", VolumeHandler.Instance.IsMuted ? "true" : "false");
    }

    private void GUIWindowManager_OnNewMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED)
      {
        if (g_Player.Playing)
        {
          if (DebugMode) Logger.Debug("Action is Position Changes");
          SetLevelForPlayback(CurrentMediaType, "Play");
        }

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
        SendSourceName(windowID);
      }
    }

    private void SendSourceName(int id)
    {
      SendEvent("input/list", "[\"Video\", \"Series\", \"Music\", \"Pictures\", \"Other\"]");

      switch (id)
      {
        case (int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_VIDEOS:
          SendEvent("input", "Video");
          break;
        case (int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_MUSIC:
          SendEvent("input", "Music");
          break;
        case (int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_PICTURES:
          SendEvent("input", "Pictures");
          break;
        case 9811: // TV-Series
          SendEvent("input", "Series");
          break;
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
      public string Length { get; set; }
    }

    private string GetCurrentMediaType(string type)
    {
      if (g_Player.IsMusic)
      {
        if (DebugMode) Logger.Debug("Media is Music");
        CurrentMediaType = "Music";
      }
      else if (g_Player.IsRadio)
      {
        if (DebugMode) Logger.Debug("Media is Radio");
        CurrentMediaType = "Radio";
      }
      else if (g_Player.IsTVRecording)
      {
        if (DebugMode) Logger.Debug("Media is Recording");
        CurrentMediaType = "Recording";
      }
      else if (g_Player.IsPicture)
      {
        if (DebugMode)
          Logger.Debug("Media is Picture");
        CurrentMediaType = "Picture";
      }
      else if (g_Player.IsTV)
      {
        if (DebugMode) Logger.Debug("Media is TV");
        CurrentMediaType = "TV";
      }
      else
      {
        if (DebugMode) Logger.Debug("Media is Video");
        CurrentMediaType = type;
      }
      return CurrentMediaType;
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
        Length = string.Empty,
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
          playback.Length = "Long";
        }
        else if ((mediaType == g_Player.MediaType.Video.ToString()) || mediaType == g_Player.MediaType.Recording.ToString())
        {
          if (g_Player.Duration < (setLevelForMediaDuration * 60))
          {
            if (DebugMode) Logger.Debug("Length is Short");
            playback.Length = "Short";
          }
          else
          {
            if (DebugMode) Logger.Debug("Length is Long");
            playback.Length = "Long";
          }
        }

        if (Level == "Play")
        {
          playback.FileName = g_Player.Player.CurrentFile;
          LatestMediaHandler.MQTTItem item = new LatestMediaHandler.MQTTItem();

          if (g_Player.IsMusic)
          {
            item = MyMusicHelper.CheckDB(playback.FileName);
            if (!string.IsNullOrEmpty(item.Filename))
            {
              playback.Genre = item.Genres;
              playback.Title = item.Title;
              // playback.Poster = item.Poster;
              // playback.Fanart = item.Fanart;
            }
          }

          if (g_Player.IsVideo)
          {
            if (!playback.FileName.StartsWith("http://localhost/")) // Online Video is not in DB so skip DB Search
            {
              try
              {
                if (DebugMode) Logger.Debug("Check to see if the video is a mounted disc.");

                string filename = playback.FileName;
                filename = MountHelper.CheckMount(ref filename);
                playback.FileName = filename;
              }
              catch
              {
                Logger.Warning("DaemonTools not Installed/Configured");
              }

              // TV Series
              try
              {
                item = TVSeriesHelper.CheckDB(playback.FileName);
                if (!string.IsNullOrEmpty(item.Filename))
                {
                  CurrentMediaType = "Series";
                  mediaType = CurrentMediaType;
                }
              }
              catch { }
              if (string.IsNullOrEmpty(item.Filename))
              {
                if (DebugMode) Logger.Debug("Video is not in TVSeries database.");
              }

              // MyVideo
              if (string.IsNullOrEmpty(item.Filename))
              {
                try
                {
                  item = MyVideoHelper.CheckDB(playback.FileName);
                }
                catch { }
              }
              if (string.IsNullOrEmpty(item.Filename))
              {
                if (DebugMode) Logger.Debug("Video is not in MyVideos database.");
              }

              // Moving Pictures
              if (string.IsNullOrEmpty(item.Filename))
              {
                try
                {
                  item = MovingPicturesHelper.CheckDB(playback.FileName);
                }
                catch { }
                if (string.IsNullOrEmpty(item.Filename))
                {
                  if (DebugMode) Logger.Debug("Video is not in Moving Pictures database.");
                }
              }

              if (!string.IsNullOrEmpty(item.Filename))
              {
                playback.Genre = item.Genres;
                playback.Title = item.Title;
                playback.Poster = item.Poster;
                playback.Fanart = item.Fanart;
              }
            }
            else
            {
              if (DebugMode) Logger.Debug("Media is OnlineVideo");
            }
          }
        }

        SendEvent("Player", new string[] { "type:" + mediaType, "state:" + Level.Replace("End", "Stop") });
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
        if (!string.IsNullOrEmpty(playback.Length))
        {
          Logger.Debug("Length: " + playback.Length);
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
      UpdatePlayingProperties();

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
        if (_threadProcessStop)
        {
          return;
        }

        if (mqttClient == null)
        {
          return;
        }

        if (!mqttClient.IsConnected)
        {
          MQTTReconnect();
        }

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
                int jsonindex = s.IndexOf("{");
                if (index > 0 && (jsonindex == -1 || jsonindex > index))
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
