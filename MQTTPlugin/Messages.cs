using uPLibrary.Networking.M2Mqtt.Messages;

namespace MQTTPlugin
{

  // *** MQTT Message
  internal class MQTTMessage
  {
    public ushort Id { get; set; }
    public string Topic { get; set; }
    public string Message { get; set; }
    public byte QoS { get; set; }
    public bool Retain { get; set; }

  /// <summary>
  /// Initializes a new instance of the MQTTMessage class.
  /// </summary>
  public MQTTMessage()
    {
      Id = 0;
      Topic = string.Empty;
      Message = string.Empty;
      QoS = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE;
      Retain = true;
    }

    /// <summary>
    /// Initializes a new instance of the MQTTMessage class.
    /// </summary>
    /// <param name="id">Identifier</param>
    public MQTTMessage(ushort id)
      : this()
    {
      Id = id;
    }

    public bool IsEmpty
    {
      get { return string.IsNullOrEmpty(Topic); }
    }
  }

}