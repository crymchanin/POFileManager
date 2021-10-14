using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace POFileManagerService.Configuration {
    [DataContract]
    public class Global {

        /// <summary>
        /// Параметры для работы с классом POFileManagerService.Net.FtpHelper
        /// </summary>
        [DataMember]
        public Ftp Ftp { get; set; }

        /// <summary>
        /// Параметры для работы с классом POFileManagerService.SQL.SQLHelper
        /// </summary>
        [DataMember]
        public Sql Sql { get; set; }

        /// <summary>
        /// Параметры для работы с классом POFileManagerService.Mail.MailHelper
        /// </summary>
        [DataMember]
        public Mail Mail { get; set; }

        /// <summary>
        /// Периодичность обработки файлов
        /// </summary>
        [DataMember]
        public int TaskInterval { get; set; }

        /// <summary>
        /// Дополнительное время при остановке службы (сек)
        /// </summary>
        [DataMember]
        public int AdditionalTime { get; set; }

        /// <summary>
        /// Параметры задачи обработки файлов
        /// </summary>
        [DataMember]
        public List<Task> Tasks { get; set; }

        /// <summary>
        /// Индекс отделения
        /// </summary>
        [DataMember]
        public int ZipCode { get; set; }

        /// <summary>
        /// Параметры системы автообновлений
        /// </summary>
        [DataMember]
        public Updates Updates { get; set; }

        /// <summary>
        /// Включение\отключение режима отладки
        /// </summary>
        [DataMember]
        public bool DebuggingEnabled { get; set; }

        /// <summary>
        /// Детальность отладки
        /// </summary>
        [DataMember]
        public int DebuggingLevel { get; set; }

        [OnSerializing()]
        internal void OnSerializing(StreamingContext context) {
            Ftp = (Ftp == null) ? new Ftp() {
                Cwd = "/",
                Host = "localhost",
                Port = 21,
                Username = "user",
                Password = "password"
            } : Ftp;
            Sql = (Sql == null) ? new Sql() {
                Database = "POFILEMANAGER.IB",
                DataSource = "localhost",
                Username = "SYSDBA",
                Password = "masterkey",
                Pooling = false,
                ConnectionLifetime = 20
            } : Sql;
            Mail = (Mail == null) ? new Mail() {
                Host = "cas.crimeanpost.ru",
                Domain = "crimeanpost",
                CertificateName = "CRIMEA-CA",
                Username = "username",
                Password = "Password",
                ToRecipient = "git.feo@crimeanpost.ru"
            } : Mail;
            Updates = (Updates == null) ? new Updates() {
                CheckUpdates = false,
                UpdaterServiceName = "POFileManagerUpdater"
            } : Updates;
            int minTaskInterval = 5;
            TaskInterval = (TaskInterval <= 0) ? minTaskInterval : TaskInterval;
            AdditionalTime = (AdditionalTime <= 0) ? 30 : Math.Max(30, AdditionalTime);
            Tasks = (Tasks == null) ? new List<Task>() : Tasks;
            ZipCode = (ZipCode == 0) ? 298100 : ZipCode;
            DebuggingLevel = (DebuggingLevel <= 0) ? 1 : DebuggingLevel;
        }
    }
}