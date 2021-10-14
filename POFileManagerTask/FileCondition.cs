using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using POFileManagerService.Tasks;


namespace POFileManagerTask
{
    public class FileCondition : IFileCondition {
        public string ErrorString { get; set; }

        public bool HasError { get; set; }

        public bool Check(Dictionary<string, object> _params) {
            int sendId = 0;
            int count = 0;
            string filename = _params["filename"] as string;
            string connStr = _params["connStr"] as string;

            using (FbConnection connection = new FbConnection(connStr)) {
                connection.Open();

                string req = "SELECT SENDID " +
                             "FROM SEND " +
                             "WHERE FILENAME LIKE '%" + Path.GetFileName(filename) + "%' " +
                             "ORDER BY SENDID DESC";
                using (FbCommand command = new FbCommand(req, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        using (FbDataReader reader = command.ExecuteReader()) {

                            while (reader.Read()) {
                                sendId = reader.GetSafeValue<int>("SENDID");

                                reader.Close();
                                break;
                            }
                            reader.Close();
                        }
                        transaction.Rollback();
                    }
                }
                if (sendId == 0) {
                    HasError = true;
                    ErrorString = "Ошибка: файл '" + filename + "' не найден в БД ИС ОПС Почтовые отправления";
                    return false;
                }

                req = "SELECT COUNT(SENDID) " +
                      "FROM DOCVAL " +
                      "WHERE SENDID = " + sendId.ToString();
                using (FbCommand command = new FbCommand(req, connection)) {
                    using (FbTransaction transaction = connection.BeginTransaction()) {
                        command.Transaction = transaction;
                        count = (int)command.ExecuteScalar();

                        transaction.Rollback();
                    }
                }
                if (count == 0) {
                    HasError = true;
                    ErrorString = "Ошибка: файл '" + filename + "' не содержит записей";
                    return false;
                }
            }

            try {
                string data = string.Empty;
                bool isArchive = true;

                using (ZipInputStream zip = new ZipInputStream(File.OpenRead(filename))) {
                    ZipEntry zipEntry;
                    try {
                        zipEntry = zip.GetNextEntry();
                        while (zipEntry != null) {
                            if (zipEntry.Name != Path.GetFileName(filename)) {
                                HasError = true;
                                ErrorString = "Ошибка: файл '" + filename + "' еще формируется либо поврежден";
                                return false;
                            }

                            byte[] buffer = new byte[4096];
                            using (MemoryStream mem = new MemoryStream()) {
                                StreamUtils.Copy(zip, mem, buffer);
                                data = Encoding.Default.GetString(mem.ToArray());
                                break;
                            }
                        }
                    }
                    catch (ZipException) {
                        isArchive = false;
                    }
                    catch {
                        throw;
                    }
                }
                if (!isArchive) {
                    data = File.ReadAllText(filename);
                }

                int len = data.Where(x => x == Convert.ToByte('\n')).ToArray().Length;
                len = Math.Max(0, len - 1);
                if (count != len) {
                    HasError = true;
                    ErrorString = "Файл '" + filename + "' содержит не все записи. Записей " + len.ToString() + " из " + count.ToString();
                    return false;
                }

            }
            catch (Exception error) {
                HasError = true;
                ErrorString = "Ошибка: файл '" + filename + "' еще формируется либо поврежден. Текст ошибки: " + error.ToString();
                return false;
            }

            return true;
        }
    }
}
