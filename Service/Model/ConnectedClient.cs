using Service.Api;

namespace Service.Model
{
    public class ConnectedClient
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public IEAServiceCallback Callback { get; set; }
    }
}