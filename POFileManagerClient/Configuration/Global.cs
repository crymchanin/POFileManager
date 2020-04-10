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

        [OnSerializing()]
        internal void OnSerializing(StreamingContext context) {
            Pinger = (Pinger == null) ? new Pinger() {
                HostIP = "8.8.8.8",
                PingTimeout = 1,
                TimerInterval = 5
            } : Pinger;
        }
    }
}
