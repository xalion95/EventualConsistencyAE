using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace EventualConsistencyAE.Connections
{
    public class Server
    {
        public static readonly IPAddress Address = IPAddress.Parse("127.0.0.1");

        private readonly List<Connection> _connections;

        private readonly TcpListener _listener;
        private readonly int _port;
        private readonly int _startPort;
        private readonly int _endPort;

        public delegate void ServerConnectedEvent(Connection connection);

        public event ServerConnectedEvent ServerConnected;

        public Server(int port, int startPort, int endPort)
        {
            try
            {
                _port = port;
                _startPort = startPort;
                _endPort = endPort;

                _connections = new List<Connection>();
                _listener = new TcpListener(Address, port);
                _listener.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                _listener.Stop();
            }
        }

        public void AcceptConnections()
        {
            try
            {
                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    var connection = new Connection(client);
                    _connections.Add(connection);

                    ServerConnected?.Invoke(connection);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void ConnectWithOthers()
        {
            for (var i = _startPort; i <= _endPort; i++)
            {
                if (i == _port || _connections.Any(connection => connection.Port == i)) continue;

                try
                {
                    var client = new TcpClient(Address.ToString(), i);
                    var connection = new Connection(client);
                    _connections.Add(connection);

                    ServerConnected?.Invoke(connection);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }

            _listener.Stop();
        }
    }
}