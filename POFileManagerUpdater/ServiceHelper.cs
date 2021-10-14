using POFileManagerUpdater.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;


namespace POFileManagerUpdater {
    public static class ServiceHelper {

        #region Члены и свойства класса
        /// <summary>
        /// Текущая рабочая папка
        /// </summary>
        public static string CurrentDirectory { get; set; }

        /// <summary>
        /// Имя данного продукта
        /// </summary>
        public static string UpdaterProductName { get; set; }

        /// <summary>
        /// Версия данного приложения
        /// </summary>
        public static string Version { get; set; }

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
        /// Главный таймер программы
        /// </summary>
        public static Timer MainTimer { get; set; }

        /// <summary>
        /// Минимальный интервал основного таймера (в минутах)
        /// </summary>
        public const int MinimumMainTimerInterval = 5 * 60 * 1000;

        /// <summary>
        /// Успешность выполнения инициализации приложения
        /// </summary>
        public static bool IsInitialized { get; set; } = false;

        /// <summary>
        /// Класс для заимодействия с жураналами Windows
        /// </summary>
        private static EventLog MainEventLog { get; set; }
        #endregion

        #region Вспомогательные методы
        /// <summary>
        /// Представляет перечисление возможных типов выводимых сообщений
        /// </summary>
        public enum MessageType {
            /// <summary>
            /// Обычный
            /// </summary>
            None,
            /// <summary>
            /// Информация
            /// </summary>
            Information,
            /// <summary>
            /// Предупреждение
            /// </summary>
            Warning,
            /// <summary>
            /// Ошибка
            /// </summary>
            Error,
            /// <summary>
            /// Отладка
            /// </summary>
            Debug
        }

        /// <summary>
        /// Создает сообщение, которое можно записать в журнал
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="messageType">Тип сообщения</param>
        public static void CreateMessage(string message, MessageType messageType) {
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

        /// <summary>
        /// Возвращает путь к текущей рабочей папке
        /// </summary>
        /// <param name="assembly">Сборка, папку которой необходимо получить</param>
        /// <returns></returns>
        public static string GetCurrentDir(Assembly assembly) {
            return Path.GetDirectoryName(assembly.Location);
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
                UpdaterProductName = fileVersionInfo.ProductName;
                CurrentDirectory = GetCurrentDir(execAssembly);
                Version = fileVersionInfo.FileVersion;

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Программе обновлений не удается выполнить запуск. Текст ошибки:\r\n" + ex.ToString(), MessageType.Error);
                return false;
            }
        }

        /// <summary>
        /// Инициализация конфигурации приложения
        /// </summary>
        /// <returns></returns>
        public static bool InitConfiguration() {
            try {
                string confPath = Path.Combine(CurrentDirectory, UpdaterProductName + ".conf");
                ConfHelper = new ConfHelper(confPath);
                if (!File.Exists(confPath)) {
                    Global defConf = new Global();
                    ConfHelper.SaveConfig(defConf, Encoding.UTF8, true);
                    CreateMessage("Файл конфигурации приложения не был обнаружен. Создан файл конфигурации с параметрами по умолчанию.", MessageType.Warning);
                }

                Configuration = ConfHelper.LoadConfig<Global>();
                if (!ConfHelper.Success) {
                    CreateMessage("Ошибка при загрузке конфигурации:\r\n" + ConfHelper.LastError.ToString(), MessageType.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex) {
                CreateMessage("Ошибка при загрузке конфигурации:\r\n" + ex.ToString(), MessageType.Error);
                return false;
            }
        }
    }
}
