using System.Globalization;
using System.Threading;
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
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}