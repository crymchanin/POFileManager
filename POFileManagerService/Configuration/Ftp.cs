using System.Runtime.Serialization;


namespace POFileManagerService.Configuration {
    /// <summary>
    /// Параметры для работы с классом POFileManagerService.Net.FtpHelper
    /// </summary>
    [DataContract]
    public class Ftp {

        /// <summary>
        /// Имя пользователя отправителя на ftp сервере
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Пароль пользователя отправителя на ftp сервере
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Адрес ftp сервера
        /// </summary>
        [DataMember]
        public string Host { get; set; }

        /// <summary>
        /// Текущий каталог ftp сервера
        /// </summary>
        [DataMember]
        public string Cwd { get; set; }

        /// <summary>
        /// Порт для подключения к ftp серверу
        /// </summary>
        [DataMember]
        public int Port { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            Cwd = (Cwd == null) ? string.Empty : Cwd;
            Cwd = Cwd.EndsWith("/") ? Cwd : (Cwd + "/");
        }
    }
}
