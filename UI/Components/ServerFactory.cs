using LiveSplit.Model;
using System;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(ServerFactory))]

namespace LiveSplit.UI.Components
{
    public class ServerFactory : IComponentFactory
    {
        public string ComponentName => "LiveSplit Server";

        public string Description => "Allows a remote connection and control of LiveSplit by starting a small server within LiveSplit.";

        public ComponentCategory Category => ComponentCategory.Control;

        public IComponent Create(LiveSplitState state) => new ServerComponent(state);

        public string UpdateName => ComponentName;

        public string UpdateURL => "http://livesplit.org/update/";

        public Version Version => Version.Parse("1.7.2");

        public string XMLURL => "http://livesplit.org/update/Components/update.LiveSplit.Server.xml";
    }
}
