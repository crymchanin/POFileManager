#region Пространства имен
using Feodosiya.Lib.Conf;
using Feodosiya.Lib.IO;
using Feodosiya.Lib.Logs;
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
using System.Security.Principal;
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
        /// Значение true данного свойства означает что программа была запущена после установки обновления
        /// </summary>
        public static bool UpdatesInstalled { get; set; } = false;

        /// <summary>
        /// Основной таймер программы
        /// </summary>
        public static Timer MainTimer { get; set; }

        /// <summary>
        /// Минимальный интервал основного таймера
        /// </summary>
        private const int MinimumMainTimerInterval = 30000;
        #endregion

        #region Вспомогательные методы
        /// <summary>
        /// Создает сообщение, которое можно записать в лог, вывести в окне и отправить по почте
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="showMessageBox">Отображает окно сообщения если установлено в true</param>
        /// <param name="sendMail">Отправляет сообщение по почте</param>
        public static void CreateMessage(string message, MessageType messageType, bool showMessageBox = false, bool sendMail = false) {
            if (messageType == MessageType.Debug) {
                if (Configuration != null) {
                    if (!Configuration.DebuggingEnabled) {
                        return;
                    }
                }
            }
            Log.Write(message, messageType);

            if (sendMail) {
                try {
                    MailHelper.SendMail("Обмен файлами с отделением " + Configuration.ZipCode.ToString(), message);
                }
                catch (Exception ex) {
                    Log.Write("Ошибка при отправке сообщения на электронную почту:\r\n" + ex.ToString(), MessageType.Error);
                }
            }

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
        /// Проверяет наличие установленного сертификата для электронной почты в системе
        /// </summary>
        /// <returns></returns>
        public static bool CheckCertificateExists(string cn) {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectName, cn, false);

            return (certificates != null && certificates.Count > 0);
        }

        /// <summary>
        /// Выполняет проверку получения повышенных привилегий программой
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator() {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent()) {
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        #endregion

        /// <summary>
        /// Инициализация конфигурации приложения
        /// </summary>
        /// <returns></returns>
        public static bool InitConfiguration() {
            try {
                string confPath = Path.Combine(Program.CurrentDirectory, "settings.conf");
                if (!File.Exists(confPath)) {
                    File.WriteAllBytes(confPath, Properties.Resources.settings);
                    CreateMessage("Файл конфигурации приложения не был обнаружен. Создан файл конфигурации с параметрами по умолчанию.", MessageType.Warning, true, false);
                }
                ConfHelper = new ConfHelper(confPath);
                Configuration = ConfHelper.LoadConfig<Global>();
                if (!ConfHelper.Success) {
                    CreateMessage("Ошибка при загрузке конфигурации:\r\n" + ConfHelper.LastError.ToString(), MessageType.Error, true, false);
                    return false;
                }

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при загрузке конфигурации:\r\n" + ex.ToString(), MessageType.Error, true, false);
                return false;
            }
        }

        /// <summary>
        /// Инициализация движка программы
        /// </summary>
        /// <returns></returns>
        public static bool InitEngine() {
            try {
                // Установка модуля обновления при его отсутствии
                string updaterPath = Path.Combine(Program.CurrentDirectory, "Updater.exe");
                if (!File.Exists(updaterPath)) {
                    File.WriteAllBytes(updaterPath, Properties.Resources.Updater);
                }

                if (!CheckCertificateExists(Configuration.Mail.CertificateName)) {
                    CreateMessage("Сертификат почтового сервера не установлен. Необходимо выполнить установку сертификата и повторно запустить программу", MessageType.Error, true, false);
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
                    AppHelper.Configuration.Sql.Database = Path.Combine(Program.CurrentDirectory, Configuration.Sql.Database);
                }
                SQLHelper.ConnectionString = string.Format("User={0};Password={1};Database={2};DataSource={3};Pooling=false;Connection lifetime=60;Charset=WIN1251;",
                    Configuration.Sql.Username, Configuration.Sql.Password,
                    Configuration.Sql.Database, Configuration.Sql.DataSource);

#if !DEBUG
                // Добавление программы в автозагрузку
                //Feodosiya.Lib.App.AppHelper.AddToAutorun(Program.ApplicationPath);
#endif

                // Проверка временных папок на их существование
                TempPath = Path.Combine(Program.CurrentDirectory, "Temp");
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

                if (UpdatesInstalled) {
                    CreateMessage("Обновление успешно установлено", MessageType.Information, false, true);
                }

                // Проверка задач по обработке файлов
                StringBuilder errors = new StringBuilder();
                foreach (Task task in Configuration.Tasks) {
                    task.Name = task.Name.ToUpper();
                    if (!string.IsNullOrWhiteSpace(task.ExternalLib)) {
                        task.ExternalLibAsm = Assembly.LoadFile(Path.Combine(Program.CurrentDirectory, task.ExternalLib));
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
                    CreateMessage("Ошибка при инициализации задач:\r\n" + errors.ToString(), MessageType.Error, true, true);
                }

                // Проверка существования таблицы для хранения данных об обработанных файлах
                SQLHelper.CreateTableOperationInfo();

                // Настройка главного таймера
                int interval = Configuration.TaskInterval;
                MainTimer = new Timer();
                MainTimer.Interval = Math.Max(MinimumMainTimerInterval, interval);

                NamedPipeListener<string> namedPipeListener = new NamedPipeListener<string>(Program.ProductName);
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
                    CreateMessage(string.Format("Ошибка Pipe: ({0}): {1}", e.ErrorType, e.Exception.Message), MessageType.Error, false, false);
                };
                namedPipeListener.Start();

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка инициализации программы:\r\n" + ex.ToString(), MessageType.Error, true, true);
                return false;
            }
        }

        /// <summary>
        /// Выролняет проверку наличия обновлений
        /// </summary>
        /// <returns></returns>
        public static bool CheckUpdates() {
            try {
                try {
                    if (Program.ARGS.Length > 0) {
                        foreach (string arg in Program.ARGS) {
                            if (arg.ToLower() == "-u") {
                                Process[] processes = Process.GetProcessesByName("Updater");
                                if (processes.Length > 0) {
                                    Process proc = processes[0];
                                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(proc.MainModule.FileName);
                                    if (info.CompanyName == "ФГУП Почта Крыма") {
                                        proc.Kill();
                                    }
                                }

                                string path = Path.Combine(Program.CurrentDirectory, "Updater.exe");
                                if (File.Exists(path)) {
                                    File.Delete(path);
                                }
                                File.WriteAllBytes(path, Properties.Resources.Updater);

                                UpdatesInstalled = true;
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    CreateMessage("Ошибка при установке обновлений: " + ex.ToString(), MessageType.Error, true, false);
                }

                if (UpdatesHelper.CheckUpdates(Configuration.Updates.ServerName, Program.Version)) {
                    Process.Start(Path.Combine(Program.CurrentDirectory, "Updater.exe"), string.Format("{0} {1} {2}", Program.Version, Configuration.Updates.ServerName, Program.ProductName));
                    return true;
                }

                return false;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при проверке обновлений: " + ex.ToString(), MessageType.Error, false, false);
                return false;
            }
        }
    }
}