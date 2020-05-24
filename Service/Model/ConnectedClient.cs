using Service.Api;

namespace Service.Model
{
    public class ConnectedClient
    {
        public int Port { get; }
        public string SessionId { get; set; }
        public IEAService Channel { get; }

        public ConnectedClient(int port, IEAService channel)
        {
            Port = port;
            Channel = channel;
        }
    }
}