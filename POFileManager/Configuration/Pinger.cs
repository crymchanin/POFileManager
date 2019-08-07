using System.Runtime.Serialization;


namespace POFileManager.Configuration {
    /// <summary>
    /// Содержит параметры для работы с классом POFileManager.Net.Pinger
    /// </summary>
    [DataContract]
    public class Pinger {
        /// <summary>
        /// Периодичность проверки доступности хоста в миллисекундах
        /// </summary>
        [DataMember]
        public int TimerInterval { get; set; }

        /// <summary>
        /// Максимальное время (после отправки сообщения проверки связи) ожидания сообщения ответа проверки связи ICMP в миллисекундах
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
