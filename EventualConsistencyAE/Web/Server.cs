using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Service.Api;
using Service.Model;

namespace EventualConsistencyAE.Web
{
    public class Server : INotifyPropertyChanged
    {
        #region Events

        public delegate void UpdateListHandler(object sender, List<Person> persons);

        public event UpdateListHandler UpdateList;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private static readonly object Locker = new object();
        private ServiceHost _serviceHost;
        public ClientCollection Clients { get; }

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

        public Server(int port, int serverId)
        {
            Port = port;

            Clients = new ClientCollection();
            Clients.ServerSynchronize += Synchronize;

            var binding = new WSDualHttpBinding();
            var smb = new ServiceMetadataBehavior {HttpGetEnabled = true};
            Service = new EAService(serverId);

            _serviceHost = new ServiceHost(Service, new Uri($@"http://localhost:{Port}"));
            _serviceHost.Description.Behaviors.Add(smb);
            _serviceHost.AddServiceEndpoint(typeof(IEAService), binding, "IEAService");
        }

        private void Synchronize(object sender, IEnumerable<Person> updatedPersons)
        {
            var persons = Service.Persons;

            lock (Locker)
            {
                foreach (var person in updatedPersons)
                {
                    var p = persons.FirstOrDefault(p1 => p1.Id == person.Id);

                    if (p == null) persons.Add(person.Copy());
                    else if (person.Status == Status.REMOVED) p.Status = Status.REMOVED;
                }
            }

            UpdateList?.Invoke(this, persons);
        }

        public void AddClient(int port)
        {
            Clients.AddClient(port);
        }

        public void Start()
        {
            if (_serviceHost.State != CommunicationState.Closed)
            {
                _serviceHost.Open();
            }
            else
            {
                var binding = new WSDualHttpBinding();
                var smb = new ServiceMetadataBehavior {HttpGetEnabled = true};

                _serviceHost = new ServiceHost(Service, new Uri($"http://localhost:{Port}"));
                _serviceHost.Description.Behaviors.Add(smb);
                _serviceHost.AddServiceEndpoint(typeof(IEAService), binding, "IEAService");
            }

            IsRunning = true;
        }

        public void Stop()
        {
            Clients.DisconnectAll();
            _serviceHost.Close();
            IsRunning = false;
        }
    }
}