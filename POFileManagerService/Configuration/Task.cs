using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;


namespace POFileManagerService.Configuration {
    /// <summary>
    /// Содержит параметры задачи обработки файлов
    /// </summary>
    [DataContract]
    public class Task {
        /// <summary>
        /// Имя задачи
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Путь к файлу или папке для их обработки
        /// </summary>
        [DataMember]
        public string Source { get; set; }

        /// <summary>
        /// Регулярное выражение, в соответствии с которым будет выполнена обработка файлов
        /// </summary>
        [DataMember]
        public string Regex { get; set; }

        /// <summary>
        /// Выполнять обработку файлов в папке рекурсивно
        /// </summary>
        [DataMember]
        public bool Recursive { get; set; }

        /// <summary>
        /// Перемещать файл из исходного расположения
        /// </summary>
        [DataMember]
        public bool MoveFile { get; set; }

        /// <summary>
        /// Интервал в днях, за который будет выполнена обработка файлов
        /// </summary>
        [DataMember]
        public int DayInterval { get; set; }

        /// <summary>
        /// Разрешить повторную отправку файлов
        /// </summary>
        [DataMember]
        public bool AllowDuplicate { get; set; }

        /// <summary>
        /// Внешняя библиотека для дополнительных условий обработки файлов
        /// </summary>
        [DataMember]
        public string ExternalLib { get; set; }

        /// <summary>
        /// Внешняя библиотека загруженная в домен
        /// </summary>
        public Assembly ExternalLibAsm { get; set; }

        /// <summary>
        /// Параметры для внешней библиотеки
        /// </summary>
        [DataMember]
        public Dictionary<string, object> ExternalLibParams { get; set; }
    }
}
