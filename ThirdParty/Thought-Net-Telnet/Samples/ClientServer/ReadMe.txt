
This samples shows a way of creating a telnet server and a telnet client.
The code does not have much error handling (or support for many Telnet
protocols) but will be expanded over time.  The server and clients are
both console applications written in VB.NET (the Telnet library itself
is written in C#).

You can run the server and client console applications at the same.
The default startup project is the server (e.g. press F5 to begin).
To start the client at the same time, right-click the TelnetClient
project, select Debug, then select Start New Instance.

A HyperTerminal shortcut is also available in this folder.

See http://www.thoughtproject.com/Libraries/BBS for a full-scale Telnet
bulletin board system that uses the library.  The source code for the
BBS will be released under LGPL sometime in summer 2007.
