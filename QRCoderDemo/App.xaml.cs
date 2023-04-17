using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;

namespace QRCoderDemo
{
    public partial class App : Application
    {
        private Window? _window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {

            _window = new MainWindow();
            _window.Activate();
        }
    }
}