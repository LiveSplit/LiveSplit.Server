# LiveSplit Server

LiveSplit Server is a LiveSplit component that allows for other programs and other computers to control LiveSplit. Other programs may depend on features in the latest Development Build.

## Setup ##

Add the component to the Layout (Control -> LiveSplit Server). In Layout Settings, you can change the Server Port and view your local IP Address.

* **Port:** is the door on your computer that data enters through. Default is **16834**. This should be fine for most people, but depending on network configurations, some ports may be blocked. See also https://en.wikipedia.org/wiki/Port_%28computer_networking%29

* **Local IP:** is the address of your computer on your network. It is needed for other computers or phones on your network to talk to yours. Programs on your computer should be able to use *"localhost"*. Note that this is **NOT** your public IP with most network configurations. In most cases, it means nothing if it is seen on stream. **DO NOT** search "what is my IP** on stream as that will show you your public IP. The local IP should be the same as displayed from running `ipconfig` from Command Prompt labeled as "IPv4 Address".

You must start the Server before programs can talk to it (Right click on LiveSplit -> Control -> Start Server). You must manually start it each time you launch LiveSplit.

To make a public server, learn to set up a web server. It is probably wiser and easier to use an IRC bot or something else though.

## Known Uses ##

* **Android LiveSplit Remote:** https://github.com/Ekelbatzen/LiveSplit.Remote.Android
* **TODO**

## Commands ##

Commands are case sensitive and end with a carriage return and a line feed (\r\n). You can provide parameters by using a space after the command and sending the parameters afterwards (`<command><space><parameters><\r\n>`).

A command can respond with a message. The message ends with a carriage return and a line feed, just like a command.

Here's the list of commands:

* starttimer
* startorsplit
* split
* unsplit
* skipsplit
* pause
* resume
* reset

* initgametime
* setgametime TIME
* setloadingtimes TIME
* pausegametime
* unpausegametime

* setcomparison COMPARISON

The following commands respond with a time:

* getdelta
* getdelta COMPARISON
* getlastsplittime
* getcomparisonsplittime
* getcurrenttime
* getfinaltime
* getfinaltime COMPARISON
* getpredictedtime COMPARISON
* getbestpossibletime

Other commands:

* getsplitindex
* getcurrentsplitname
* getprevioussplitname

When using Game Time, it's important that you call "initgametime" once. Once "initgametime" is used, an additional comparison will appear and you can switch to it via the context menu (Compare Against > Game Time). This special comparison will show everything based on the Game Time (every component now shows Game Time based information).

## Example Clients ##

**Python 2**

```python
import socket
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect(("localhost", 16834))
s.send("starttimer\r\n")
```
**Java 7+**

```java
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.net.Socket;

public class MainTest {

	public static void main(String[] args){
    String hostName = "localhost";
		int portNumber = 16834;

		String str = "starttimer\r\n";

		try (Socket socket = new Socket(hostName, portNumber);
				OutputStreamWriter osw = new OutputStreamWriter(socket.getOutputStream(), "UTF-8")) {
			send(str, osw);			

		} catch (Exception e) {
			// TODO: handle exception
			System.err.println(e.getMessage());
		}
    }
	
    static void send(String str, OutputStreamWriter o) throws IOException {
		o.write(str, 0, str.length());
		o.flush();
	}

}
```
