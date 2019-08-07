using System.Collections.Generic;
using System.Runtime.Serialization;


namespace POFileManager.Configuration {
    [DataContract]
    public class Global {

        /// <summary>
        /// Параметры для подключения к серверу обновлений
        /// </summary>
        [DataMember]
        public Updates Updates { get; set; }

        /// <summary>
        /// Параметры для работы с классом POFileManager.Net.FtpHelper
        /// </summary>
        [DataMember]
        public Ftp Ftp { get; set; }

        /// <summary>
        /// Параметры для работы с классом POFileManager.SQL.SQLHelper
        /// </summary>
        [DataMember]
        public Sql Sql { get; set; }

        /// <summary>
        /// Параметры для работы с классом POFileManager.Mail.MailHelper
        /// </summary>
        [DataMember]
        public Mail Mail { get; set; }

        /// <summary>
        /// Периодичность обработки файлов
        /// </summary>
        [DataMember]
        public int TaskInterval { get; set; }

        /// <summary>
        /// Параметры задачи обработки файлов
        /// </summary>
        [DataMember]
        public List<Task> Tasks { get; set; }

        /// <summary>
        /// Параметры для работы с классом POFileManager.Net.Pinger
        /// </summary>
        [DataMember]
        public Pinger Pinger { get; set; }

        /// <summary>
        /// Индекс отделения
        /// </summary>
        [DataMember]
        public int ZipCode { get; set; }

        /// <summary>
        /// Включение\отключение режима отладки
        /// </summary>
        [DataMember]
        public bool DebuggingEnabled { get; set; }
    }
}
