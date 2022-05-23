using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
public class ServerInfoResolver
{
    private static ServerInfoResolver instance;
    public EventHandler<EventArgs> OnUpdateView;
    public List<ClientConnectionItem> cn_clients;

    public ServerInfoResolver()
    {
        cn_clients = new List<ClientConnectionItem>();
        new Thread(() => {
            while (true) {
                // Update temporaly affected variables
                // Server
                Server.instance.time_count++;
                if (!Server.instance.listening)
                {
                    break;
                }
                // Clients
                // Se detecta que hay nuevos clientes o clientes desconectados
                if (cn_clients.Count != Server.instance.clients.Count)
                {
                    cn_clients.Clear();
                    for (int i = 0; i < Server.instance.clients.Count;i++) {
                        ClientHandler client = Server.instance.clients[i];
                        cn_clients.Add(new ClientConnectionItem()
                        {
                            ClientID = (client.session.master_access ? "[root]" : "0x"+ client._ClientID.ToString("X4")),
                            IpAdress = client.ip,
                            Time = "0:00",
                            requestNum = "0",
                            Access = "Indefined"
                        });
                    }
                }

                for (int i = 0; i < Server.instance.clients.Count; i++)
                {
                    ClientHandler client = Server.instance.clients[i];
                    Session session = client.session;
                    session.timecount++;
                    cn_clients[i].Access = ((session.master_access && session.login) ? "Master" : (session.master_access ? "Root" : "Normal"));
                    cn_clients[i].Time = toTimeFormat(session.timecount);
                    cn_clients[i].requestNum = "" + client.requestNum;
                }

                // Update View
                OnUpdateView?.Invoke(this, new EventArgs());
                Thread.Sleep(1000);
                if (!Server.instance.listening)
                {
                    break;
                }
            }
        }).Start();
    }
    private string toTimeFormat(int count)
    {
        int minutes = (count / 60);
        return (minutes / 60) + ":" + ((minutes % 60 < 10) ? "0" : "") + (minutes % 60) + ":" + ((count % 60 < 10) ? "0" : "") + (count % 60);
    }
    public static ServerInfoResolver getInstance()
    {
        if (instance == null) instance = new ServerInfoResolver();
        return instance;
    }
    
}