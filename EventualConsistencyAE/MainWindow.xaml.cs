using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        public ObservableCollection<Server> Servers { get; } = new ObservableCollection<Server>();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            for (var i = 0; i < 11; i++)
            {
                var server = new Server(i + 60_000);
                server.Service.UpdateList += UpdateList;
                Servers.Add(server);

                ListViewServers.Items.Add(server);

                GridViewPersonData.Columns.Add(new GridViewColumn
                {
                    Header = $"Server {i + 60_000}",
                    DisplayMemberBinding = new Binding($"[{i}]"),
                    Width = 125
                });
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

                foreach (var clientServer in Servers
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

        private void CloseConnection_OnClick(object sender, RoutedEventArgs e)
        {
            var client = (ConnectedClient) ((Button) sender).DataContext;
            var server = (Server) ListViewServers.SelectedItem;

            server.DisconnectWithClient(client.Port);
            ListViewServerConnections.Items.Remove(client);
        }

        private void StartServers_OnClick(object sender, RoutedEventArgs e)
        {
            Servers.Where(server => !server.IsRunning).ToList().ForEach(server => server.Start());
        }

        private void DisconnectServers_OnClick(object sender, RoutedEventArgs e)
        {
            Servers.Where(server => server.IsRunning).ToList().ForEach(server =>
            {
                server.IsRunning = false;
                server.Service.DisconnectWithAllClients();
                ListViewServerConnections.Items.Clear();
            });
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var server in Servers.Where(server => server.IsRunning))
            {
                foreach (var clientServer in Servers
                    .Where(clientServer => server != clientServer && clientServer.IsRunning))
                {
                    server.AddClient(clientServer.Port);
                }
            }
        }

        private void UpdateList(object sender)
        {
            ListViewPersonData.Items.Clear();

            for (var i = 0; i < Servers.Max(server => server.Service.PersonCount); i++)
            {
                var row = new string[Servers.Count];

                for (var j = 0; j < Servers.Count; j++)
                {
                    var server = Servers[j];

                    if (i >= server.Service.PersonCount) continue;

                    var person = server.Service.Persons[i];
                    row[j] = person.Id + " - " + person.Name;
                }

                ListViewPersonData.Items.Add(row);
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Parallel.ForEach(Servers, new ParallelOptions {MaxDegreeOfParallelism = 32}, server => server.Stop());
        }

        private void AddPersonButton_OnClick(object sender, RoutedEventArgs e)
        {
            var person = new Person
            {
                Id = int.Parse(IdField.Text),
                Name = NameField.Text
            };

            var selectedServer = (Server) SelectedServerComboBox.SelectedItem;

            if (selectedServer.IsRunning)
                selectedServer.Service.AddPerson(person.Id, person.Name);
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

            ConnectionMap.Dispatcher?.Invoke(() => DrawHelper.DrawConnectionMap(ConnectionMap, Servers.ToList()),
                DispatcherPriority.Loaded);
        }

        private void SelectedServerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedServer = (Server)SelectedServerComboBox.SelectedItem;

            if (!selectedServer.IsRunning)
                addButton.IsEnabled = false;
            else
                addButton.IsEnabled = true;
        }

        #endregion
    }
}