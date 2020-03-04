using System.Net;
using System.Net.Sockets;

namespace EventualConsistencyAE.Connections
{
    public class Connection
    {
        private TcpClient _client;
        public readonly IPAddress Address;
        public readonly int Port;

        public Connection(TcpClient client)
        {
            _client = client;

            var remoteEndPoint = ((IPEndPoint) _client.Client.RemoteEndPoint);
            Address = remoteEndPoint.Address;
            Port = remoteEndPoint.Port;
        }

        public override string ToString()
        {
            return Address + ":" + Port;
        }
    }
}