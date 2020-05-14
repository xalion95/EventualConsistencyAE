using System;
using System.Collections.Generic;
using System.ServiceModel;
using Service.Api;
using Service.Model;

namespace EventualConsistencyAE.Web
{
    public class Client : IEAServiceCallback
    {
        public IEAService Channel { get; }
        public int ServerPort { get; }

        #region Events

        public delegate void ServerDisconnectedHandler(object sender);

        public event ServerDisconnectedHandler ServerDisconnected;

        public delegate void ServerSynchronizeHandler(object sender, List<Person> updatedPersons);

        public event ServerSynchronizeHandler ServerSynchronize;

        #endregion

        public Client(int serverPort)
        {
            try
            {
                ServerPort = serverPort;

                var timeSpan = new TimeSpan(0, 0, 15);
                var serverUrl = $@"http://localhost:{serverPort}/IEAService";
                var binding = new WSDualHttpBinding
                {
                    OpenTimeout = timeSpan,
                    CloseTimeout = timeSpan,
                    SendTimeout = timeSpan,
                    ReceiveTimeout = timeSpan
                };
                var factory = new DuplexChannelFactory<IEAService>(new InstanceContext(this), binding, serverUrl);

                Channel = factory.CreateChannel();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #region IEAServiceCallback interface

        public void Disconnect()
        {
            ServerDisconnected?.Invoke(this);
        }

        public void Synchronize(List<Person> newPersons)
        {
            ServerSynchronize?.Invoke(this, newPersons);
        }

        #endregion
    }
}