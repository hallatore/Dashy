using System.Linq;
using System.Windows;

namespace Dashy
{
    public partial class App : Application
    {
        public string Profile { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Profile = e.Args.FirstOrDefault() ?? "settings";
        }
    }
}
