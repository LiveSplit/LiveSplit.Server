using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public partial class Settings : UserControl
    {
        public ushort Port { get; set; }

        public String PortString { get { return Port.ToString(); } set { Port = UInt16.Parse(value); } }

        public Settings()
        {
            InitializeComponent();
            Port = 16834;

            txtPort.DataBindings.Add("Text", this, "PortString", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");

            var portNode = document.CreateElement("Port");
            portNode.InnerText = PortString;
            settingsNode.AppendChild(portNode);

            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            PortString = settings["Port"].InnerText;
        }
    }
}
