using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Service.Api;
using Service.Model;

namespace EventualConsistencyAE.Web
{
    public class Server
    {
        #region Events

        public delegate void UpdateListHandler(object sender, List<Person> persons);

        public event UpdateListHandler UpdateList;

        #endregion

        private static readonly object Locker = new object();
        private readonly ServiceHost _serviceHost;
        public EAService Service { get; }

        private readonly ClientCollection _clientCollection;

        public Server(int port)
        {
            _clientCollection = new ClientCollection();
            _clientCollection.ServerSynchronize += Synchronize;

            var binding = new WSDualHttpBinding();
            var smb = new ServiceMetadataBehavior {HttpGetEnabled = true};
            Service = new EAService();


            _serviceHost = new ServiceHost(Service, new Uri($"http://localhost:{port}"));
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
            _clientCollection.AddClient(port);
        }

        public void Start()
        {
            _serviceHost.Open();
        }

        public void Stop()
        {
            _clientCollection.DisconnectAll();
            Service.Stop();
        }
    }
}