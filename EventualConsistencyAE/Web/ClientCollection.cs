using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Model;

namespace EventualConsistencyAE.Web
{
    public class ClientCollection : IEnumerable<Client>
    {
        private readonly List<Client> _clients = new List<Client>();

        #region Events

        public event Client.ServerSynchronizeHandler ServerSynchronize;

        #endregion

        public void AddClient(int serverPort)
        {
            var client = new Client(serverPort);
            client.ServerDisconnected += ServerDisconnectedEvent;
            client.ServerSynchronize += ServerSynchronizeEvent;
            _clients.Add(client);
        }

        public async Task ConnectWithAll()
        {
            await Task.Run(() => Parallel.ForEach(_clients, client => client.Channel.Connect()));
        }

        private void ServerSynchronizeEvent(object sender, List<Person> updatedPersons)
        {
            ServerSynchronize?.Invoke(this, updatedPersons);
        }

        private void ServerDisconnectedEvent(object sender)
        {
            _clients.Remove((Client) sender);
        }

        public void DisconnectAll()
        {
            Parallel.ForEach(_clients,
                new ParallelOptions {MaxDegreeOfParallelism = 32},
                client =>
                {
                    try
                    {
                        client.Channel.Disconnect();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                });
            _clients.Clear();
        }

        public IEnumerator<Client> GetEnumerator()
        {
            return ((IEnumerable<Client>) _clients).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}