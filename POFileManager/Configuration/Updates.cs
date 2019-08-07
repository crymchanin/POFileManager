using System.Runtime.Serialization;


namespace POFileManager.Configuration {
    /// <summary>
    /// Содержит параметры для подключения к серверу обновлений
    /// </summary>
    [DataContract]
    public class Updates {
        /// <summary>
        /// Сервер обновлений
        /// </summary>
        [DataMember]
        public string ServerName { get; set; }
    }
}
