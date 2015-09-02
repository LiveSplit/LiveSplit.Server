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
using System.Threading.Tasks;

namespace LiveSplit.UI.Components
{
    public class Component : IComponent
    {
        public Settings Settings { get; set; }
        public TcpListener Server { get; set; }

        public List<Connection> Connections { get; set; }

        protected LiveSplitState State { get; set; }
        protected TimerModel Model { get; set; }
        protected ITimeFormatter DeltaFormatter { get; set; }
        protected ITimeFormatter SplitTimeFormatter { get; set; }
        protected NamedPipeServerStream WaitingServerPipe { get; set; }

        protected bool AlwaysPauseGameTime { get; set; }

        public float PaddingTop { get { return 0; } }
        public float PaddingBottom { get { return 0; } }
        public float PaddingLeft { get { return 0; } }
        public float PaddingRight { get { return 0; } }

        public string ComponentName
        {
            get { return "LiveSplit Server (" + Settings.Port + ")"; }
        }

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }

        public Component(LiveSplitState state)
        {
            Settings = new Settings();
            Model = new TimerModel();
            Connections = new List<Connection>();

            DeltaFormatter = new PreciseDeltaFormatter(TimeAccuracy.Hundredths);
            SplitTimeFormatter = new RegularTimeFormatter(TimeAccuracy.Hundredths);

            ContextMenuControls = new Dictionary<String, Action>();
            ContextMenuControls.Add("Start Server", Start);

            State = state;
            Model.CurrentState = State;
            State.OnStart += State_OnStart;
        }

        public void Start()
        {
            CloseAllConnections();
            Server = new TcpListener(IPAddress.Any, (int)Settings.Port);
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

                Connect(client.GetStream());

                Server.BeginAcceptTcpClient(AcceptTcpClient, null);
            }
            catch { }
        }

        public void AcceptPipeClient(IAsyncResult result)
        {
            try
            {
                WaitingServerPipe.EndWaitForConnection(result);

                Connect(WaitingServerPipe);

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
            Connections.Add(connection);
        }

        TimeSpan? parseTime(String timeString)
        {
            if (timeString == "-")
                return null;

            return TimeSpanParser.Parse(timeString);
        }

        void connection_ScriptReceived(object sender, ScriptEventArgs e)
        {
            try
            {
                e.Script["state"] = State;
                e.Script["model"] = Model;
                e.Script["sendMessage"] = new Action<String>(x => e.Connection.SendMessage(x));
                var result = e.Script.Run();
                if (result != null)
                    e.Connection.SendMessage(result.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                e.Connection.SendMessage(ex.Message);
            }
        }

        void connection_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                var message = e.Message;
                if (message == "startorsplit")
                {
                    if (State.CurrentPhase == TimerPhase.Running)
                    {
                        Model.Split();
                    }
                    else
                    {
                        Model.Start();
                    }

                }
                else if (message == "split")
                {
                    Model.Split();
                }
                else if (message == "unsplit")
                {
                    Model.UndoSplit();
                }
                else if (message == "skipsplit")
                {
                    Model.SkipSplit();
                }
                else if (message == "pause" && State.CurrentPhase != TimerPhase.Paused)
                {
                    Model.Pause();
                }
                else if (message == "resume" && State.CurrentPhase == TimerPhase.Paused)
                {
                    Model.Pause();
                }
                else if (message == "reset")
                {
                    Model.Reset();
                }
                else if (message == "starttimer")
                {
                    Model.Start();
                }
                else if (message.StartsWith("setgametime "))
                {
                    var value = message.Split(' ')[1];
                    var time = parseTime(value);
                    State.SetGameTime(time);
                }
                else if (message.StartsWith("setloadingtimes "))
                {
                    var value = message.Split(' ')[1];
                    var time = parseTime(value);
                    State.LoadingTimes = time ?? TimeSpan.Zero;
                }
                else if (message == "pausegametime")
                {
                    State.IsGameTimePaused = true;
                }
                else if (message == "unpausegametime")
                {
                    AlwaysPauseGameTime = false;
                    State.IsGameTimePaused = false;
                }
                else if (message == "alwayspausegametime")
                {
                    AlwaysPauseGameTime = true;
                    State.IsGameTimePaused = true;
                }
                else if (message == "getdelta" || message.StartsWith("getdelta "))
                {
                    var comparison = State.CurrentComparison;
                    if (message.Contains(" "))
                        comparison = message.Split(new char[] { ' ' }, 2)[1];
                    var delta = LiveSplitStateHelper.GetLastDelta(State, State.CurrentSplitIndex, comparison, State.CurrentTimingMethod);
                    var response = DeltaFormatter.Format(delta);
                    e.Connection.SendMessage(response);
                }
                else if (message == "getsplitindex")
                {
                    var splitindex = State.CurrentSplitIndex;
                    var response = splitindex.ToString();
                    e.Connection.SendMessage(response);
                }
                else if (message == "getcurrentsplitname")
                {
                	var splitindex = State.CurrentSplitIndex;
                    var currentsplitname = State.CurrentSplit.Name;
                    var response = currentsplitname;
                    e.Connection.SendMessage(response);
                }
                else if (message == "getprevioussplitname")
                {
                	var previoussplitindex = State.CurrentSplitIndex-1;
                	var previoussplitname = State.Run[previoussplitindex].Name;
                    var response = previoussplitname;
                    e.Connection.SendMessage(response);
                }                
                else if (message == "getlastsplittime" && State.CurrentSplitIndex > 0)
                {
                    var splittime = State.Run[State.CurrentSplitIndex - 1].SplitTime[State.CurrentTimingMethod];
                    var response = SplitTimeFormatter.Format(splittime);
                    e.Connection.SendMessage(response);
                }
                else if (message == "getcomparisonsplittime")
                {
                    var splittime = State.CurrentSplit.Comparisons[State.CurrentComparison][State.CurrentTimingMethod];
                    var response = SplitTimeFormatter.Format(splittime);
                    e.Connection.SendMessage(response);
                }
                else if (message == "getcurrenttime")
                {
                    var time = State.CurrentTime[State.CurrentTimingMethod];
                    var response = SplitTimeFormatter.Format(time);
                    e.Connection.SendMessage(response);
                }
                else if (message == "getfinaltime" || message.StartsWith("getfinaltime "))
                {
                    var comparison = State.CurrentComparison;
                    if (message.Contains(" "))
                    {
                        comparison = message.Split(new char[] { ' ' }, 2)[1];
                    }
                    var time = (State.CurrentPhase == TimerPhase.Ended)
                        ? State.CurrentTime[State.CurrentTimingMethod]
                        : State.Run.Last().Comparisons[comparison][State.CurrentTimingMethod];
                    var response = SplitTimeFormatter.Format(time);
                    e.Connection.SendMessage(response);
                }
                else if (message.StartsWith("getpredictedtime "))
                {
                    var comparison = message.Split(new char[] { ' ' }, 2)[1];
                    var prediction = PredictTime(State, comparison);
                    var response = SplitTimeFormatter.Format(prediction);
                    e.Connection.SendMessage(response);
                }
                else if (message == "getbestpossibletime")
                {
                    var comparison = LiveSplit.Model.Comparisons.BestSegmentsComparisonGenerator.ComparisonName;
                    var prediction = PredictTime(State, comparison);
                    var response = SplitTimeFormatter.Format(prediction);
                    e.Connection.SendMessage(response);
                }
                else if (message.StartsWith("setcomparison "))
                {
                    var comparison = message.Split(new char[] { ' ' }, 2)[1];
                    State.CurrentComparison = comparison;
                }
                else if (message == "switchto realtime")
                {
                    State.CurrentTimingMethod = TimingMethod.RealTime;
                }
                else if (message == "switchto gametime")
                {
                    State.CurrentTimingMethod = TimingMethod.GameTime;
                }
                else if (message.StartsWith("setsplitname "))
                {
                    int index = Convert.ToInt32(message.Split(new char[] { ' ' }, 3)[1]);
                    String title = message.Split(new char[] { ' ' }, 3)[2];
                    State.Run[index].Name = title;
                    State.Run.HasChanged = true;
                }
                else if (message.StartsWith("setcurrentsplitname "))
                {
                    String title = message.Split(new char[] { ' ' }, 2)[1];
                    State.Run[State.CurrentSplitIndex].Name = title;
                    State.Run.HasChanged = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void State_OnStart(object sender, EventArgs e)
        {
            if (AlwaysPauseGameTime)
                State.IsGameTimePaused = true;
        }

        private TimeSpan? PredictTime(LiveSplitState state, String comparison)
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

        public float VerticalHeight
        {
            get { return 0; }
        }

        public float MinimumWidth
        {
            get { return 0; }
        }

        public float HorizontalWidth
        {
            get { return 0; }
        }

        public float MinimumHeight
        {
            get { return 0; }
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return Settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public void Update(UI.IInvalidator invalidator, LiveSplitState state, float width, float height, UI.LayoutMode mode)
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
