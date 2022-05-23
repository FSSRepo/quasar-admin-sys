using System;

public class ServerData : EventArgs
{
    public int requestID;
    public Server server;
    public ClientHandler client;
    public ByteStream buffer;
}
