using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EventualConsistencyAE.Connections;

namespace EventualConsistencyAE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Server _server;
        private ObservableCollection<string> _servers;

        public MainWindow(int port, int start, int end)
        {
            InitializeComponent();
            _servers = new ObservableCollection<string>();
            ListViewConnectedServers.ItemsSource = _servers;

            _server = new Server(port, start, end);
            _server.ServerConnected += connection =>
                ListViewConnectedServers.Dispatcher?.Invoke(() => _servers.Add(connection.ToString()));

            Task.Run(() => _server.AcceptConnections());

            ConnectButton.Click += (sender, args) => _server.ConnectWithOthers();
        }
    }
}