using System;
using System.Windows;

namespace EventualConsistencyAE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var port = 50000;
            var startPort = 50000;
            var endPort = 50002;

            if (e.Args.Length > 2)
            {
                port = Convert.ToInt32(e.Args[0]);
                startPort = Convert.ToInt32(e.Args[1]);
                endPort = Convert.ToInt32(e.Args[2]);
            }

            var mainWindow = new MainWindow(port, startPort, endPort);
            mainWindow.Show();
        }
    }
}