using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using EventualConsistencyAE.Web;
using Service.Model;

namespace EventualConsistencyAE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<Server> _servers = new List<Server>();

        public MainWindow()
        {
            InitializeComponent();

            for (var i = 0; i < 11; i++)
            {
                var server = new Server(i + 60_000);
                server.UpdateList += UpdateList;
                _servers.Add(server);

                ListViewServers.Items.Add(server);
            }
        }

        #region Events

        private void ToggleServer_OnClick(object sender, RoutedEventArgs e)
        {
            var server = (Server) ((Button) sender).DataContext;

            if (!server.IsRunning)
            {
                server.Start();
                ListViewServerConnections.Items.Clear();

                foreach (var clientServer in _servers
                    .Where(clientServer => clientServer != server &&
                                           clientServer.IsRunning &&
                                           server.Service.Clients.All(client => client.Port != clientServer.Port)))
                {
                    server.AddClient(clientServer.Port);
                }

                server.Service.Clients.ForEach(client => ListViewServerConnections.Items.Add(client));
            }
            else
            {
                server.IsRunning = false;
                server.Service.DisconnectWithAllClients();
                ListViewServerConnections.Items.Clear();
            }
        }

        private void StartServers_OnClick(object sender, RoutedEventArgs e)
        {
            _servers.Where(server => !server.IsRunning).ToList().ForEach(server => server.Start());
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var server in _servers.Where(server => server.IsRunning))
            {
                foreach (var clientServer in _servers
                    .Where(clientServer => server != clientServer && clientServer.IsRunning))
                {
                    server.AddClient(clientServer.Port);
                }
            }
        }

        private void UpdateList(object sender, List<Person> persons)
        {
            var data = ListViewPersonData.Items;

            data.Clear();

            // foreach (var perStartServers_OnClick(person => person.Status != Status.REMOVED))
            // {
            //     data.Add(person);
            // }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Parallel.ForEach(_servers, new ParallelOptions {MaxDegreeOfParallelism = 32}, server => server.Stop());
        }

        private void DeletePerson_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // var person = (Person) ((Button) sender).DataContext;
                // ListViewPersonData.Items.Remove(person);
                // _server.Service.RemovePerson(person.Id);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void AddPersonButton_OnClick(object sender, RoutedEventArgs e)
        {
            // var person = new Person
            // {
            //     Id = int.Parse(IdField.Text),
            //     Name = NameField.Text
            // };
            // _server.Service.AddPerson(person.Id, person.Name);
            // ListViewPersonData.Items.Add(person);
        }

        private void Servers_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var server = (Server) ((ListView) sender).SelectedItem;

            ListViewServerConnections.Items.Clear();

            foreach (var serverClient in server.Service.Clients)
            {
                ListViewServerConnections.Items.Add(serverClient);
            }
        }

        private void TabView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = ((TabControl) sender).SelectedItem;

            if (!Equals(tab, ConnectionMapTabItem)) return;

            ConnectionMap.Dispatcher?.Invoke(() => DrawHelper.DrawConnectionMap(ConnectionMap, _servers),
                DispatcherPriority.Loaded);
        }

        #endregion
    }
}