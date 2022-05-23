using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

public class ClientHandler
{
	public Socket socket;
	public ByteStream input, output;
	Server server;
	public bool request_close = false;
	public int _ClientID = 0;
	public string ip;
	public Session session;
	public int requestNum = 0;

	public ClientHandler(Socket socket, Server server)
	{
		this.socket = socket;
		this.server = server;
		input = new ByteStream(4096);
		output = new ByteStream(4096);
		ip = socket.RemoteEndPoint.ToString();
		session = new Session();
	}

	public void start()
	{
		new Thread(runMainProcess).Start();
	}
	
	public void disconnect()
	{
		request_close = true;
		ServerData args = new ServerData();
		args.client = this;
		args.server = server;
		server.OnDisconnect?.Invoke(this, args);
	}

	public void begin(int responseID)
    {
		output.rewind();
		output.writeInt(responseID);
	}

	public void end()
    {
		socket.Send(output.getBuffer());
    }

	public void runMainProcess()
	{
		try
		{
			ServerData args = new ServerData();
			args.client = this;
			args.server = server;
			args.buffer = input;
			server.OnConnect?.Invoke(this, args);
			while (!request_close)
			{
				int received = socket.Receive(input.getBuffer());
				if (received > 0)
				{
					input.rewind();
					args = new ServerData();
					args.requestID = input.readInt();
					args.client = this;
					args.server = server;
					args.buffer = input;
					if(args.requestID == 0xFFC) // user type
					{
						if(input.readInt() == 1) // master
                        {
							session.master_access = true;
						}
                        else
                        {
							// create client id
							_ClientID = new Random().Next(-20000, 20000);
							// send
							begin(0xFFA);
							output.writeInt(_ClientID);
							end();
						}
						continue;
                    }
					server.OnReceived?.Invoke(this, args);
					requestNum++;
				}
				Thread.Sleep(60);
			}
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
			server.clients.Remove(this);
		} finally {
            try
            {
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
	}
}
