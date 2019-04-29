using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class Settings : UserControl
    {
        public ushort TcpPort { get; set; }

        public ushort WebSocketPort { get; set; }

        public string LocalIP { get; set; }

        public bool UseWebSockets { get; set; }

        public string WebSocketOrigins { get; set; }

        public string GetIP()
        {
            IPAddress[] ipv4Addresses = Array.FindAll(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            return ipv4Addresses[0].ToString();
        }

        public string TcpPortString
        {
            get { return TcpPort.ToString(); }
            set { TcpPort = ushort.Parse(value); }
        }

        public string WebsocketPortString
        {
            get { return WebSocketPort.ToString(); }
            set { WebSocketPort = ushort.Parse(value); }
        }

        public Settings()
        {
            InitializeComponent();
            TcpPort = 16834;
            WebSocketPort = 16835;
            WebSocketOrigins = "";
            LocalIP = GetIP();
            label3.Text = LocalIP;
            UseWebSockets = false;

            cbWebSockets.DataBindings.Add("Checked", this, "UseWebSockets", false, DataSourceUpdateMode.OnPropertyChanged);
            txtWebSocketPort.DataBindings.Add("Text", this, "WebSocketPortString", false, DataSourceUpdateMode.OnPropertyChanged);
            txtWebSocketOrigins.DataBindings.Add("Text", this, "WebSocketOrigins", false, DataSourceUpdateMode.OnPropertyChanged);
            txtTcpPort.DataBindings.Add("Text", this, "TcpPortString", false, DataSourceUpdateMode.OnPropertyChanged);
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
            SettingsHelper.CreateSetting(document, parent, "WebSocketPort", WebsocketPortString);
            SettingsHelper.CreateSetting(document, parent, "WebSocketOrigins", WebSocketOrigins);
            return SettingsHelper.CreateSetting(document, parent, "TcpPort", TcpPortString);
        }

        public void SetSettings(XmlNode settings)
        {
            UseWebSockets = SettingsHelper.ParseBool(settings["UseWebSockets"]);
            WebsocketPortString = SettingsHelper.ParseString(settings["WebSocketPort"]);
            WebSocketOrigins = SettingsHelper.ParseString(settings["WebSocketOrigins"]);
            TcpPortString = SettingsHelper.ParseString(settings["TcpPort"]);
        }

        private void TableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
