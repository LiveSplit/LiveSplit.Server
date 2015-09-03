Use the LiveSplit server component to initiate a server. You can configure its port in its settings. The default port is 16834. You can start up the server by right clicking LiveSplit and choosing Control > Start Server.  Also LiveSplit doesn't close properly with the server being started, so it needs to be killed with the Task Manager.

Once you connect to the server with your connection, you can use a lot of commands. Every command is case sensitive and ends with a carriage return and a line feed (\r\n). A command can have parameters. You can provide parameters by using a space after the command and sending the parameters afterwards (<command><space><parameters><\r\n>).

A command can respond with a message. The message ends with a carriage return and a line feed, just like a command.

Here's the list of commands:

starttimer
startorsplit
split
unsplit
skipsplit
pause
resume
reset

initgametime
setgametime TIME
setloadingtimes TIME
pausegametime
unpausegametime

setcomparison COMPARISON

The following commands respond with a time:

getdelta
getdelta COMPARISON
getlastsplittime
getcomparisonsplittime
getcurrenttime
getfinaltime
getfinaltime COMPARISON
getpredictedtime COMPARISON
getbestpossibletime

Other commands:

getsplitindex
getcurrentsplitname
getprevioussplitname

When using game time it's important that you call "initgametime" once. Once "initgametime" is used, an additional comparison will appear and you can switch to it via the context menu (Compare Against > Game Time). This special comparison will show everything based on the game time (every component now shows game time based information).