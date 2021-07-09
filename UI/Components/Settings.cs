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

        public string GetIP()
        {
            IPAddress[] ipv4Addresses = Array.FindAll(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            
            return String.Join(",", Array.ConvertAll(ipv4Addresses, x => x.ToString()));
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

            txtPort.DataBindings.Add("Text", this, "PortString", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            CreateSettingsNode(document, parent);
            return parent;
        }

        public int GetSettingsHashCode()
        {
            return CreateSettingsNode(null, null);
        }

        private int CreateSettingsNode(XmlDocument document, XmlElement parent)
        {
            return SettingsHelper.CreateSetting(document, parent, "Port", PortString);
        }

        public void SetSettings(XmlNode settings)
        {
            PortString = SettingsHelper.ParseString(settings["Port"]);
        }
    }
}
