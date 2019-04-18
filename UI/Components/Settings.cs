using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class Settings : UserControl
    {
        public ushort Port { get; set; }

        public string LocalIP { get; set; }

        public bool UseWebSockets { get; set; }

        public string GetIP()
        {
            IPAddress[] ipv4Addresses = Array.FindAll(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            return ipv4Addresses[0].ToString();
        }

        public string PortString
        {
            get { return Port.ToString(); }
            set { Port = ushort.Parse(value); }
        }

        public Settings()
        {
            InitializeComponent();
            Port = 16834;
            LocalIP = GetIP();
            label3.Text = LocalIP;
            UseWebSockets = false;

            cbWebSockets.DataBindings.Add("Checked", this, "UseWebSockets", false, DataSourceUpdateMode.OnPropertyChanged);
            txtPort.DataBindings.Add("Text", this, "PortString", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNodes(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNodes(null, null);
        }

        private int CreateSettingsNodes(XmlDocument document, XmlElement parent)
        {
            SettingsHelper.CreateSetting(document, parent, "UseWebSockets", UseWebSockets);
            return SettingsHelper.CreateSetting(document, parent, "Port", PortString);
        }

        public void SetSettings(XmlNode settings)
        {
            UseWebSockets = SettingsHelper.ParseBool(settings["UseWebSockets"]);
            PortString = SettingsHelper.ParseString(settings["Port"]);
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
