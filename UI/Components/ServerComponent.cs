using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public class ServerComponent : IComponent
    {
        public Settings Settings { get; set; }
        public TcpListener Server { get; set; }

        public List<Connection> Connections { get; set; }

        protected LiveSplitState State { get; set; }
        protected Form Form { get; set; }
        protected TimerModel Model { get; set; }
        protected ITimeFormatter DeltaFormatter { get; set; }
        protected ITimeFormatter SplitTimeFormatter { get; set; }
        protected NamedPipeServerStream WaitingServerPipe { get; set; }

        protected bool AlwaysPauseGameTime { get; set; }

        public float PaddingTop => 0;
        public float PaddingBottom => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;

        public string ComponentName => $"LiveSplit Server ({ Settings.Port })";

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }

        public ServerComponent(LiveSplitState state)
        {
            Settings = new Settings();
            Model = new TimerModel();
            Connections = new List<Connection>();

            DeltaFormatter = new PreciseDeltaFormatter(TimeAccuracy.Hundredths);
            SplitTimeFormatter = new RegularTimeFormatter(TimeAccuracy.Hundredths);

            ContextMenuControls = new Dictionary<string, Action>();
            ContextMenuControls.Add("Start Server", Start);

            State = state;
            Form = state.Form;

            Model.CurrentState = State;
            State.OnStart += State_OnStart;
        }

        public void Start()
        {
            CloseAllConnections();
            Server = new TcpListener(IPAddress.Any, Settings.Port);
            Server.Start();
            Server.BeginAcceptTcpClient(AcceptTcpClient, null);
            WaitingServerPipe = CreateServerPipe();
            WaitingServerPipe.BeginWaitForConnection(AcceptPipeClient, null);

            ContextMenuControls.Clear();
            ContextMenuControls.Add("Stop Server", Stop);
        }

        public void Stop()
        {
            CloseAllConnections();
            ContextMenuControls.Clear();
            ContextMenuControls.Add("Start Server", Start);
        }

        protected void CloseAllConnections()
        {
            if (WaitingServerPipe != null)
                WaitingServerPipe.Dispose();
            foreach (var connection in Connections)
            {
                connection.Dispose();
            }
            Connections.Clear();
            if (Server != null)
                Server.Stop();
        }

        public void AcceptTcpClient(IAsyncResult result)
        {
            try
            {
                var client = Server.EndAcceptTcpClient(result);

                Form.BeginInvoke(new Action(() => Connect(client.GetStream())));

                Server.BeginAcceptTcpClient(AcceptTcpClient, null);
            }
            catch { }
        }

        public void AcceptPipeClient(IAsyncResult result)
        {
            try
            {
                WaitingServerPipe.EndWaitForConnection(result);

                Form.BeginInvoke(new Action(() => Connect(WaitingServerPipe)));

                WaitingServerPipe = CreateServerPipe();
                WaitingServerPipe.BeginWaitForConnection(AcceptPipeClient, null);
            } catch { }
        }

        private NamedPipeServerStream CreateServerPipe()
        {
            var pipe = new NamedPipeServerStream("LiveSplit", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            return pipe;
        }

        private void Connect(Stream stream)
        {
            var connection = new Connection(stream);
            connection.MessageReceived += connection_MessageReceived;
            connection.ScriptReceived += connection_ScriptReceived;
            connection.Disconnected += connection_Disconnected;
            Connections.Add(connection);
        }

        TimeSpan? parseTime(string timeString)
        {
            if (timeString == "-")
                return null;

            return TimeSpanParser.Parse(timeString);
        }

        void connection_ScriptReceived(object sender, ScriptEventArgs e)
        {
            Form.BeginInvoke(new Action(() => ProcessScriptRequest(e.Script, e.Connection)));
        }

        private void ProcessScriptRequest(IScript script, Connection clientConnection)
        {
            try
            {
                script["state"] = State;
                script["model"] = Model;
                script["sendMessage"] = new Action<string>(x => clientConnection.SendMessage(x));
                var result = script.Run();
                if (result != null)
                    clientConnection.SendMessage(result.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                clientConnection.SendMessage(ex.Message);
            }
        }

        void connection_MessageReceived(object sender, MessageEventArgs e)
        {
            Form.BeginInvoke(new Action(() => ProcessMessage(e.Message, e.Connection)));
        }

        private void ProcessMessage(String message, Connection clientConnection)
        {
            try
            {
<<<<<<< HEAD
                var args = message.Split(new [] { ' ' }, 2);
=======
                var args = message.Split(new char[] {' '}, 2);
>>>>>>> e03c160a82d0919efc8a260ce8a48f39763f35b4
                var command = args[0];
                switch (command)
                {
                    case "startorsplit":
                    {
                        if (State.CurrentPhase == TimerPhase.Running)
                        {
                            Model.Split();
                        }
                        else
                        {
                            Model.Start();
                        }
                        break;
                    }
                    case "split":
                    {
                        Model.Split();
                        break;
                    }
                    case "unsplit":
                    {
                        Model.UndoSplit();
                        break;
                    }
                    case "skipsplit":
                    {
                        Model.SkipSplit();
                        break;
                    }
                    case "pause":
                    {
                        if (State.CurrentPhase != TimerPhase.Paused)
                        {
                            Model.Pause();
                        }
                        break;
                    }
                    case "resume":
                    {
                        if (State.CurrentPhase == TimerPhase.Paused)
                        {
                            Model.Pause();
                        }
                        break;
                    }
                    case "reset":
                    {
                        Model.Reset();
                        break;
                    }
                    case "starttimer":
                    {
                        Model.Start();
                        break;
                    }
                    case "setgametime":
                    {
                        var time = parseTime(args[1]);
                        State.SetGameTime(time);
                        break;
                    }
                    case "setloadingtimes":
                    {
                        var time = parseTime(args[1]);
                        State.LoadingTimes = time ?? TimeSpan.Zero;
                        break;
                    }
                    case "pausegametime":
                    {
                        State.IsGameTimePaused = true;
                        break;
                    }
                    case "unpausegametime":
                    {
                        AlwaysPauseGameTime = false;
                        State.IsGameTimePaused = false;
                        break;
                    }
                    case "alwayspausegametime":
                    {
                        AlwaysPauseGameTime = true;
                        State.IsGameTimePaused = true;
                        break;
                    }
                    case "getdelta":
                    {
                        var comparison = args.Length > 1 ? args[1] : State.CurrentComparison;
                        TimeSpan? delta = null;
                        if (State.CurrentPhase == TimerPhase.Running || State.CurrentPhase == TimerPhase.Paused)
                            delta = LiveSplitStateHelper.GetLastDelta(State, State.CurrentSplitIndex, comparison, State.CurrentTimingMethod);
                        else if (State.CurrentPhase == TimerPhase.Ended)
                            delta = State.Run.Last().SplitTime[State.CurrentTimingMethod] - State.Run.Last().Comparisons[comparison][State.CurrentTimingMethod];

                        var response = DeltaFormatter.Format(delta);
                        clientConnection.SendMessage(response);
                        break;
                    }
                    case "getsplitindex":
                    {
                        var splitindex = State.CurrentSplitIndex;
                        var response = splitindex.ToString();
                        clientConnection.SendMessage(response);
                        break;
                    }
                    case "getcurrentsplitname":
                    {
                        var currentsplitname = State.CurrentSplit.Name;
                        clientConnection.SendMessage(currentsplitname);
                        break;
                    }
                    case "getlastsplitname":
                    case "getprevioussplitname":
                    {
                        var previoussplitname = State.Run[State.CurrentSplitIndex].Name;
                        clientConnection.SendMessage(previoussplitname);
                        break;
                    }
                    case "getlastsplittime":
                    case "getprevioussplittime":
                    {
                        if (State.CurrentSplitIndex > 0)
                        {
                            var time = State.Run[State.CurrentSplitIndex - 1].SplitTime[State.CurrentTimingMethod];
                            var response = SplitTimeFormatter.Format(time);
                            clientConnection.SendMessage(response);
                        }
                        break;
                    }
                    case "getcomparisonsplittime":
                    {
                        var time = State.CurrentSplit.Comparisons[State.CurrentComparison][State.CurrentTimingMethod];
                        var response = SplitTimeFormatter.Format(time);
                        clientConnection.SendMessage(response);
                        break;
                    }
                    case "getcurrentrealtime":
                    {
                        var response = SplitTimeFormatter.Format(State.CurrentTime.RealTime);
                        clientConnection.SendMessage(response);
                        break;
                    }
                    case "getcurrentgametime":
                    {
                        var timingMethod = TimingMethod.GameTime;
                        if (!State.IsGameTimeInitialized)
                            timingMethod = TimingMethod.RealTime;
                        var response = SplitTimeFormatter.Format(State.CurrentTime[timingMethod]);
                        clientConnection.SendMessage(response);
                        break;
                    }
                    case "getcurrenttime":
                    {
                        var timingMethod = State.CurrentTimingMethod;
                        if (timingMethod == TimingMethod.GameTime && !State.IsGameTimeInitialized)
                            timingMethod = TimingMethod.RealTime;
                        var response = SplitTimeFormatter.Format(State.CurrentTime[timingMethod]);
                        clientConnection.SendMessage(response);
                        break;
                    }
                    case "getfinaltime":
                    case "getfinalsplittime":
                    {
                        var comparison = args.Length > 1 ? args[1] : State.CurrentComparison;
                        var time = (State.CurrentPhase == TimerPhase.Ended)
                            ? State.CurrentTime[State.CurrentTimingMethod]
                            : State.Run.Last().Comparisons[comparison][State.CurrentTimingMethod];
                        var response = SplitTimeFormatter.Format(time);
                        clientConnection.SendMessage(response);
                        break;
                    }
                    case "getbestpossibletime":
                    case "getpredictedtime":
                    {
                        string comparison;
                        if (command == "getbestpossibletime")
                            comparison = LiveSplit.Model.Comparisons.BestSegmentsComparisonGenerator.ComparisonName;
                        else
                            comparison = args.Length > 1 ? args[1] : State.CurrentComparison;
                        var prediction = PredictTime(State, comparison);
                        var response = SplitTimeFormatter.Format(prediction);
                        clientConnection.SendMessage(response);
                        break;
                    }
                    case "gettimerphase":
                    case "getcurrenttimerphase":
                    {
                        var response = State.CurrentPhase.ToString();
                        clientConnection.SendMessage(response);
                        break;
                    }
                    case "setcomparison":
                    {
                        State.CurrentComparison = args[1];
                        break;
                    }
                    case "switchto":
                    {
                        State.CurrentTimingMethod = args[1] == "gametime" ? TimingMethod.GameTime : TimingMethod.RealTime;
                        break;
                    }
                    case "setsplitname":
                    case "setcurrentsplitname":
                    {
                        var index = State.CurrentSplitIndex;
                        var title = args[1];

                        if (command == "setsplitname")
                        {
<<<<<<< HEAD
                            var options = args[1].Split(new [] { ' ' }, 2);
=======
                            var options = args[1].Split(new char[] {' '}, 2);
>>>>>>> e03c160a82d0919efc8a260ce8a48f39763f35b4
                            index = Convert.ToInt32(options[0]);
                            title = options[1];
                        }

                        State.Run[index].Name = title;
                        State.Run.HasChanged = true;
                        break;
                    }
                    default:
                    {
                        // perhaps an error should be returned for an unrecognized message?
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void connection_Disconnected(object sender, EventArgs e)
        {
            Form.BeginInvoke(new Action(() =>
            {
                var connection = (Connection)sender;
                Connections.Remove(connection);
                connection.Dispose();
            }));
        }

        private void State_OnStart(object sender, EventArgs e)
        {
            if (AlwaysPauseGameTime)
                State.IsGameTimePaused = true;
        }

        private TimeSpan? PredictTime(LiveSplitState state, string comparison)
        {
            if (state.CurrentPhase == TimerPhase.Running || state.CurrentPhase == TimerPhase.Paused)
            {
                TimeSpan? delta = LiveSplitStateHelper.GetLastDelta(state, state.CurrentSplitIndex, comparison, State.CurrentTimingMethod) ?? TimeSpan.Zero;
                var liveDelta = state.CurrentTime[State.CurrentTimingMethod] - state.CurrentSplit.Comparisons[comparison][State.CurrentTimingMethod];
                if (liveDelta > delta)
                    delta = liveDelta;
                return delta + state.Run.Last().Comparisons[comparison][State.CurrentTimingMethod];
            }
            else if (state.CurrentPhase == TimerPhase.Ended)
            {
                return state.Run.Last().SplitTime[State.CurrentTimingMethod];
            }
            else
            {
                return state.Run.Last().Comparisons[comparison][State.CurrentTimingMethod];
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
        }

        public float VerticalHeight => 0;

        public float MinimumWidth => 0;

        public float HorizontalWidth => 0;

        public float MinimumHeight => 0;

        public XmlNode GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return Settings;
        }

        public void SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
        }

        public void Dispose()
        {
            State.OnStart -= State_OnStart;
            CloseAllConnections();
        }

        public int GetSettingsHashCode()
        {
            return Settings.GetSettingsHashCode();
        }
    }
}
