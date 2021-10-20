using System.Runtime.Serialization;

namespace POFileManagerClient.Configuration {
    /// <summary>
    /// Параметры для системы логирования    
    /// </summary>
    [DataContract]
    public class Logs {

        /// <summary>
        /// Максимальный размер лога (в байтах)
        /// </summary>
        [DataMember]
        public long MaxLogLength { get; set; }

        /// <summary>
        /// Время хранения сжатых логов (в днях)
        /// </summary>
        [DataMember]
        public int CompressedLogsLifetime { get; set; }
    }
}