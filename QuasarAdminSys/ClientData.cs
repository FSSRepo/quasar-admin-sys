using System;

public class ClientData : EventArgs
{
    public Client client;
    public int responseID;
    public ByteStream buffer;
    public string error;
}
