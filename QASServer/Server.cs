using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
public class Server
{
    Socket server;
    public String ip;
    public List<ClientHandler> clients = new List<ClientHandler>();
    public EventHandler<ServerData> OnConnect;
    public EventHandler<ServerData> OnDisconnect;
    public EventHandler<ServerData> OnReceived;
    public EventHandler<ReportData> OnReport;
    public EventHandler<string> OnServerCriticalFail;
    public static Server instance;
    public int time_count = 0;
    public bool listening = true;

    public static void init(string ip,int port)
    {
        if (instance != null) return;
        instance = new Server(ip, port);
    }

    public int port;
    private int MAX_CLIENTS = 5;

    public bool hasMaster()
    {
        for(int i = 0; i < clients.Count;i++)
        {
            if (clients[i].session.master_access)
            {
                return true;
            }
        }
        return false;
    }

    public static void disposeListeners() {
        instance.OnConnect = null;
        instance.OnReport = null;
    }

    public Server(String ip, int port)
    {
        this.ip = ip;
        this.port = port;
    }

    public void start()
    {
        new Thread(listenThread).Start();
    }

    public void releaseClient(ClientHandler client)
    {
       clients.Remove(client);
    }

    public void shutdown()
    {
        clients.ForEach((client) =>
        {
            client.request_close = true;
        });
        clients.Clear();
        clients = null;
        listening = false;
        server.Close();
    }

    private void listenThread()
    {
        try
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            server.Listen(20);
            while (listening)
            {
                Socket client = server.Accept();
                if (clients.Count + 1 < MAX_CLIENTS)
                {
                    ClientHandler client_handler = new ClientHandler(client, this);
                    client_handler.start();
                    clients.Add(client_handler);
                }
                else if(client != null)
                {
                    ByteStream temp = new ByteStream(1024);
                    temp.writeShort(ConstantsResponse.SRV_REJECT);
                    client.Send(temp.getBuffer());
                    client.Close();
                    temp = null;
                } else
                {
                    break;
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
            listening = false;
            OnServerCriticalFail?.Invoke(this, "El servidor esta teniendo problemas para iniciar\n"+e.ToString());
        }
    }
}
