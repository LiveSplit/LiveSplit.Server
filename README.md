# LiveSplit Server

LiveSplit Server is a LiveSplit component that allows for other programs and other computers to control LiveSplit.

## Install

Consider using the latest development build of LiveSplit available at [http://livesplit.org/LiveSplitDevBuild.zip](http://livesplit.org/LiveSplitDevBuild.zip). This includes the server component and any new features since the last release that applications may depend on.

Alternatively:

- Download the Server component from [https://github.com/LiveSplit/LiveSplit.Server/releases](https://github.com/LiveSplit/LiveSplit.Server/releases)
- Locate your LiveSplit instillation directory
- Place the contents of the downloaded zip into the "LiveSplit\Components" directory

## Setup

Add the component to the Layout (Control -> LiveSplit Server). In Layout Settings, you can change the Server Port and view your local IP Address.

### Control

You **MUST** start the Server before programs can talk to it (Right click on LiveSplit -> Control -> Start Server). You **MUST** manually start it each time you launch LiveSplit.

### Layout Settings

#### Port

**Port** is the door (1 of thousands) on your computer that this program sends data through. Default is **16834**. This should be fine for most people, but depending on network configurations, some ports may be blocked. See also https://en.wikipedia.org/wiki/Port_%28computer_networking%29

#### Local IP

**Local IP** is the address of your computer on your network. It is needed for other computers or phones on your network to talk to yours. Programs on your computer should be able to use _"localhost"_.

Note that this is **NOT** your public IP with most network configurations. In most cases, it means nothing if it is seen on stream. **DO NOT** search _"what is my IP"_ on stream as that will show you your public IP.

The local IP is the "IPv4 Address" of the first connected network adapter. This is normally what clients need, but software (such as virtual machines or VPNs) may add network adapters which can appear first. If in doubt, open Command Prompt and run `ipconfig`. The device you are looking for is probably either "Ethernet adapter Ethernet" or "Wireless LAN adapter Wi-Fi".

### Using Across the Internet

To make a public server, consider learning to set up a web server and use what you learn. It is probably wiser, safer, and easier to use an IRC bot or something else though. Look at "Known Uses" or ask around.

## Known Uses

- **Android LiveSplit Remote**: https://github.com/Ekelbatzen/LiveSplit.Remote.Android
- **SplitNotes**: https://github.com/joelnir/SplitNotes
- **Autosplitter Remote Client**: https://github.com/RavenX8/LiveSplit.Server.Client

Made something cool? Consider getting it added to this list.

## Commands

Commands are case sensitive and end with a carriage return and a line feed (\r\n). You can provide parameters by using a space after the command and sending the parameters afterwards (`<command><space><parameters><\r\n>`).

A command can respond with a message. The message ends with a carriage return and a line feed, just like a command.

Here's the list of commands:

- starttimer
- startorsplit
- split
- unsplit
- skipsplit
- pause
- resume
- reset
- initgametime
- setgametime TIME
- setloadingtimes TIME
- pausegametime
- unpausegametime
- setcomparison COMPARISON

The following commands respond with a time:

- getdelta
- getdelta COMPARISON
- getlastsplittime
- getcomparisonsplittime
- getcurrenttime
- getfinaltime
- getfinaltime COMPARISON
- getpredictedtime COMPARISON
- getbestpossibletime

Other commands:

- getsplitindex
- getcurrentsplitname
- getprevioussplitname
- getcurrenttimerphase

Commands are defined at `ProcessMessage` in "ServerComponent.cs".

When using Game Time, it's important that you call "initgametime" once. Once "initgametime" is used, an additional comparison will appear and you can switch to it via the context menu (Compare Against > Game Time). This special comparison will show everything based on the Game Time (every component now shows Game Time based information).

## Example Clients

### Python

```python
import socket

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect(("localhost", 16834))
s.send(b"starttimer\r\n")
```

### Java 7+

```java
import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;

public class MainTest {
    public static void main(String[] args) throws IOException {
        Socket socket = new Socket("localhost", 16834);
        PrintWriter writer = new PrintWriter(socket.getOutputStream());
        writer.write("starttimer\r\n");
        writer.flush();
        socket.close();
    }
}
```
### Node.js

Node.js client implementation available here: https://github.com/satanch/node-livesplit-client
