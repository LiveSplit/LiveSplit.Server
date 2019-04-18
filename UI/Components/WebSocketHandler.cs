using WebSocketSharp;
using WebSocketSharp.Server;

namespace LiveSplit.UI.Components
{
    class WebSocketHandler : WebSocketBehavior
    {
        private ServerComponent parent;
        private IConnection cxn;
        public WebSocketHandler(ServerComponent p)
        {
            parent = p;
        }
        protected override void OnOpen()
        {
            cxn = new WebSocketConnection(Send);
            parent.Connections.Add(cxn);
        }
        protected override void OnClose(CloseEventArgs e)
        {
            parent.Connections.Remove(cxn);
        }
        protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
        {
            if (!e.IsText)
            {
                return;
            }
            parent.ProcessMessage(e.Data, cxn);
        }
    }
}
