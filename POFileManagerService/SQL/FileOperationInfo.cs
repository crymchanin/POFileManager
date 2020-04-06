using System;


namespace POFileManagerService.SQL {
    public class FileOperationInfo {
        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Имя задачи
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Дата операции
        /// </summary>
        public DateTime OperationDate { get; set; }
    }
}
