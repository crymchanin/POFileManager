#region Пространства имен
using Feodosiya.Lib.Conf;
using Feodosiya.Lib.IO;
using Feodosiya.Lib.IO.Pipes;
using Feodosiya.Lib.Logs;
using Feodosiya.Lib.Threading;
using POFileManagerClient.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
#endregion


namespace POFileManagerClient {
    public static class AppHelper {

        #region Члены и свойства класса
        /// <summary>
        /// Именованный канал
        /// </summary>
        public static NamedPipeListener<string> NamedPipeListener { get; set; }

        /// <summary>
        /// Текущая рабочая папка
        /// </summary>
        public static string CurrentDirectory { get; set; }

        /// <summary>
        /// Имя данного продукта
        /// </summary>
        public static string ProductName { get; set; }

        /// <summary>
        /// Версия данного приложения
        /// </summary>
        public static string Version { get; set; }

        /// <summary>
        /// GUID приложения
        /// </summary>
        public static string GUID { get; set; }

        /// <summary>
        /// Лог файл программы
        /// </summary>
        public static Log Log { get; set; }

        /// <summary>
        /// Конфигурационный файл
        /// </summary>
        public static ConfHelper ConfHelper { get; set; }

        /// <summary>
        /// Конфигурация программы
        /// </summary>
        public static Global Configuration { get; set; }
        #endregion

        #region Вспомогательные методы
        /// <summary>
        /// Создает сообщение, которое можно записать в лог, вывести в окне и отправить по почте
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="showMessageBox">Отображает окно сообщения если установлено в true</param>
        public static void CreateMessage(string message, MessageType messageType, bool showMessageBox = false) {
            if (messageType == MessageType.Debug) {
                if (Configuration != null) {
                    if (!Configuration.DebuggingEnabled) {
                        return;
                    }
                }
                else {
                    return;
                }
            }

            try {
                Log.Write(message, messageType);
            }
            catch { }

            // Отображает окно сообщения с заданным текстом если true
            if (showMessageBox) {
                MessageBoxIcon msgBoxIcon;
                string msgBoxTitle;
                switch (messageType) {
                    case MessageType.Debug:
                        msgBoxIcon = MessageBoxIcon.Information;
                        msgBoxTitle = "Отладка";
                        break;
                    case MessageType.Error:
                        msgBoxIcon = MessageBoxIcon.Error;
                        msgBoxTitle = "Ошибка";
                        break;
                    case MessageType.Information:
                        msgBoxIcon = MessageBoxIcon.Information;
                        msgBoxTitle = "Информация";
                        break;
                    case MessageType.Warning:
                        msgBoxIcon = MessageBoxIcon.Warning;
                        msgBoxTitle = "Внимание";
                        break;
                    case MessageType.None:
                    default:
                        msgBoxIcon = MessageBoxIcon.None;
                        msgBoxTitle = "Сообщение";
                        break;
                }
                MessageBox.Show(message, msgBoxTitle, MessageBoxButtons.OK, msgBoxIcon);
            }
        }
        #endregion

        /// <summary>
        /// Предварительная инициализация программы
        /// </summary>
        /// <returns></returns>
        public static bool PreInit() {
            try {
                Assembly execAssembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(execAssembly.Location);
                ProductName = fileVersionInfo.ProductName;
                CurrentDirectory = IOHelper.GetCurrentDir(execAssembly);
                Version = fileVersionInfo.FileVersion;

                Log = new Log(Path.Combine(CurrentDirectory, ProductName + ".log")) { InsertDate = true, AutoCompress = true };
                Log.ExceptionThrownEvent += (e) => MessageBox.Show(e.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Программе обмена не удается выполнить запуск. Обратитесь в ГИТ. Текст ошибки:\r\n" + ex.ToString(), MessageType.Error, true);
                return false;
            }
        }

        /// <summary>
        /// Инициализация конфигурации приложения
        /// </summary>
        /// <returns></returns>
        public static bool InitConfiguration() {
            try {
                string confPath = Path.Combine(CurrentDirectory, ProductName + ".conf");
                ConfHelper = new ConfHelper(confPath);
                if (!File.Exists(confPath)) {
                    Global defConf = new Global();
                    ConfHelper.SaveConfig(defConf, Encoding.UTF8, true);
                    CreateMessage("Файл конфигурации приложения не был обнаружен. Создан файл конфигурации с параметрами по умолчанию.", MessageType.Warning, true);
                }

                Configuration = ConfHelper.LoadConfig<Global>();
                if (!ConfHelper.Success) {
                    CreateMessage("Ошибка при загрузке конфигурации:\r\n" + ConfHelper.LastError.ToString(), MessageType.Error, true);
                    return false;
                }

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при загрузке конфигурации:\r\n" + ex.ToString(), MessageType.Error, true);
                return false;
            }
        }

        /// <summary>
        /// Инициализация движка программы
        /// </summary>
        /// <returns></returns>
        public static bool InitEngine(MainForm mainForm) {
            try {
                CreateMessage("Инициализация и запуск именованного канала...", MessageType.Debug);
                NamedPipeListener = new NamedPipeListener<string>(ProductName, System.Security.Principal.WellKnownSidType.AuthenticatedUserSid);
                NamedPipeListener.MessageReceived += delegate (object sender, NamedPipeListenerMessageReceivedEventArgs<string> e) {
                    switch (e.Message) {
                        case "maximize":
                            mainForm.InvokeIfRequired(() => mainForm.ShowWindow());
                            break;
                        case "complete":
                            mainForm.InvokeIfRequired(() => mainForm.ForceRunButton.Enabled = true);
                            break;
                        default:
                            break;
                    }
                };
                NamedPipeListener.Error += delegate (object sender, NamedPipeListenerErrorEventArgs e) {
                    CreateMessage(string.Format("Ошибка Pipe: ({0}): {1}", e.ErrorType, e.Exception.Message), MessageType.Error);
                };
                NamedPipeListener.Start();

                try {
                    CreateMessage("Добавление программы в автозагрузку...", MessageType.Debug);
                    #if !DEBUG
                        Feodosiya.Lib.App.AppHelper.AddToAutorun(Path.Combine(CurrentDirectory, ProductName + ".exe"));
                    #endif
                }
                catch (Exception ex) {
                    CreateMessage("Ошибка при добавление программы в автозагрузку:\r\n" + ex.ToString(), MessageType.Error);
                }

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при инициализации программы:\r\n" + ex.ToString(), MessageType.Error, true);
                return false;
            }
        }
    }
}
