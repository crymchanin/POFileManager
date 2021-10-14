using System.Runtime.Serialization;


namespace POFileManagerService.Configuration {
    /// <summary>
    /// Параметры для системы автообновлений    
    /// </summary>
    [DataContract]
    public class Updates {

        /// <summary>
        /// Имя службы автообновлений
        /// </summary>
        [DataMember]
        public string UpdaterServiceName { get; set; }

        /// <summary>
        /// Проверять наличие обновлений
        /// </summary>
        [DataMember]
        public bool CheckUpdates { get; set; }
    }
}
