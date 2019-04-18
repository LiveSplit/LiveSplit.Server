using System;

namespace LiveSplit.UI.Components
{
    class WebSocketConnection : IConnection
    {
        private readonly Action<string> Send;

        public WebSocketConnection(Action<string> send)
        {
            Send = send;
        }

        public void SendMessage(string message)
        {
            Send(message);
        }

        public void Dispose()
        {

        }
    }
}
