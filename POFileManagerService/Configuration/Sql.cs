using Feodosiya.Lib.Security;
using System.Runtime.Serialization;


namespace POFileManagerService.Configuration {
    /// <summary>
    /// Параметры для работы с классом POFileManagerService.SQL.SQLHelper
    /// </summary>
    [DataContract]
    public class Sql {
        /// <summary>
        /// Имя пользователя сервера FB
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Пароль пользователя сервера FB
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Путь к файлу БД
        /// </summary>
        [DataMember]
        public string DataSource { get; set; }

        /// <summary>
        /// Хост на котором располагается БД
        /// </summary>
        [DataMember]
        public string Database { get; set; }

        /// <summary>
        /// Использовать пул соединений
        /// </summary>
        [DataMember]
        public bool Pooling { get; set; }

        /// <summary>
        /// Время жизни соединения
        /// </summary>
        [DataMember]
        public int ConnectionLifetime { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            if (Password != null && Password.Trim() != "") {
                try {
                    Password = Password.GetDecryptedString();
                }
                catch { }
            }
        }
    }
}
