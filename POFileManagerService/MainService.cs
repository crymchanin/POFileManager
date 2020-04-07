#region Пространства имен
using Feodosiya.Lib.IO.Pipes;
using Feodosiya.Lib.Logs;
using POFileManagerService.Tasks;
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

            if (!(ServiceHelper.IsInitialized = ServiceHelper.PreInit(MainEventLog))) {
                Stop();
                return;
            }

            ServiceHelper.CreateMessage("Инициализация конфигурации приложения...", MessageType.Information, true);
            if (!(ServiceHelper.IsInitialized = ServiceHelper.InitConfiguration())) {
                Stop();
                return;
            }

            ServiceHelper.CreateMessage("Инициализация основных параметров...", MessageType.Information, true);
            if (!(ServiceHelper.IsInitialized = ServiceHelper.InitEngine())) {
                Stop();
                return;
            }

            ServiceHelper.CreateMessage("Инициализация планировщика...", MessageType.Information, true);
            ServiceHelper.MainTimer = new Timer(delegate (object s) {
                TasksHelper.RunTasksThread();
            }, null, Timeout.Infinite, Timeout.Infinite);
        }

        protected override void OnStart(string[] args) {
            try {
                if (!ServiceHelper.IsInitialized) {
                    return;
                }

                ServiceHelper.CreateMessage("Запуск планировщика...", MessageType.Information, true);
                ServiceHelper.MainTimer.Change(0, Math.Max(ServiceHelper.MinimumMainTimerInterval, ServiceHelper.Configuration.TaskInterval));
                ServiceHelper.CreateMessage("Инициализация и запуск именованного канала...", MessageType.Information, true);
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
                        ServiceHelper.CreateMessage("Отправка сигнала о завершении выполнения задач", MessageType.Information);
                        NamedPipeListener<string>.SendMessage("POFileManagerClient", "complete");
                    }
                    catch { }
                    ServiceHelper.CreateMessage("Завершение работы службы...", MessageType.Information, true);
                }

                if (ServiceHelper.MainTimer != null) {
                    ServiceHelper.CreateMessage("Остановка планировщика...", MessageType.Information, true);
                    ServiceHelper.MainTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
                if (ServiceHelper.NamedPipeListener != null) {
                    ServiceHelper.CreateMessage("Отключение именованного канала...", MessageType.Information, true);
                    ServiceHelper.NamedPipeListener.End();
                }
            }
            catch (Exception ex) {
                ServiceHelper.CreateMessage("Ошибка при остановке службы:" + ex.ToString(), MessageType.Error, true);
            }
        }
    }
}
