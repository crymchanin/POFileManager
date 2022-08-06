#region Пространства имен
using Feodosiya.Lib.Conf;
using FluentFTP;
using ICSharpCode.SharpZipLib.Zip;
using POFileManagerService.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#endregion


namespace POFileManagerService.Net {
    public static class FtpHelper {

        #region Члены и свойства класса
        /// <summary>
        /// Параметры подключения к ftp серверу
        /// </summary>
        public static Configuration.Ftp FtpConfinguration { get; set; }
        #endregion


        private static void CompressFolder(string path, ZipFile zip, int folderOffset, List<FileOperationInfo> allFiles) {

            foreach (string filePath in Directory.GetFiles(path)) {

                FileInfo fileInfo = new FileInfo(filePath);

                string entryName = filePath.Substring(folderOffset);
                entryName = ZipEntry.CleanName(entryName);
                zip.BeginUpdate();
                zip.Add(filePath, entryName);
                zip.CommitUpdate();

                FileOperationInfo fInfo = new FileOperationInfo();
                fInfo.FileName = Path.GetFileName(filePath);
                fInfo.TaskName = Path.GetFileName(Path.GetDirectoryName(filePath));
                fInfo.OperationDate = DateTime.Now;
                File.Delete(filePath);
                allFiles.Add(fInfo);
            }
            foreach (string folder in Directory.GetDirectories(path)) {
                CompressFolder(folder, zip, folderOffset, allFiles);
            }
        }

        /// <summary>
        /// Запаковывает файлы располагающиеся по заданному пути в архив
        /// </summary>
        /// <param name="srcPath">Исходная папка</param>
        /// <param name="zipName">Имя для создаваемого архива</param>
        public static FileOperationInfo[] PackFiles(string srcPath, string zipName) {
            if (Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories).Length == 0) {
                return null;
            }

            List<FileOperationInfo> fInfos = new List<FileOperationInfo>();
            int folderOffset = srcPath.Length + (srcPath.EndsWith("\\") ? 0 : 1);
            using (ZipFile zip = ZipFile.Create(zipName)) {
                CompressFolder(srcPath, zip, folderOffset, fInfos);
            }

            return fInfos.ToArray();
        }

        public static void AddPackageMeta(string zipPath, PackageMeta packageMeta) {
            string metaName = "package.meta";
            ConfHelper confHelper = new ConfHelper(metaName);
            string json = confHelper.GetConfigJson(packageMeta, Encoding.UTF8, true);

            using (ZipFile zip = new ZipFile(zipPath)) {
                zip.BeginUpdate();
                zip.Add(new ZipDataSource(Encoding.UTF8.GetBytes(json)), metaName);
                zip.CommitUpdate();
            }
        }

        /// <summary>
        /// Возвращает массив данных содержащих информацию о файлах на отправку 
        /// </summary>
        /// <param name="zipPath">Путь к zip архиву с файлами на отправку</param>
        /// <returns></returns>
        public static IEnumerable<FileOperationInfo> GetFilesOperationInfoFromZip(string zipPath) {
            using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(zipPath))) {
                ZipEntry zipEntry = zipStream.GetNextEntry();
                while (zipEntry != null) {
                    string zipEntryName = zipEntry.Name;
                    string taskName = Path.GetDirectoryName(zipEntryName);
                    string fileName = Path.GetFileName(zipEntryName);
                    zipEntry = zipStream.GetNextEntry();

                    yield return new FileOperationInfo() { TaskName = taskName, FileName = fileName, OperationDate = DateTime.Now };
                }
            }
        }

        /// <summary>
        /// Загружает заданный архив на ftp
        /// </summary>
        /// <param name="zipPath">Имя архива</param>
        /// <param name="zipCode">Индекс текущего отделения</param>
        /// <param name="exception">Содержит информацию о произошедшей ошибке во время загрузки архива. При успешном выполнении загрузки имеет значение null</param>
        public static bool UploadArchive(string zipPath, int zipCode, out Exception exception) {
            try {
                using (FtpClient client = new FtpClient(FtpConfinguration.Host, FtpConfinguration.Port,
                    new System.Net.NetworkCredential(FtpConfinguration.Username, FtpConfinguration.Password))) {
                    client.Connect();
                    client.DataConnectionType = FtpDataConnectionType.PASV;
                    client.SetWorkingDirectory(FtpConfinguration.Cwd);

                    string ftpFile = string.Format("{0}{1}/{2}", FtpConfinguration.Cwd, zipCode, Path.GetFileName(zipPath));
                    FtpStatus flag = client.UploadFile(zipPath, ftpFile);

                    if (flag == FtpStatus.Success) {
                        File.Delete(zipPath);
                        exception = null;
                    }
                    else {
                        exception = new Exception("Не удалось отправить zip архив");
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex) {
                exception = ex;
                return false;
            }
        }

        /// <summary>
        /// Возвращает список архивов, которые расположеных во временной папке программы и не были обработаны ранее
        /// </summary>
        /// <param name="srcPath">Путь к временной папке программы</param>
        /// <param name="exception">Содержит информацию о произошедшей ошибке во время поиска не отправленных архивов. При успешном выполнении загрузки имеет значение null</param>
        /// <returns></returns>
        public static List<string> GetSkippedZipFiles(string srcPath, out Exception exception) {
            Regex regEx = new Regex(@"^[0-9A-f]{8}-[0-9A-f]{4}-[0-9A-f]{4}-[0-9A-f]{4}-[0-9A-f]{12}$"); // Пример: fb5cff64-7c28-46ca-b4ec-33126ca3c746
            List<string> result = new List<string>();
            try {
                result = Directory.EnumerateFiles(srcPath, "*.zip", SearchOption.TopDirectoryOnly)
                    .Where(s => regEx.IsMatch(Path.GetFileNameWithoutExtension(s))).ToList();

                exception = null;
            }
            catch(Exception ex) {
                exception = ex;
            }

            return result;
        }

        /// <summary>
        /// Проверяет наличие ошибок в архиве
        /// </summary>
        /// <param name="path">Путь к архиву</param>
        /// <returns></returns>
        public static bool TestArchive(string path) {
            bool result;

            try {
                using (ZipFile zip = new ZipFile(path)) {
                    result = zip.TestArchive(true, TestStrategy.FindFirstError, (TestStatus ts, string msg) => {
                        if (msg != null) {
                            ServiceHelper.CreateMessage($"Ошибка в архиве '{path}': " + msg, Feodosiya.Lib.Logs.MessageType.Error, true);
                        }
                    });
                }
            }
            catch (Exception ex) {
                result = false;
                ServiceHelper.CreateMessage($"Ошибка при проверке целостности архива '{path}': " + ex.ToString(), Feodosiya.Lib.Logs.MessageType.Error, true);
            }

            return result;
        }
    }
}
