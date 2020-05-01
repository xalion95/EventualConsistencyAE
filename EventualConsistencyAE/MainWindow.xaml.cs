using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EventualConsistencyAE.Web;
using Service.Model;

namespace EventualConsistencyAE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly int _port, _start, _end;
        private readonly Server _server;

        public MainWindow(int port, int start, int end)
        {
            _port = port;
            _start = start;
            _end = end;

            _server = new Server(port);
            _server.Service.ConnectedClient += ConnectedClient;
            _server.UpdateList += UpdateList;

            InitializeComponent();

            _server.Start();
        }

        private void ConnectedClient(object sender, List<ConnectedClient> clients)
        {
            ListViewConnectedServers.Items.Clear();

            foreach (var client in clients)
            {
                ListViewConnectedServers.Items.Add($"{client.Address}:{client.Port}");
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _server.Stop();
        }

        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            for (var i = _start; i <= _end; i++)
            {
                if (i == _port) continue;

                _server.AddClient(i);
            }
        }

        private void UpdateList(object sender, List<Person> persons)
        {
            var data = ListViewPersonData.Items;

            data.Clear();

            foreach (var person in persons.Where(person => person.Status != Status.REMOVED))
            {
                data.Add(person);
            }
        }

        private void DeletePerson_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var person = (Person) ((Button) sender).DataContext;
                ListViewPersonData.Items.Remove(person);
                _server.Service.RemovePerson(person.Id);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void AddPersonButton_OnClick(object sender, RoutedEventArgs e)
        {
            var person = new Person
            {
                Id = int.Parse(IdField.Text),
                Name = NameField.Text
            };
            _server.Service.AddPerson(person.Id, person.Name);
            ListViewPersonData.Items.Add(person);
        }
    }
}