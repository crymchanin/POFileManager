#region Пространства имен
using Feodosiya.Lib.Security;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text.RegularExpressions;
#endregion


namespace POFileManagerService.SQL {
    public static class SQLHelper {

        #region Члены и свойства класса
        /// <summary>
        /// Строка подключения к БД программы
        /// </summary>
        public static string ConnectionString { get; set; }
        #endregion

        /// <summary>
        /// Запуск службы Firebird
        /// </summary>
        /// <returns></returns>
        public static bool StartFirebirdService() {
            ServiceController sc = new ServiceController();
            sc.ServiceName = "FirebirdServerDefaultInstance";
            bool flag = false;

            switch (sc.Status) {
                case ServiceControllerStatus.Stopped:
                case ServiceControllerStatus.Paused:
                    try {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running);

                        flag = true;
                    }
                    catch (Exception) {
                        flag = false;
                    }
                    break;
                case ServiceControllerStatus.StopPending:
                    try {
                        sc.WaitForStatus(ServiceControllerStatus.Stopped);
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running);

                        flag = true;
                    }
                    catch (Exception) {
                        flag = false;
                    }
                    break;
                case ServiceControllerStatus.PausePending:
                    try {
                        sc.WaitForStatus(ServiceControllerStatus.Paused);
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running);

                        flag = true;
                    }
                    catch (Exception) {
                        flag = false;
                    }
                    break;
                case ServiceControllerStatus.Running:
                    flag = true;
                    break;
                case ServiceControllerStatus.StartPending:
                case ServiceControllerStatus.ContinuePending:
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    flag = true;
                    break;
                default:
                    break;
            }

            return flag;
        }

        /// <summary>
        /// Создает таблицу для задач с проверкой дубликации файлов
        /// </summary>
        /// <param name="taskName">Имя таблицы</param>
        public static void CreateTableFingerprint(string taskName) {
            if (!StartFirebirdService()) {
                throw new Exception("Не удалось запустить службу Firebird");
            }

            string connStr = @"SELECT 1 FROM RDB$RELATIONS
                               WHERE RDB$RELATION_NAME = '" + taskName + "'";
            object result;
            using (FbConnection connection = new FbConnection(ConnectionString)) {
                connection.Open();

                using (FbCommand command = new FbCommand(connStr, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        result = command.ExecuteScalar();

                        transaction.Rollback();
                    }
                }
            }
            if (result != null) {
                return;
            }

            connStr = "CREATE TABLE " + taskName + @"
                            (FILENAME CHAR(255) NOT NULL,
                                HASH CHAR(255) NOT NULL,
                                OPERATIONDATE TIMESTAMP NOT NULL,
                                MODIFYDATE TIMESTAMP NOT NULL
                            );";
            using (FbConnection connection = new FbConnection(ConnectionString)) {
                connection.Open();

                using (FbCommand command = new FbCommand(connStr, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Создает таблицу для хранения данных об обработанных файлах в случае её отсутствия
        /// </summary>
        public static void CreateTableOperationInfo() {
            if (!StartFirebirdService()) {
                throw new Exception("Не удалось запустить службу Firebird");
            }

            string connStr = @"SELECT 1 FROM RDB$RELATIONS
                               WHERE RDB$RELATION_NAME = 'FILES'";
            object result;
            using (FbConnection connection = new FbConnection(ConnectionString)) {
                connection.Open();

                using (FbCommand command = new FbCommand(connStr, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        result = command.ExecuteScalar();

                        transaction.Rollback();
                    }
                }
            }
            if (result != null) {
                return;
            }

            connStr = "CREATE TABLE FILES " +
                             @"(ID INTEGER NOT NULL,
                                FILENAME VARCHAR(256) NOT NULL,
                                FILETYPE VARCHAR(256) NOT NULL,
                                OPERATIONDATE TIMESTAMP NOT NULL
                            );";
            using (FbConnection connection = new FbConnection(ConnectionString)) {
                connection.Open();

                using (FbCommand command = new FbCommand(connStr, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }

                connStr = "ALTER TABLE FILES ADD CONSTRAINT PK_FILES PRIMARY KEY (ID);";
                using (FbCommand command = new FbCommand(connStr, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }

                connStr = "CREATE GENERATOR GEN_FILES_ID;";
                using (FbCommand command = new FbCommand(connStr, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }

                connStr = "CREATE OR ALTER TRIGGER FILES_BI FOR FILES " +
                          "ACTIVE BEFORE INSERT POSITION 0 " +
                          "as " +
                          "begin " +
                                "if (new.id is null) then " +
                                    "new.id = gen_id(gen_files_id,1); " +
                          "end";
                using (FbCommand command = new FbCommand(connStr, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет наличие дубликата файла
        /// </summary>
        /// <param name="taskName">Имя задачи</param>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns></returns>
        public static bool CheckForDuplicate(string taskName, string filePath) {
            string fileName = Path.GetFileName(filePath);
            string hash = Pbkdf2Cryptography.GetMD5Hash(File.ReadAllBytes(filePath));
            string connStr = @"SELECT 1 FROM " + taskName +
                             @" WHERE FILENAME = '" + fileName + "' " +
                             @"AND HASH = '" + hash + "'";
            object result;
            using (FbConnection connection = new FbConnection(ConnectionString)) {
                connection.Open();

                using (FbCommand command = new FbCommand(connStr, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        result = command.ExecuteScalar();

                        transaction.Rollback();
                    }
                }
            }

            return (result != null);
        }

        /// <summary>
        /// Добавляет запись с признаком уникальности обработанного файла
        /// </summary>
        /// <param name="taskName">Имя задачи</param>
        /// <param name="filePath">Путь к файлу</param>
        public static void CreateFileFingerprint(string taskName, string filePath) {
            string fileName = Path.GetFileName(filePath);
            FileInfo fileInfo = new FileInfo(filePath);
            string hash = Pbkdf2Cryptography.GetMD5Hash(File.ReadAllBytes(filePath));

            string connStr = "INSERT INTO " + taskName +
                          " (FILENAME, HASH, OPERATIONDATE, MODIFYDATE) " +
                          string.Format("VALUES('{0}','{1}','{2}','{3}');", fileName, hash,
                          DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));

            using (FbConnection connection = new FbConnection(ConnectionString)) {
                connection.Open();

                using (FbCommand command = new FbCommand(connStr, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Добавляет данными об обработанных файлах
        /// </summary>
        /// <param name="fileInfos">Массив данных содержащий информацию об отправленных файлах</param>
        private static void CreateFileOperationInfo(FileOperationInfo[] fileInfos) {
            using (FbConnection connection = new FbConnection(ConnectionString)) {
                connection.Open();

                foreach (FileOperationInfo fileInfo in fileInfos) {
                    string connStr = "INSERT INTO FILES " +
                          "(FILENAME, FILETYPE, OPERATIONDATE) " +
                          string.Format("VALUES('{0}','{1}','{2}');", fileInfo.FileName, fileInfo.TaskName,
                          fileInfo.OperationDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    using (FbCommand command = new FbCommand(connStr, connection)) {
                        using (FbTransaction transaction = connection.BeginTransaction()) {
                            command.Transaction = transaction;
                            command.ExecuteNonQuery();

                            transaction.Commit();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Генерирует SQL файл с данными об отправленных файлах
        /// </summary>
        /// <param name="fileInfos">Массив данных содержащий информацию об отправленных файлах</param>
        private static void CreateSqlFileOperationInfo(FileOperationInfo[] fileInfos) {
            string insertStr = string.Empty;

            foreach (FileOperationInfo fileInfo in fileInfos) {
                insertStr += "INSERT INTO FILES " +
                              "(FILENAME, FILETYPE, OPERATIONDATE) " +
                              string.Format("VALUES('{0}','{1}','{2}');\r\n", fileInfo.FileName, fileInfo.TaskName,
                              fileInfo.OperationDate.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            if (insertStr.Length > 0) {
                string fileName = Path.Combine(ServiceHelper.TempSqlPath, string.Format("{0}.sql", DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss")));
                File.WriteAllText(fileName, insertStr);
            }
        }

        /// <summary>
        /// Записывает данные об отправленных файлах в БД или SQL скрипт
        /// </summary>
        /// <param name="fInfos">Массив данных содержащий информацию об отправленных файлах</param>
        /// <remarks>В случае возникновении исключения при записи данных в БД, будет выполнена их сохранение в SQL файл</remarks>
        public static void WriteFileOperationInfo(FileOperationInfo[] fInfos) {
            try {
                CreateFileOperationInfo(fInfos);
            }
            catch (Exception ex) { // При ошибке записи в БД - пишем в файл и выбрасываем отловленное исключение
                CreateSqlFileOperationInfo(fInfos);
                throw ex;
            }
        }

        /// <summary>
        /// Выполняет SQL скрипты
        /// </summary>
        public static void ExecuteSqlScripts() {
            // Выполняем поиск SQL файлов во временной папке программы
            Regex regEx = new Regex(@"^\d{2}.\d{2}.\d{4}_\d{2}.\d{2}.\d{2}$");
            string[] files = Directory.EnumerateFiles(ServiceHelper.TempSqlPath, "*.sql", SearchOption.TopDirectoryOnly)
                    .Where(s => regEx.IsMatch(Path.GetFileNameWithoutExtension(s))).ToArray();
            // Если файлы не найдены прекращаем работу метода
            if (files.Length <= 0) {
                return;
            }

            // Выполняем перебор найденных файлов
            foreach (string sqlPath in files) {
                // Считываем строки из файла
                string[] lines = File.ReadAllLines(sqlPath);
                // Если файл пустой (ну а вдруг) - удаляем его и переходим к следующему файлу
                if (lines.Length <= 0) {
                    File.Delete(sqlPath);
                    continue;
                }
                // Перебираем массив строк файла
                foreach (string cmdText in lines) {
                    // Выполняем запрос из строки файла
                    using (FbConnection connection = new FbConnection(ConnectionString)) {
                        connection.Open();

                        using (FbCommand command = new FbCommand(cmdText, connection)) {
                            using (FbTransaction transaction = connection.BeginTransaction()) {
                                command.Transaction = transaction;
                                command.ExecuteNonQuery();

                                transaction.Commit();
                            }
                        }
                    }
                }
                // Удаляем обработанный файл
                File.Delete(sqlPath);
            }
        }
    }
}
