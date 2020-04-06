using System.Runtime.Serialization;


namespace POFileManagerService.Configuration {
    /// <summary>
    /// Содержит параметры для отправки почты
    /// </summary>
    [DataContract]
    public class Mail {
        /// <summary>
        /// Имя пользователя отправителя на почтовом сервере
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Пароль пользователя отправителя на почтовом сервере
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Домен связанный с учетными данными отправителя
        /// </summary>
        [DataMember]
        public string Domain { get; set; }

        /// <summary>
        /// Адрес почтового сервера
        /// </summary>
        [DataMember]
        public string Host { get; set; }

        /// <summary>
        /// E-mail получателя
        /// </summary>
        [DataMember]
        public string ToRecipient { get; set; }

        /// <summary>
        /// Имя сертификата почтового сервера
        /// </summary>
        [DataMember]
        public string CertificateName { get; set; }
    }
}
