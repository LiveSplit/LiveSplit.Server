using System;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class Settings : UserControl
    {
        public ushort Port { get; set; }

        public String PortString
        {
            get { return Port.ToString(); }
            set { Port = UInt16.Parse(value); }
        }

        public Settings()
        {
            InitializeComponent();
            Port = 16834;

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
