#region Пространства имен
using System.ComponentModel;
#endregion


namespace POFileManagerService {
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer {
        public ProjectInstaller() {
            InitializeComponent();
        }
    }
}
