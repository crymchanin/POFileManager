using System.Runtime.Serialization;


namespace POFileManagerClient.Configuration {
    [DataContract]
    public class Global {

        /// <summary>
        /// Параметры для работы с классом POFileManagerClient.Pinger
        /// </summary>
        [DataMember]
        public Pinger Pinger { get; set; }

        /// <summary>
        /// Включение\отключение режима отладки
        /// </summary>
        [DataMember]
        public bool DebuggingEnabled { get; set; }

        /// <summary>
        /// Параметры для системы логирования    
        /// </summary>
        [DataMember]
        public Logs Logs { get; set; }

        [OnDeserializing()]
        internal void OnDeserializing(StreamingContext context) {
            Pinger = (Pinger == null) ? new Pinger() {
                HostIP = "8.8.8.8",
                PingTimeout = 1,
                TimerInterval = 5
            } : Pinger;
            Logs = (Logs == null) ? new Logs {
                MaxLogLength = 1024 * 500,
                CompressedLogsLifetime = -1
            } : Logs;
        }
    }
}
