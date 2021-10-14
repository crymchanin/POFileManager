#region Пространства имен
using Feodosiya.Lib.IO.Pipes;
using Feodosiya.Lib.Logs;
using POFileManagerService.Tasks;
using POFileManagerService.Updates;
using System;
using System.ServiceProcess;
using System.Threading;
#endregion

// HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\WaitToKillServiceTimeout (ms)
namespace POFileManagerService {
    public partial class MainService : ServiceBase {
        public MainService() {
            InitializeComponent();

            #if DEBUG
                Thread.Sleep(5000);
            #endif
        }

        protected override void OnStart(string[] args) {
            try {
                if (!(ServiceHelper.IsInitialized = ServiceHelper.PreInit(MainEventLog))) {
                    return;
                }

                ServiceHelper.CreateMessage("Инициализация конфигурации приложения...", MessageType.Debug, 1);
                if (!(ServiceHelper.IsInitialized = ServiceHelper.InitConfiguration())) {
                    return;
                }

                ServiceHelper.CreateMessage("Инициализация основных параметров...", MessageType.Debug, 1);
                if (!(ServiceHelper.IsInitialized = ServiceHelper.InitEngine())) {
                    return;
                }

                ServiceHelper.CreateMessage("Инициализация планировщика...", MessageType.Debug, 1);
                ServiceHelper.MainTimer = new Timer(delegate (object s) {
                    TasksHelper.RunTasksThread();
                }, null, Timeout.Infinite, Timeout.Infinite);

                ServiceHelper.CreateMessage("Запуск планировщика...", MessageType.Information, true);
                ServiceHelper.MainTimer.Change(0, Math.Max(ServiceHelper.MinimumMainTimerInterval, ServiceHelper.Configuration.TaskInterval * 60 * 1000));
                ServiceHelper.CreateMessage("Инициализация и запуск именованного канала...", MessageType.Debug, 1);
                ServiceHelper.NamedPipeListener = new NamedPipeListener<string>(ServiceHelper.ProductName, System.Security.Principal.WellKnownSidType.AuthenticatedUserSid);
                ServiceHelper.NamedPipeListener.MessageReceived += delegate (object sender, NamedPipeListenerMessageReceivedEventArgs<string> e) {
                    switch (e.Message) {
                        case "force":
                            TasksHelper.RunTasksThread();
                            break;
                        default:
                            break;
                    }
                };
                ServiceHelper.NamedPipeListener.Error += delegate (object sender, NamedPipeListenerErrorEventArgs e) {
                    ServiceHelper.CreateMessage(string.Format("Ошибка Pipe: ({0}): {1}", e.ErrorType, e.Exception.Message), MessageType.Error, true);
                };
                ServiceHelper.NamedPipeListener.Start();

                if (ServiceHelper.Configuration.Updates.CheckUpdates) {
                    ServiceHelper.CreateMessage("Проверка обновлений для службы автообновлений...", MessageType.Debug, 1);
                    UpdateHelper.CheckUpdatesForUpdater();

                    UpdateHelper.CheckAndInstallConfigUpdate();
                }
            }
            catch (Exception ex) {
                ServiceHelper.CreateMessage("Ошибка при запуске службы:" + ex.ToString(), MessageType.Error, true);
            }
        }

        protected override void OnStop() {
            try {
                if (ServiceHelper.IsRunning) {
                    ServiceHelper.CreateMessage("Ожидание завершения работы службы...", MessageType.Information, true);
                    for (int i = 0; i < ServiceHelper.Configuration.AdditionalTime; i++) {
                        RequestAdditionalTime(2000);
                        Thread.Sleep(1000);
                    }
                    try {
                        ServiceHelper.CreateMessage("Отправка сигнала о завершении выполнения задач...", MessageType.Debug, 1);
                        NamedPipeListener<string>.SendMessage("POFileManagerClient", "complete", 10000);
                    }
                    catch { }
                    ServiceHelper.CreateMessage("Завершение работы службы...", MessageType.Information, true);
                }

                if (ServiceHelper.MainTimer != null) {
                    ServiceHelper.CreateMessage("Остановка планировщика...", MessageType.Information, true);
                    ServiceHelper.MainTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
                if (ServiceHelper.NamedPipeListener != null) {
                    ServiceHelper.CreateMessage("Отключение именованного канала...", MessageType.Debug, 1);
                    ServiceHelper.NamedPipeListener.End();
                }
            }
            catch (Exception ex) {
                ServiceHelper.CreateMessage("Ошибка при остановке службы:" + ex.ToString(), MessageType.Error, true);
            }
        }
    }
}
