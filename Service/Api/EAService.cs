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
        private readonly List<ConnectedClient> _clients = new List<ConnectedClient>();
        private readonly Timer _timer;
        public List<Person> Persons { get; } = new List<Person>();
        public int ServerId { get; }

        public EAService(int serverId)
        {
            ServerId = serverId;

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
        }

        public void Disconnect()
        {
            var currentContext = OperationContext.Current;
            var callback = currentContext.GetCallbackChannel<IEAServiceCallback>();
            _clients.RemoveAll(c => c.Callback == callback);
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