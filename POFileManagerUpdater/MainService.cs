using POFileManagerUpdater.Updates;
using System;
using System.ServiceProcess;
using System.Threading;


namespace POFileManagerUpdater {
    public partial class MainService : ServiceBase {
        public MainService() {
            InitializeComponent();

            #if DEBUG
                Thread.Sleep(5000);
            #endif

            if (!(ServiceHelper.IsInitialized = ServiceHelper.PreInit(MainEventLog))) {
                Stop();
                return;
            }

            ServiceHelper.CreateMessage("Инициализация конфигурации приложения...", ServiceHelper.MessageType.Debug);
            if (!(ServiceHelper.IsInitialized = ServiceHelper.InitConfiguration())) {
                Stop();
                return;
            }

            ServiceHelper.CreateMessage("Инициализация планировщика...", ServiceHelper.MessageType.Debug);
            ServiceHelper.MainTimer = new Timer(delegate (object s) {
                UpdatesHelper.RunUpdate();
            }, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnStart(string[] args) {
            try {
                if (!ServiceHelper.IsInitialized) {
                    return;
                }

                ServiceHelper.CreateMessage("Запуск планировщика...", ServiceHelper.MessageType.Information);
                ServiceHelper.MainTimer.Change(0, Math.Max(ServiceHelper.MinimumMainTimerInterval, ServiceHelper.Configuration.UpdateCheckInterval * 60 * 1000));
            }
            catch (Exception ex) {
                ServiceHelper.CreateMessage("Ошибка при запуске службы:" + ex.ToString(), ServiceHelper.MessageType.Error);
            }
        }

        protected override void OnStop() {
            try {
                if (ServiceHelper.IsRunning) {
                    ServiceHelper.CreateMessage("Ожидание завершения работы службы...", ServiceHelper.MessageType.Information);
                    for (int i = 0; i < ServiceHelper.Configuration.AdditionalTime; i++) {
                        RequestAdditionalTime(2000);
                        Thread.Sleep(1000);
                    }

                    ServiceHelper.CreateMessage("Завершение работы службы...", ServiceHelper.MessageType.Information);
                }

                if (ServiceHelper.MainTimer != null) {
                    ServiceHelper.CreateMessage("Остановка планировщика...", ServiceHelper.MessageType.Information);
                    ServiceHelper.MainTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            catch (Exception ex) {
                ServiceHelper.CreateMessage("Ошибка при остановке службы:" + ex.ToString(), ServiceHelper.MessageType.Error);
            }
        }
    }
}
