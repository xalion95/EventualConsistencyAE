using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                var server = new Server(i + 60_000, i);
                server.UpdateList += UpdateList;
                _servers.Add(server);

                ListViewServers.Items.Add(server);
            }
        }

        #region Events

        private void ToggleServer_OnClick(object sender, RoutedEventArgs e)
        {
            var server = (Server) ((Button) sender).DataContext;

            if (server.IsRunning) server.Stop();
            else server.Start();
        }

        private void StartServers_OnClick(object sender, RoutedEventArgs e)
        {
            _servers.ForEach(server => server.Start());
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

                server.Clients?.ConnectWithAll();
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
            Parallel.ForEach(_servers,
                new ParallelOptions {MaxDegreeOfParallelism = 32},
                server => server.Stop());
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

        #endregion

        private void Servers_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var server = (Server) ((ListView) sender).SelectedItem;

            ListViewServerConnections.Items.Clear();

            foreach (var serverClient in server.Clients)
            {
                ListViewServerConnections.Items.Add(serverClient);
            }
        }
    }
}