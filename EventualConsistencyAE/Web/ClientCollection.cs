using System;
using System.Collections.Generic;
using Service.Model;

namespace EventualConsistencyAE.Web
{
    public class ClientCollection
    {
        private readonly List<Client> _clients = new List<Client>();

        #region Events

        public event Client.ServerSynchronizeHandler ServerSynchronize;

        #endregion

        public void AddClient(int serverPort)
        {
            var client = new Client(serverPort);
            client.Channel.Connect();
            client.ServerDisconnected += ServerDisconnectedEvent;
            client.ServerSynchronize += ServerSynchronizeEvent;
            _clients.Add(client);
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
            foreach (var client in _clients)
            {
                try
                {
                    client.Channel.Disconnect();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            _clients.Clear();
        }
    }
}