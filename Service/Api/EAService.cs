using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Timers;
using Service.Model;

namespace Service.Api
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EAService : IEAService
    {
        private readonly object _locker = new object();
        public List<ConnectedClient> Clients { get; } = new List<ConnectedClient>();

        public List<Person> Persons { get; } = new List<Person>();
        public int Port { get; }


        public EAService(int port)
        {
            Port = port;

            var timer = new Timer
            {
                AutoReset = true,
                Interval = 5 * 1000,
                // Enabled = true
            };
            timer.Elapsed += (sender, args) =>
            {
                var updatedPersons = Persons.Where(person => person.Status != Status.SYNCHRONIZED).ToList();

                if (updatedPersons.Count == 0) return;
                
                Clients[new Random().Next(Clients.Count)].Channel.AddPersons(updatedPersons);

                foreach (var person in updatedPersons.Where(person => person.Status == Status.NEW))
                {
                    person.Status = Status.SYNCHRONIZED;
                }

                Persons.RemoveAll(person => person.Status == Status.REMOVED);
            };
        }

        #region IEAService interface

        public void Connect(int clientPort)
        {
            var currentContext = OperationContext.Current;

            var client = Clients.Find(c => c.Port == clientPort);

            if (client == null)
            {
                var channel = CreateChannel(clientPort);
                client = new ConnectedClient(clientPort, channel) {SessionId = currentContext.SessionId};
                Clients.Add(client);
            }
            else if (client.SessionId == null)
            {
                client.SessionId = currentContext.SessionId;
            }
        }

        public void Disconnect()
        {
            var sessionId = OperationContext.Current.SessionId;
            Clients.RemoveAll(c => c.SessionId == sessionId);
        }

        public void AddPersons(List<Person> updatedPersons)
        {
            lock (_locker)
            {
                foreach (var person in updatedPersons)
                {
                    var p = Persons.FirstOrDefault(p1 => p1.Id == person.Id);

                    if (p == null) Persons.Add(person.Copy());
                    else if (person.Status == Status.REMOVED) p.Status = Status.REMOVED;
                }
            }
        }

        #endregion

        #region Collection

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

        #endregion

        public void Stop()
        {
            Clients.AsParallel().ForAll(client => client.Channel.Disconnect());
        }

        public void ConnectWithClient(int clientPort)
        {
            if (Clients.Any(client => client.Port == clientPort)) return;

            var channel = CreateChannel(clientPort);
            channel.Connect(Port);
            Clients.Add(new ConnectedClient(clientPort, channel));
        }

        public void DisconnectWithAllClients()
        {
            Clients.ForEach(client => client.Channel.Disconnect());
            Clients.Clear();
        }

        private static IEAService CreateChannel(int port)
        {
            var timeSpan = TimeSpan.FromSeconds(15);
            var clientUrl = $@"http://localhost:{port}/IEAService";
            var binding = new WSHttpBinding();
            var factory = new ChannelFactory<IEAService>(binding, clientUrl);

            return factory.CreateChannel();
        }
    }
}