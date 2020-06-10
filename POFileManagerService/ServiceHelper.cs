#region Пространства имен
using Feodosiya.Lib.Conf;
using Feodosiya.Lib.IO;
using Feodosiya.Lib.IO.Pipes;
using Feodosiya.Lib.Logs;
using Feodosiya.Lib.Security;
using POFileManagerService.Configuration;
using POFileManagerService.Mail;
using POFileManagerService.Net;
using POFileManagerService.SQL;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
#endregion


namespace POFileManagerService {
    public static class ServiceHelper {

        #region Члены и свойства класса
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

        /// <summary>
        /// Отображает состояние процесса выполнения обработки файлов
        /// </summary>
        public static volatile bool IsRunning = false;


        /// <summary>
        /// Временная папка программы
        /// </summary>
        public static string TempPath { get; set; }

        /// <summary>
        /// Временная папка программы для хранения файлов
        /// </summary>
        public static string TempTasksPath { get; set; }

        /// <summary>
        /// Временная папка программы для архивов
        /// </summary>
        public static string TempFtpPath { get; set; }

        /// <summary>
        /// Временная папка программы для SQL скриптов
        /// </summary>
        public static string TempSqlPath { get; set; }

        /// <summary>
        /// Главный таймер программы
        /// </summary>
        public static Timer MainTimer { get; set; }

        /// <summary>
        /// Класс для работы с именованными каналами
        /// </summary>
        public static NamedPipeListener<string> NamedPipeListener { get; set; }

        /// <summary>
        /// Успешность выполнения инициализации приложения
        /// </summary>
        public static bool IsInitialized { get; set; } = false;

        /// <summary>
        /// Минимальный интервал основного таймера (в минутах)
        /// </summary>
        public const int MinimumMainTimerInterval = 5 * 60 * 1000;

        /// <summary>
        /// Класс для заимодействия с жураналами Windows
        /// </summary>
        private static EventLog MainEventLog { get; set; }
        #endregion


        #region Вспомогательные методы
        /// <summary>
        /// Создает сообщение, которое можно записать в лог, вывести в окне и отправить по почте
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="writeToEventLog">Записывает сообщение в журналы Windows</param>
        /// <param name="sendMail">Отправляет сообщение по почте</param>
        public static void CreateMessage(string message, MessageType messageType, bool writeToEventLog = false, bool sendMail = false) {
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
            catch (Exception ex) {
                WriteToWindowsJournal(ex.ToString(), EventLogEntryType.Error);
            }

            // Записывает заданный текст в журналы Windows если true
            if (writeToEventLog) {
                EventLogEntryType eventLogEntryType;
                switch (messageType) {
                    case MessageType.Error:
                        eventLogEntryType = EventLogEntryType.Error;
                        break;
                    case MessageType.Warning:
                        eventLogEntryType = EventLogEntryType.Warning;
                        break;
                    case MessageType.Debug:
                    case MessageType.Information:
                    case MessageType.None:
                    default:
                        eventLogEntryType = EventLogEntryType.Information;
                        break;
                }
                WriteToWindowsJournal(message, eventLogEntryType);
            }

            // Отправляет заданный текст на почту если true
            if (sendMail) {
                try {
                    MailHelper.SendMail(string.Format("Обмен файлами с отделением {0} PC: {1}", Configuration.ZipCode, Environment.MachineName), message);
                }
                catch (Exception ex) {
                    try {
                        string msg = "Ошибка при отправке сообщения на электронную почту:\r\n" + ex.ToString();
                        Log.Write(msg, MessageType.Error);
                        WriteToWindowsJournal(msg, EventLogEntryType.Error);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Вносит в журнал событий следующие записи с заданным текстом сообщения: ошибка, предупреждение, сведения, аудит отказов или аудит успехов
        /// </summary>
        /// <param name="message">Строка для записи в журнал событий</param>
        /// <param name="logEntryType">Одно из значений System.Diagnostics.EventLogEntryType</param>
        public static void WriteToWindowsJournal(string message, EventLogEntryType logEntryType = EventLogEntryType.Information) {
            try {
                MainEventLog.WriteEntry(message, logEntryType);
            }
            catch { }
        }
        #endregion

        /// <summary>
        /// Предварительная инициализация программы
        /// </summary>
        /// <returns></returns>
        public static bool PreInit(EventLog eventLog) {
            try {
                MainEventLog = eventLog;
                Assembly execAssembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(execAssembly.Location);
                ProductName = fileVersionInfo.ProductName;
                CurrentDirectory = IOHelper.GetCurrentDir(execAssembly);
                Version = fileVersionInfo.FileVersion;

                Log = new Log(Path.Combine(CurrentDirectory, ProductName + ".log")) { InsertDate = true, AutoCompress = true };
                Log.ExceptionThrownEvent += (e) => CreateMessage("Ошибка:\r\n" + e.ToString(), MessageType.Error, true);

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Программе обмена не удается выполнить запуск. Текст ошибки:\r\n" + ex.ToString(), MessageType.Error, true);
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
        public static bool InitEngine() {
            try {
                #region Проверка сертификата для отправки почты
                if (!SecurityHelper.CheckCertificateExists(StoreName.Root, StoreLocation.LocalMachine, Configuration.Mail.CertificateName)) {
                    CreateMessage("Сертификат почтового сервера не установлен. Необходимо выполнить установку сертификата и повторно запустить программу", MessageType.Error, true);
                    return false;
                }
                #endregion

                #region Инициализация параметров электронной почты
                MailHelper.MailConfiguration = Configuration.Mail;
                #endregion

                #region Инициализация параметров ftp сервера
                FtpHelper.FtpConfinguration = Configuration.Ftp;
                #endregion

                #region Инициализация параметров SQL
                if (!IOHelper.IsFullPath(Configuration.Sql.Database)) {
                    Configuration.Sql.Database = Path.Combine(CurrentDirectory, Configuration.Sql.Database);
                }
                SQLHelper.ConnectionString = string.Format("User={0};Password={1};Database={2};DataSource={3};Pooling={4};Connection lifetime={5};Charset=WIN1251;",
                    Configuration.Sql.Username, Configuration.Sql.Password,
                    Configuration.Sql.Database, Configuration.Sql.DataSource,
                    Configuration.Sql.Pooling.ToString().ToLower(), Configuration.Sql.ConnectionLifetime);
                #endregion

                #region Проверка и создание временных папок на их существование
                TempPath = Path.Combine(CurrentDirectory, "Temp");
                if (!Directory.Exists(TempPath)) {
                    Directory.CreateDirectory(TempPath);
                }
                TempTasksPath = Path.Combine(TempPath, "Tasks");
                if (!Directory.Exists(TempTasksPath)) {
                    Directory.CreateDirectory(TempTasksPath);
                }
                TempFtpPath = Path.Combine(TempPath, "Ftp");
                if (!Directory.Exists(TempFtpPath)) {
                    Directory.CreateDirectory(TempFtpPath);
                }
                TempSqlPath = Path.Combine(TempPath, "Sql");
                if (!Directory.Exists(TempSqlPath)) {
                    Directory.CreateDirectory(TempSqlPath);
                }
                #endregion

                #region Проверка задач по обработке файлов
                CreateMessage("Загрузка задач...", MessageType.Information, false);
                StringBuilder errors = new StringBuilder();
                foreach (Task task in Configuration.Tasks) {
                    task.Name = task.Name.ToUpper();
                    task.Source = Environment.ExpandEnvironmentVariables(task.Source);
                    if (!string.IsNullOrWhiteSpace(task.ExternalLib)) {
                        task.ExternalLibAsm = Assembly.LoadFile(Path.Combine(CurrentDirectory, Environment.ExpandEnvironmentVariables(task.ExternalLib)));
                    }
                    if (!task.AllowDuplicate) {
                        try {
                            // Проверка существования таблицы для хранения хеш сумм файлов
                            SQLHelper.CreateTableFingerprint(task.Name);
                        }
                        catch (Exception ex) {
                            errors.AppendLine(string.Format("Ошибка: {0}", ex.ToString()));
                        }
                    }

                    try {
                        IOHelper.IsPathDirectory(task.Source);
                    }
                    catch (Exception ex) {
                        if (ex is FileNotFoundException || ex is DirectoryNotFoundException) {
                            CreateMessage(string.Format("Путь '{0}' для задачи '{1}' не существует", task.Source, task.Name), MessageType.Warning);
                            continue;
                        }

                        throw;
                    }
                }
                if (errors.Length > 0) {
                    CreateMessage("Ошибка при инициализации задач:\r\n" + errors.ToString(), MessageType.Error, true, true);
                    //return false;
                }
                #endregion

                #region Проверка существования таблицы для хранения данных об обработанных файлах
                try {
                    SQLHelper.CreateTableOperationInfo();
                }
                catch (Exception ex) {
                    CreateMessage("Ошибка при инициализации программы:\r\n" + ex.ToString(), MessageType.Error, true, true);
                    //return false;
                }
                #endregion

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка инициализации программы:\r\n" + ex.ToString(), MessageType.Error, true, true);
                return false;
            }
        }
    }
}
