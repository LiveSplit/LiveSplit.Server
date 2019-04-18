using System;

namespace LiveSplit
{
    interface IConnection : IDisposable
    {
        void SendMessage(string message);
    }
}
