using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LiveSplit.UI.Components
{
    public class MessageEventArgs : EventArgs
    {
        public Connection Connection { get; protected set; }
        public String Message { get; protected set; }

        public MessageEventArgs(Connection connection, String message)
        {
            Connection = connection;
            Message = message;
        }
    }

    public class ScriptEventArgs : EventArgs
    {
        public Connection Connection { get; protected set; }
        public IScript Script { get; protected set; }

        public ScriptEventArgs(Connection connection, IScript script)
        {
            Connection = connection;
            Script = script;
        }
    }

    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
    public delegate void ScriptEventHandler(object sender, ScriptEventArgs e);

    public class Connection : IDisposable
    {
        protected Stream Stream { get; private set; }
        protected StreamReader Reader { get; private set; }
        protected Thread ReaderThread { get; private set; }

        public event MessageEventHandler MessageReceived;
        public event ScriptEventHandler ScriptReceived;

        public Connection(Stream stream)
        {
            Stream = stream;
            Reader = new StreamReader(Stream);

            ReaderThread = new Thread(new ThreadStart(ReadCommands));
            ReaderThread.Start();
        }

        public void ReadCommands()
        {
            while (true)
            {
                var command = Reader.ReadLine();
                if (command != null)
                {
                    if (command.StartsWith("startscript"))
                    {
                        var splits = command.Split(new char[] { ' ' }, 2);
                        var language = "C#";
                        if (splits.Length > 1)
                            language = splits[1];
                        ReadScript(language);
                    }
                    else
                    {
                        if (MessageReceived != null)
                            MessageReceived(this, new MessageEventArgs(this, command));
                    }
                }
                else break;
            }
        }

        private void ReadScript(String language)
        {
            var line = "";
            var builder = new StringBuilder();
            while (true)
            {
                line = Reader.ReadLine();
                if (line == "endscript")
                    break;
                builder.AppendLine(line);
            }

            if (ScriptReceived != null)
            {
                try
                {
                    var script = ScriptFactory.Create(language, builder.ToString());
                    ScriptReceived(this, new ScriptEventArgs(this, script));
                }
                catch (Exception ex)
                {
                    SendMessage("Compile Error: " + ex.Message);
                }
            }
        }

        public void SendMessage(String message)
        {
            var buffer = Encoding.UTF8.GetBytes(message + "\r\n");
            Stream.Write(buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            Stream.Dispose();
            ReaderThread.Abort();
        }
    }
}
