using System;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Service.Api;

namespace EventualConsistencyAE.Web
{
    public class Server : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private readonly object _locker = new object();
        private ServiceHost _serviceHost;

        public EAService Service { get; }
        public int Port { get; }

        private bool _isRunning;

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
            }
        }

        public Server(int port)
        {
            Port = port;

            var binding = new WSHttpBinding();
            var smb = new ServiceMetadataBehavior {HttpGetEnabled = true};
            Service = new EAService(port);

            _serviceHost = new ServiceHost(Service, new Uri($@"http://localhost:{port}"));
            _serviceHost.Description.Behaviors.Add(smb);
            _serviceHost.AddServiceEndpoint(typeof(IEAService), binding, "IEAService");
        }

        public void AddClient(int clientPort)
        {
            Service.ConnectWithClient(clientPort);
        }

        public void Start()
        {
            if (_serviceHost.State == CommunicationState.Created)
            {
                _serviceHost.Open();
            }

            IsRunning = true;
        }

        public void DisconnectWithClient(int clientPort)
        {
            foreach (var c in Service.Clients.Where(client => client.Port == clientPort))
            {
                c.Channel.Disconnect();
            }

            Service.Clients.RemoveAll(client => client.Port == clientPort);
        }

        public void Stop()
        {
            _serviceHost.Close();
            IsRunning = false;
        }
    }
}