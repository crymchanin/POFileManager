#region Пространства имен
using Feodosiya.Lib.Conf;
using Feodosiya.Lib.IO;
using Feodosiya.Lib.IO.Pipes;
using Feodosiya.Lib.Logs;
using Feodosiya.Lib.Math;
using Feodosiya.Lib.Threading;
using POFileManagerClient.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        /// Папка с логами программы
        /// </summary>
        public static string LogsPath { get; set; }

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
        /// Создает сообщение, которое можно записать в лог, вывести в окне
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

                LogsPath = Path.Combine(CurrentDirectory, "Logs");
                if (!Directory.Exists(LogsPath)) {
                    Directory.CreateDirectory(LogsPath);
                }
                Log = new Log(Path.Combine(LogsPath, ProductName + ".log")) { InsertDate = true, AutoCompress = true };
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

                Log.MaxLogLength = Configuration.Logs.MaxLogLength;

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

                #region Удаление сжатых логов по сроку давности
                try {
                    if (Configuration.Logs.CompressedLogsLifetime > -1) {
                        foreach (string file in Directory.GetFiles(LogsPath, "*.gz", SearchOption.TopDirectoryOnly)) {
                            if (!Regex.IsMatch(Path.GetFileName(file), $"^{ProductName}" + @".log_\d{2}_\d{2}_\d{4}_\d{2}_\d{2}_\d{2}\.gz$")) {
                                continue;
                            }

                            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                            date = date.AddDays(Configuration.Logs.CompressedLogsLifetime.ToNegative());
                            FileInfo info = new FileInfo(file);
                            if (info.LastWriteTime > date) {
                                continue;
                            }

                            File.Delete(file);
                            CreateMessage(string.Format("Архив логов {0} был удален из за сроков его давности: {1} дней", file, Configuration.Logs.CompressedLogsLifetime), MessageType.Debug);
                        }
                    }
                }
                catch (Exception ex) {
                    CreateMessage("Ошибка при удалении архивов с логами: " + ex.ToString(), MessageType.Error);
                }
                #endregion

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при инициализации программы:\r\n" + ex.ToString(), MessageType.Error, true);
                return false;
            }
        }
    }
}
