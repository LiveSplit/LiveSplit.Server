# LiveSplit Server

LiveSplit Server is a LiveSplit component that allows for other programs and other computers to split. Other programs may depend on features in the laytest development build.

## Setup ##

Add the component to the layout (Control -> LiiveSplit Server). In layout settings you can change the server port and view your local ip address.

* **Port:** is the door on your computer data enters through. Default is **16834**. This should be fine for most people but depending on network configurations some ports may be blocked. See also https://en.wikipedia.org/wiki/Port_%28computer_networking%29

* **Local IP:** is the address of your computer on your network. It is needed for other computers or phones on your network to talk to yours. Programs on your computer should be able to use *"localhost"* Note this is **NOT** your public IP with most network configurations. In most cases it means nothing if it is seen on stream. **DO NOT** search "what is my ip** on stream as that will show you your public IP. The local ip should be the same as displayed from running `ipconfig` from Command Prompt labeled as "IPv4 Address".

You must start the server before programs can talk to it. Right click LiveSplit -> Control -> Start Server. Note "Start" just starts the timer like your configured hotkey. You must manually start it each time you launch LiveSplit.

To make a public server, learn to set up a web server. It is probably wiser and easier to use an irc bot or something else though.

## Known Uses ##

* **Android LiveSplit Remote:** In development.
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

When using game time it's important that you call "initgametime" once. Once "initgametime" is used, an additional comparison will appear and you can switch to it via the context menu (Compare Against > Game Time). This special comparison will show everything based on the game time (every component now shows game time based information).
