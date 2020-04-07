#region Пространства имен
using System.ServiceProcess;
#endregion


namespace POFileManagerService {
    static class Program {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main() {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MainService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
