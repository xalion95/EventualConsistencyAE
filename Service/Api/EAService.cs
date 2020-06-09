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
        public List<ConnectedClient> Clients { get; } = new List<ConnectedClient>();

        public List<Person> Persons { get; } = new List<Person>();
        public int Port { get; }
        public int PersonCount { get; private set; }
        public bool IsClosed { get; private set; } = false;
        private Timer _timer;

        public delegate void UpdateListHandler(object sender);

        public event UpdateListHandler UpdateList;

        public EAService(int port)
        {
            Port = port;

            _timer = new Timer
            {
                AutoReset = true,
                Interval = 2 * 1000,
                Enabled = true
            };
            _timer.Elapsed += (sender, args) =>
            {
                if (Clients.Count == 0 || IsClosed) return;

                List<Person> response;

                try
                {
                    lock (Clients)
                    {
                        if (Clients.All(c => c.SessionId == null)) return;

                        ConnectedClient client;
                        do
                        {
                            client = Clients[new Random().Next(Clients.Count)];
                        } while (client.SessionId == null);

                        response = client.Channel.AddPersons(Persons);
                    }

                    if (response.Count <= 0) return;

                    foreach (var person in response.Where(person => Persons.All(person1 => person1.Id != person.Id)))
                    {
                        Persons.Add(person);
                    }

                    Persons.Sort((p1, p2) => p1.Id.CompareTo(p2.Id));
                    PersonCount = Persons.Count;
                }
                catch (Exception e)
                {
                    Console.WriteLine("client is closed");
                }
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

        public List<Person> AddPersons(List<Person> updatedPersons)
        {
            var isUpdated = false;

            lock (Persons)
            {
                foreach (var person in updatedPersons)
                {
                    var p = Persons.FirstOrDefault(p1 => p1.Id == person.Id);

                    if (p != null) continue;

                    Persons.Add(person.Copy());
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    Persons.Sort((p1, p2) => p1.Id.CompareTo(p2.Id));
                    PersonCount = Persons.Count;
                    UpdateList?.Invoke(this);
                }
            }

            return Persons.Where(person => updatedPersons.All(person1 => person1.Id != person.Id)).ToList();
        }

        #endregion

        #region Collection

        public void AddPerson(int id, string name)
        {
            Persons.Add(new Person
            {
                Id = id,
                Name = name
            });

            Persons.Sort((p1, p2) => p1.Id.CompareTo(p2.Id));
            PersonCount = Persons.Count;
            UpdateList?.Invoke(this);
        }

        #endregion

        public void ConnectWithClient(int clientPort)
        {
            var channel = CreateChannel(clientPort);
            channel.Connect(Port);

            lock (Clients)
            {
                Clients.Add(new ConnectedClient(clientPort, channel));
            }
        }

        public void DisconnectWithAllClients()
        {
            Clients.ForEach(client => client.Channel.Disconnect());
            Clients.Clear();
        }

        private static IEAService CreateChannel(int port)
        {
            var clientUrl = $@"http://localhost:{port}/IEAService";
            var binding = new WSHttpBinding();
            var factory = new ChannelFactory<IEAService>(binding, clientUrl);
            binding.ReceiveTimeout = TimeSpan.FromSeconds(2);

            return factory.CreateChannel();
        }

        public void Close()
        {
            IsClosed = true;
            _timer.Enabled = false;
        }
    }
}