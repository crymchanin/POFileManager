using System.Runtime.Serialization;


namespace POFileManagerClient.Configuration {
    /// <summary>
    /// Содержит параметры для работы с классом POFileManagerClient.Net.Pinger
    /// </summary>
    [DataContract]
    public class Pinger {
        /// <summary>
        /// Периодичность проверки доступности хоста в секундах
        /// </summary>
        [DataMember]
        public int TimerInterval { get; set; }

        /// <summary>
        /// Максимальное время (после отправки сообщения проверки связи) ожидания сообщения ответа проверки связи ICMP в секундах
        /// </summary>
        [DataMember]
        public int PingTimeout { get; set; }

        /// <summary>
        /// Проверяемый хост
        /// </summary>
        [DataMember]
        public string HostIP { get; set; }
    }
}
