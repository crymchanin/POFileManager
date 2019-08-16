#region Пространства имен
using Feodosiya.Lib.Conf;
using Feodosiya.Lib.IO;
using Feodosiya.Lib.Logs;
using Feodosiya.Lib.Security;
using POFileManager.Configuration;
using POFileManager.Mail;
using POFileManager.Net;
using POFileManager.SQL;
using POFileManager.Updates;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
#endregion


namespace POFileManager {
    /// <summary>
    /// Вспомогательные методы и свойства
    /// </summary>
    public static class AppHelper {

        #region Члены и свойства класса
        /// <summary>
        /// Аргументы программы
        /// </summary>
        public static string[] ARGS { get; set; }

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
        /// Основной таймер программы
        /// </summary>
        public static Timer MainTimer { get; set; }

        /// <summary>
        /// Таймер для проверки и установки обновлений
        /// </summary>
        public static System.Threading.Timer UpdateTimer { get; private set; }

        /// <summary>
        /// Отображает состояние процесса обновления приложения
        /// </summary>
        public static volatile bool IsUpdateRunning = false;

        /// <summary>
        /// Значение true данного свойства означает что программа была запущена после установки обновления
        /// </summary>
        public static bool UpdatesInstalled { get; set; } = false;

        /// <summary>
        /// Минимальный интервал таймера обновлений
        /// </summary>
        private const int MinimumUpdateTimerInterval = 600000;

        /// <summary>
        /// Минимальный интервал основного таймера
        /// </summary>
        private const int MinimumMainTimerInterval = 30000;

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
        /// <param name="showMessageBox">Отображает окно сообщения если установлено в true</param>
        /// <param name="sendMail">Отправляет сообщение по почте</param>
        /// <param name="writeToEventLog">Записывает сообщение в журналы Windows</param>
        public static void CreateMessage(string message, MessageType messageType, bool showMessageBox = false, bool sendMail = false, bool writeToEventLog = false) {
            if (messageType == MessageType.Debug) {
                if (Configuration != null) {
                    if (!Configuration.DebuggingEnabled) {
                        return;
                    }
                }
            }
            Log.Write(message, messageType);

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
                    string msg = "Ошибка при отправке сообщения на электронную почту:\r\n" + ex.ToString();
                    Log.Write(msg, MessageType.Error);
                    WriteToWindowsJournal(msg, EventLogEntryType.Error);
                }
            }

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
        public static bool PreInit() {
            try {
                Assembly execAssembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(execAssembly.Location);
                ProductName = fileVersionInfo.ProductName;
                CurrentDirectory = IOHelper.GetCurrentDir(execAssembly);
                Version = fileVersionInfo.FileVersion;

                ARGS = Environment.GetCommandLineArgs();

                Log = new Log(Path.Combine(CurrentDirectory, ProductName + ".log")) { InsertDate = true, AutoCompress = true };
                Log.ExceptionThrownEvent += (e) => MessageBox.Show(e.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Если не админ, то в журналы писать не сможем в связи с отсутствием прав
                if (SecurityHelper.IsAdministrator()) {
                    // Инициализация класса для взаимодействия с журналами Windows
                    MainEventLog = new EventLog();
                    MainEventLog.BeginInit();
                    MainEventLog.Log = "Application";
                    MainEventLog.Source = ProductName;
                    MainEventLog.EndInit();
                }

                return true;
            }
            catch (Exception ex) {
                MessageBox.Show("Программе обмена не удается выполнить запуск. Обратитесь в ГИТ. Текст ошибки:\r\n" + ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Инициализация конфигурации приложения
        /// </summary>
        /// <returns></returns>
        public static bool InitConfiguration() {
            try {
                string confPath = Path.Combine(CurrentDirectory, "settings.conf");
                if (!File.Exists(confPath)) {
                    File.WriteAllBytes(confPath, Properties.Resources.settings);
                    CreateMessage("Файл конфигурации приложения не был обнаружен. Создан файл конфигурации с параметрами по умолчанию.", MessageType.Warning, true, false, true);
                }
                ConfHelper = new ConfHelper(confPath);
                Configuration = ConfHelper.LoadConfig<Global>();
                if (!ConfHelper.Success) {
                    CreateMessage("Ошибка при загрузке конфигурации:\r\n" + ConfHelper.LastError.ToString(), MessageType.Error, true, false, true);
                    return false;
                }

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при загрузке конфигурации:\r\n" + ex.ToString(), MessageType.Error, true, false, true);
                return false;
            }
        }

        /// <summary>
        /// Инициализация движка программы
        /// </summary>
        /// <returns></returns>
        public static bool InitEngine() {
            try {

                if (!SecurityHelper.CheckCertificateExists(StoreName.Root, StoreLocation.CurrentUser, Configuration.Mail.CertificateName)) {
                    CreateMessage("Сертификат почтового сервера не установлен. Необходимо выполнить установку сертификата и повторно запустить программу", MessageType.Error, true, false, true);
                    return false;
                }

                // Инициализация параметров электронной почты
                MailHelper.Host = Configuration.Mail.Host;
                MailHelper.Domain = Configuration.Mail.Domain;
                MailHelper.ToRecipient = Configuration.Mail.ToRecipient;
                MailHelper.Username = Configuration.Mail.Username;
                MailHelper.Password = Configuration.Mail.Password;

                // Инициализация параметров ftp сервера
                FtpHelper.Host = Configuration.Ftp.Host;
                FtpHelper.Port = Configuration.Ftp.Port;
                FtpHelper.Cwd = Configuration.Ftp.Cwd;
                FtpHelper.Username = Configuration.Ftp.Username;
                FtpHelper.Password = Configuration.Ftp.Password;

                // Инициализация параметров SQL
                if (!IOHelper.IsFullPath(Configuration.Sql.Database)) {
                    Configuration.Sql.Database = Path.Combine(CurrentDirectory, Configuration.Sql.Database);
                }
                SQLHelper.ConnectionString = string.Format("User={0};Password={1};Database={2};DataSource={3};Pooling=false;Connection lifetime=60;Charset=WIN1251;",
                    Configuration.Sql.Username, Configuration.Sql.Password,
                    Configuration.Sql.Database, Configuration.Sql.DataSource);

                // Проверка временных папок на их существование
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

                // Проверка задач по обработке файлов
                CreateMessage("Загрузка задач...", MessageType.Information, false, false, true);
                StringBuilder errors = new StringBuilder();
                foreach (Task task in Configuration.Tasks) {
                    task.Name = task.Name.ToUpper();
                    if (!string.IsNullOrWhiteSpace(task.ExternalLib)) {
                        task.ExternalLibAsm = Assembly.LoadFile(Path.Combine(CurrentDirectory, task.ExternalLib));
                    }
                    if (!task.AllowDuplicate) {
                        SQLHelper.CreateTableFingerprint(task.Name);
                    }

                    try {
                        IOHelper.IsPathDirectory(task.Source);
                    }
                    catch (Exception ex) {
                        if (ex is FileNotFoundException || ex is DirectoryNotFoundException) {
                            errors.AppendLine(string.Format("Путь '{0}' для задачи '{1}' не существует", task.Source, task.Name));
                            continue;
                        }

                        throw;
                    }
                }
                if (errors.Length > 0) {
                    CreateMessage("Ошибка при инициализации задач:\r\n" + errors.ToString(), MessageType.Error, false, true, true);
                }

                // Проверка существования таблицы для хранения данных об обработанных файлах
                SQLHelper.CreateTableOperationInfo();

                // Настройка главного таймера
                int interval = Configuration.TaskInterval;
                MainTimer = new Timer();
                MainTimer.Interval = Math.Max(MinimumMainTimerInterval, interval);

                // Настройка таймера обновлений
                UpdateTimer = new System.Threading.Timer((s) => CheckUpdates(), null, 0, Math.Max(MinimumUpdateTimerInterval, Configuration.CheckUpdateInterval));

                CreateMessage("Инициализация и запуск именованного канала...", MessageType.Information, false, false, true);
                NamedPipeListener<string> namedPipeListener = new NamedPipeListener<string>(ProductName);
                namedPipeListener.MessageReceived += delegate (object sender, NamedPipeListenerMessageReceivedEventArgs<string> e) {
                    switch (e.Message) {
                        case "force":
                            TasksHelper.RunTasksThread();
                            break;
                        default:
                            break;
                    }
                };
                namedPipeListener.Error += delegate (object sender, NamedPipeListenerErrorEventArgs e) {
                    CreateMessage(string.Format("Ошибка Pipe: ({0}): {1}", e.ErrorType, e.Exception.Message), MessageType.Error, false, false, true);
                };
                namedPipeListener.Start();

                CreateMessage("Отправка информации о текущей версии программы на сервер обновлений...", MessageType.Information, false, false, true);
                System.Threading.Thread thread = new System.Threading.Thread(delegate() {
                    try {
                        string errorString;
                        if (!UpdatesHelper.SendVersionInformation(Configuration.Updates.ServerName, Configuration.ZipCode, ProductName, Version, out errorString)) {
                            throw new Exception(errorString);
                        }
                    }
                    catch (Exception ex) {
                        CreateMessage("Ошибка при отправке информации о текущей версии программы:\r\n" + ex.ToString(), MessageType.Error, false, false, true);
                    }
                });
                thread.Start();

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка инициализации программы:\r\n" + ex.ToString(), MessageType.Error, true, true, true);
                return false;
            }
        }

        /// <summary>
        /// Выролняет проверку наличия обновлений
        /// </summary>
        /// <returns></returns>
        public static void CheckUpdates() {
            if (IsUpdateRunning) {
                return;
            }
            IsUpdateRunning = true;

            try {
                if (ARGS.Length > 0) {
                    foreach (string arg in ARGS) {
                        if (arg.ToLower() == "-u" && !UpdatesInstalled) {
                            Process[] processes = Process.GetProcessesByName("Updater");
                            if (processes.Length > 0) {
                                Process proc = processes[0];
                                FileVersionInfo info = FileVersionInfo.GetVersionInfo(proc.MainModule.FileName);
                                if (info.CompanyName == "ФГУП Почта Крыма") {
                                    proc.Kill();
                                }
                            }

                            string path = Path.Combine(CurrentDirectory, "Updater.exe");
                            if (File.Exists(path)) {
                                File.Delete(path);
                            }
                            File.WriteAllBytes(path, Properties.Resources.Updater);

                            // Создаем сообщение об успешной установке обновления
                            CreateMessage("Обновление успешно установлено", MessageType.Information, false, true, true);

                            UpdatesInstalled = true;
                        }
                    }
                }
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при установке обновлений: " + ex.ToString(), MessageType.Error, true, false, true);
            }

            try {
                // Установка модуля обновления при его отсутствии
                string updaterPath = Path.Combine(CurrentDirectory, "Updater.exe");
                if (!File.Exists(updaterPath)) {
                    File.WriteAllBytes(updaterPath, Properties.Resources.Updater);
                }
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при распаковке клиента обновлений: " + ex.ToString(), MessageType.Error, false, false, false);
            }

            try {
                CreateMessage("Проверка обновлений конфигурационного файла...", MessageType.Information, false, false, true);
                if (UpdatesHelper.CheckConfigUpdates(Path.Combine(CurrentDirectory, "settings.upd"), Configuration)) {
                    // Сохраняем изменения в конфигурационный файл
                    ConfHelper.SaveConfig(Configuration, Encoding.UTF8, true);
                    Configuration = ConfHelper.LoadConfig<Global>();
                    CreateMessage("Выполнено обновление конфигурационного файла", MessageType.Information, false, false, true);
                }
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при установке обновлений конфигурационного файла: " + ex.ToString(), MessageType.Error, false, false, true);
            }

            try {
                CreateMessage("Проверка обновлений...", MessageType.Information, false, false, true);
                if (UpdatesHelper.CheckUpdates(Configuration.Updates.ServerName, Version, ProductName)) {
                    Process.Start(Path.Combine(CurrentDirectory, "Updater.exe"), string.Format("{0} {1} {2}", Version, Configuration.Updates.ServerName, ProductName));
                    GUIController.ExitOnLoaded();
                    return;
                }
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при проверке обновлений: " + ex.ToString(), MessageType.Error, false, false, true);
            }

            IsUpdateRunning = false;
        }
    }
}