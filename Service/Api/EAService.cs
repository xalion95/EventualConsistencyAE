using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Timers;
using Service.Model;

namespace Service.Api
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EAService : IEAService
    {
        #region Events

        public delegate void ClientListHandler(object sender, List<ConnectedClient> clients);

        public event ClientListHandler ConnectedClient;

        #endregion

        private readonly List<ConnectedClient> _clients = new List<ConnectedClient>();
        private readonly Timer _timer;
        public List<Person> Persons { get; } = new List<Person>();

        public EAService()
        {
            _timer = new Timer
            {
                AutoReset = true,
                Interval = 30 * 1000,
                Enabled = true
            };
            _timer.Elapsed += (sender, args) =>
            {
                var updatedPersons = Persons.Where(person => person.Status != Status.SYNCHRONIZED).ToList();

                if (updatedPersons.Count == 0) return;

                _clients.AsParallel().ForAll(client => client.Callback.Synchronize(updatedPersons));

                foreach (var person in updatedPersons.Where(person => person.Status == Status.NEW))
                {
                    person.Status = Status.SYNCHRONIZED;
                }

                Persons.RemoveAll(person => person.Status == Status.REMOVED);
            };
        }

        #region IEAService interface

        public void Connect()
        {
            var currentContext = OperationContext.Current;
            var callback = currentContext.GetCallbackChannel<IEAServiceCallback>();
            var endpoint =
                (RemoteEndpointMessageProperty) currentContext.IncomingMessageProperties[
                    RemoteEndpointMessageProperty.Name];

            if (endpoint == null) return;

            var client = new ConnectedClient
            {
                Address = endpoint?.Address,
                Port = endpoint.Port,
                Callback = callback,
            };

            if (_clients.Contains(client)) return;

            _clients.Add(client);

            ConnectedClient?.Invoke(this, _clients);
        }

        public void Disconnect()
        {
            var currentContext = OperationContext.Current;
            var callback = currentContext.GetCallbackChannel<IEAServiceCallback>();
            var result = _clients.RemoveAll(c => c.Callback == callback);

            if (result <= 0) return;

            ConnectedClient?.Invoke(this, _clients);
        }

        #endregion

        public void AddPerson(int id, string name)
        {
            Persons.Add(new Person
            {
                Id = id,
                Name = name,
                Status = Status.NEW
            });
        }

        public void RemovePerson(int id)
        {
            foreach (var person in Persons.Where(person => person.Id == id))
            {
                person.Status = Status.REMOVED;
            }
        }

        public List<string> GetPersons()
        {
            return Persons.Select(person => person.Name).ToList();
        }

        public void Stop()
        {
            _clients.AsParallel().ForAll(client => client.Callback.Disconnect());
        }
    }
}