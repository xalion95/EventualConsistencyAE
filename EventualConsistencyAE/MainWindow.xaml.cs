using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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
                Servers.Add(server);

                ListViewServers.Items.Add(server);

                GridViewPersonData.Columns.Add(new GridViewColumn
                {
                    Header = $"Server {i + 60_000}",
                    //   DisplayMemberBinding = new Binding($"[{i}]"),
                    Width = 125,
                    CellTemplate = GetDataTemplate(i)
                });
            }

            var timer = new Timer
            {
                AutoReset = true,
                Interval = 250,
                Enabled = true
            };
            timer.Elapsed += UpdateList;
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
                server.Service.Clear();
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
                server.Service.Clear();
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

        private void UpdateList(object sender, ElapsedEventArgs e)
        {
            var items = ListViewPersonData.Items;
            items.Dispatcher?.Invoke(() =>
            {
                items.Clear();

                for (var i = 0; i < Servers.Max(server => server.Service.PersonCount); i++)
                {
                    var row = new Person[Servers.Count];
                    items.Add(row);

                    for (var j = 0; j < Servers.Count; j++)
                    {
                        var server = Servers[j];

                        if (i >= server.Service.PersonCount) continue;

                        var person = server.Service.Persons[i];
                        row[j] = person;
                    }
                }
            });
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

        private void RemovePersonButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedServer = (Server) SelectedServerRemoveComboBox.SelectedItem;

            if (selectedServer.IsRunning)
                selectedServer.Service.RemovePerson(int.Parse(IdRemoveField.Text));
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
            var selectedServer = (Server) SelectedServerComboBox.SelectedItem;

            AddButton.IsEnabled = selectedServer.IsRunning;
        }

        #endregion

        private static DataTemplate GetDataTemplate(int row)
        {
            var trigger = new DataTrigger {Binding = new Binding($"[{row}].IsRemoved"), Value = true};
            trigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.Gray));

            var style = new Style {TargetType = typeof(TextBlock)};
            style.Triggers.Add(trigger);

            var textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, new Binding($"[{row}]"));
            textFactory.SetValue(StyleProperty, style);

            return new DataTemplate {VisualTree = textFactory};
        }
    }
}