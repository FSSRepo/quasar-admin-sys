using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
public class Client
{
    private String ip = "";
    private int port;
    private Socket socket;
    public static Client instance;
    public ByteStream output;
    ByteStream input;
    public bool request_close = false;
    bool send_request = false;
    bool connected = false;
    public event EventHandler<ClientData> OnReceived;
    public event EventHandler<ClientData> OnError;

    private Client(String ip, int port)
    {
        this.port = port;
        this.ip = ip;
        output = new ByteStream(4096);
        input = new ByteStream(4096);
    }

    public static void init()
    {
        if (instance != null)
        {
            instance = null;
        }
        //instance = new Client("192.168.43.104", 4000);
        instance = new Client("192.168.1.127", 4000);
        //instance = new Client("192.168.1.6", 4000);
        instance.start();
    }

    private void start()
    {
        new Thread(receiveThread).Start();
    }

    private void sendThread()
    {
        while (!request_close)
        {
            if (send_request)
            {
                socket.Send(output.getBuffer());
                send_request = false;
            }
            Thread.Sleep(10);
        }
    }

    public bool begin(int requestID)
    {
        if (connected)
        {
            output.rewind();
            output.writeInt(requestID);
            return true;
        }
        return false;
    }

    public void end()
    {
        send_request = true;
    }

    private void receiveThread()
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            connected = true;
            new Thread(sendThread).Start();
            requestMasterAccess();
            while (!request_close)
            {
                if (socket.Receive(input.getBuffer()) > 0)
                {
                    input.rewind();
                    ClientData args = new ClientData();
                    args.responseID = input.readInt();
                    args.client = this;
                    args.buffer = input;
                    OnReceived?.Invoke(this, args);
                }
                Thread.Sleep(50);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("Error: " + e.ToString());
            OnError?.Invoke(this,new ClientData() { error = "Error al conectarse con el servidor" });
        }
    }

    private void requestMasterAccess()
    {
        begin(0xFFC);
        output.writeInt(1);
        end();
    }
}
