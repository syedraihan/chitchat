using System;
using System.Windows;

namespace ChitChat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ChitChatApplication app;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var view = new MainWindow();
            var viewModel = new ChitChatViewModel();
            app = new ChitChatApplication(viewModel);

            view.DataContext = viewModel;
            view.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            app.Close();
        }
    }
}
