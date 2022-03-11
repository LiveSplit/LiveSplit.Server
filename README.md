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

Commands are JSON strings that end with a carriage return and a line feed (\r\n).
```json
{ "command": "starttimer" }
```
Additional data can be passed to commands that accept them (such as `getdelta`) in the form of a `data` object.
```json
{
    "command": "getdelta",
    "data": {
        "comparison": "Best Segments"
    }
}
```
Remember that the JSON object **must** be serialized to a string before it is sent to LiveSplit.Server.

All commands respond with a message. The message is a JSON string and ends with a carriage return and a line feed, just like a command. The message will, at minimum, contain two properties: `"command"` and `"status"`. The command property simply reiterates what command was attempted. The `"status"` property will have a value of either `"success"` or `"error"` to indicate the success or failure of the operation, respectively.
Example response for a request of `{ "command": "starttimer" }`:
```json
{
    "command": "starttimer",
    "status": "success"
}
```
If the requested command expects some kind of information, it will be included in a `data` object with a property name similar to the command name.
```json
{
    "command": "getbestpossibletime",
    "status": "success",
    "data": {
        "bestPossibleTime": "45:02.85"
    }
}
```

The following commands perform actions and do not return any additional data:

- starttimer
- startorsplit
- split
- unsplit
- skipsplit
- pause
- resume
- reset
- initgametime
- pausegametime
- unpausegametime
- setgametime (expects `time` property)
- setloadingtimes (expects `time` property)
- setcomparison (expects `comparison` property)
- setsplitname (expects `index` and `name` properties)
- setcurrentsplitname (expects `name` property)
- switchto (expects `timingMethod` property with value of either "gametime" or "realtime")
- alwayspausegametime

The following commands respond with a time:

- getdelta (optional `comparison` property) - time is prefixed with "+" or "-"
- getprevioussplittime / getlastsplittime
- getcurrentsplittime / getcomparisonsplittime (optional `comparison` property)
- getcurrenttime
- getfinaltime / getfinalsplittime (optional `comparison` property)
- getpredictedtime (optional `comparison` property)
- getbestpossibletime

The following commands respond with other types of information:

- getsplitindex
- getcurrentsplitname
- getprevioussplitname / getlastsplitname
- gettimerphase / getcurrenttimerphase

Commands are defined at `ProcessMessage` in "ServerComponent.cs".

### Game Time
When using Game Time, it's important that you call "initgametime" once. Once "initgametime" is used, an additional comparison will appear and you can switch to it via the context menu (Compare Against > Game Time). This special comparison will show everything based on the Game Time (every component now shows Game Time based information). If you do not initialize game time, all commands that respond with a time will use Real Time for the timing method, even if you specify Game Time.

## Matching responses to commands
Message responses are not guaranteed to be sent in the same order incoming commands are received. This means that if your application is waiting for a response to its request, it could receive the response for a *different* request instead, if the two requests were made close enough to each other. In order to guarantee a response corresponds to a given request, clients should generate a [nonce](https://en.wikipedia.org/wiki/Cryptographic_nonce) and send it with their command request. LiveSplit.Server will include the nonce in the response so that your code may check that it matches before performing any additional actions. This also has the added benefit of preventing another application from sending messages over the same port that could trigger actions in your application.

Example request with nonce:
```json
{
    "command": "getdelta",
    "nonce": "ac49057d-be60-44e5-a05a-882b9eb31e81",
}
```
Response:
```json
{
    "command": "getdelta",
    "status": "success",
    "nonce": "ac49057d-be60-44e5-a05a-882b9eb31e81",
    "data": {
        "delta": "-1:30.12"
    }
}
```
Note that while it is common for nonces to be of a standardized format, such as UUID in the example above, there are no requirements around their structure or uniqueness; any string will be accepted by LiveSplit.Server. It is up to whatever client you use to guarantee their uniqueness. The nonce field is completely optional, though strongly recommended in any case that sends commands even somewhat frequently.

## Example Clients

### Python

```python
import socket

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect(("localhost", 16834))
s.send(b"{\"command\":\"starttimer\"}\r\n")
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
        writer.write("{\"command\":\"starttimer\"}\r\n");
        writer.flush();
        socket.close();
    }
}
```
### Node.js

Node.js client implementation available here: https://github.com/satanch/node-livesplit-client
