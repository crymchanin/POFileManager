using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace POFileManagerService {
    /// <summary>
    /// Метаданные    
    /// </summary>
    [DataContract]
    public class PackageMeta {

        /// <summary>
        /// Имя компьютера
        /// </summary>
        [DataMember]
        public string MachineName { get; set; }

        /// <summary>
        /// Имя пользователя, сеанс которого активен в данный момент
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// Индекс отделения
        /// </summary>
        [DataMember]
        public int ZipCode { get; set; }

        /// <summary>
        /// Имя пакета
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Дата формирования пакета
        /// </summary>
        [DataMember]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Список файлов пакета
        /// </summary>
        [DataMember]
        public List<string> FilesList { get; set; }

        /// <summary>
        /// Количество файлов в пакете
        /// </summary>
        [DataMember]
        public int FilesCount { get; set; }
    }
}