using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LiveSplit.UI.Components
{
    public class MessageEventArgs : EventArgs
    {
        public TcpConnection Connection { get; }
        public string Message { get; }

        public MessageEventArgs(TcpConnection connection, string message)
        {
            Connection = connection;
            Message = message;
        }
    }

    public class ScriptEventArgs : EventArgs
    {
        public TcpConnection Connection { get; }
        public IScript Script { get; }

        public ScriptEventArgs(TcpConnection connection, IScript script)
        {
            Connection = connection;
            Script = script;
        }
    }

    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
    public delegate void ScriptEventHandler(object sender, ScriptEventArgs e);

    public class TcpConnection : IConnection
    {
        protected Stream Stream { get; private set; }
        protected StreamReader Reader { get; private set; }

        private bool isWebSocket;

        protected Action<string> wsSend;
        protected Thread ReaderThread { get; private set; }

        public event MessageEventHandler MessageReceived;
        public event ScriptEventHandler ScriptReceived;
        public event EventHandler Disconnected;

        public TcpConnection(Stream stream)
        {
            Stream = stream;
            Reader = new StreamReader(Stream);

            isWebSocket = false;

            ReaderThread = new Thread(new ThreadStart(ReadCommands));
            ReaderThread.Start();
        }

        public void ReadCommands()
        {
            while (true)
            {
                string command = null;
                try
                {
                    command = Reader.ReadLine();
                }
                catch { }
                if (command != null)
                {
                    if (command.StartsWith("startscript"))
                    {
                        var splits = command.Split(new[] { ' ' }, 2);
                        var language = "C#";
                        if (splits.Length > 1)
                            language = splits[1];
                        ReadScript(language);
                    }
                    else
                    {
                        MessageReceived?.Invoke(this, new MessageEventArgs(this, command));
                    }
                }
                else break;
            }

            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private void ReadScript(string language)
        {
            var builder = new StringBuilder();
            while (true)
            {
                var line = Reader.ReadLine();
                if (line == "endscript")
                    break;
                builder.AppendLine(line);
            }
            try
            {
                var script = ScriptFactory.Create(language, builder.ToString());
                ScriptReceived?.Invoke(this, new ScriptEventArgs(this, script));
            }
            catch (Exception ex)
            {
                SendMessage("Compile Error: " + ex.Message);
            }
        }

        public void SendMessage(string message)
        {
            if (isWebSocket)
            {
                wsSend(message);
            }
            else
            {
                var buffer = Encoding.UTF8.GetBytes(message + Environment.NewLine);
                Stream.Write(buffer, 0, buffer.Length);
            }
        }

        public void Dispose()
        {
            Stream.Dispose();
            ReaderThread.Abort();
        }
    }
}
