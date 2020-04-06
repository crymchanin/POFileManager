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

    }
}
