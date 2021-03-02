using System;
using System.Windows.Forms;

using MediaPortal.Configuration;
using MediaPortal.InputDevices;

namespace MQTTPlugin
{
  public partial class FormSettings : Form
  {

    public FormSettings()
    {
      InitializeComponent();
    }


    private void displayDeviceSettings()
    {
      using (var xmlReader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MQTTPlugin.xml")))
      {
        WindowChange_checkBox.Checked = xmlReader.GetValueAsBool(MQTTPlugin.PLUGIN_NAME, "WindowChange", true);
        mediaDuration_textBox.Text = (xmlReader.GetValueAsInt(MQTTPlugin.PLUGIN_NAME, "SetLevelForMediaDuration", 10)).ToString();

        host_textBox.Text = xmlReader.GetValueAsString(MQTTPlugin.BROKER, "Host", string.Empty);
        port_textBox.Text = xmlReader.GetValueAsString(MQTTPlugin.BROKER, "Port", "1883");
        user_textBox.Text = xmlReader.GetValueAsString(MQTTPlugin.BROKER, "User", string.Empty);
        password_textBox.Text = DPAPI.DecryptString(xmlReader.GetValueAsString(MQTTPlugin.BROKER, "Password", string.Empty));

        debug_checkBox.Checked = xmlReader.GetValueAsBool(MQTTPlugin.PLUGIN_NAME, "DebugMode", false);
      }
    }

    private void FormSettings_Load(object sender, EventArgs e)
    {
      displayDeviceSettings();
    }

    public static void Main()
    {
      FormSettings frm = new FormSettings();
      if (frm.ShowDialog() == DialogResult.OK)
      {
      }
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      using (MediaPortal.Profile.Settings xmlWriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MQTTPlugin.xml")))
      {
        xmlWriter.SetValueAsBool(MQTTPlugin.PLUGIN_NAME, "WindowChange", WindowChange_checkBox.Checked);
        xmlWriter.SetValue(MQTTPlugin.PLUGIN_NAME, "SetLevelForMediaDuration", Convert.ToInt32(mediaDuration_textBox.Text));

        xmlWriter.SetValue(MQTTPlugin.BROKER, "Host", host_textBox.Text);
        xmlWriter.SetValue(MQTTPlugin.BROKER, "Port", port_textBox.Text);
        xmlWriter.SetValue(MQTTPlugin.BROKER, "User", user_textBox.Text);
        xmlWriter.SetValue(MQTTPlugin.BROKER, "Password", DPAPI.EncryptString(password_textBox.Text));

        xmlWriter.SetValueAsBool(MQTTPlugin.PLUGIN_NAME, "DebugMode", debug_checkBox.Checked);
      }
    }


    private void textBox1_TextChanged(object sender, EventArgs e)
    {

    }

    private void groupBox2_Enter(object sender, EventArgs e)
    {

    }

    private void button3_Click(object sender, EventArgs e)
    {
      InputMappingForm inputMappingForm = new InputMappingForm("MQTTPlugin");
      inputMappingForm.ShowDialog();
    }
  }
}