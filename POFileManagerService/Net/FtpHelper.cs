#region Пространства имен
using FluentFTP;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using POFileManagerService.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset) {

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files) {

                FileInfo fileInfo = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset);
                entryName = ZipEntry.CleanName(entryName);
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fileInfo.LastWriteTime;
                newEntry.Size = fileInfo.Length;

                zipStream.PutNextEntry(newEntry);

                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename)) {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders) {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }

        /// <summary>
        /// Запаковывает файлы располагающиеся по заданному пути в архив
        /// </summary>
        /// <param name="srcPath">Исходная папка</param>
        /// <param name="zipName">Имя для создаваемого архива</param>
        public static FileOperationInfo[] PackFiles(string srcPath, string zipName) {
            string[] files = Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories);
            int length = files.Length;
            if (length == 0) {
                return null;
            }
            FileOperationInfo[] fInfos = new FileOperationInfo[length];

            FileStream fsOut = File.Create(zipName);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);
            zipStream.SetLevel(3);

            int folderOffset = srcPath.Length + (srcPath.EndsWith("\\") ? 0 : 1);

            CompressFolder(srcPath, zipStream, folderOffset);

            zipStream.IsStreamOwner = true;
            zipStream.Close();

            int index = 0;
            foreach (string file in files) {
                fInfos[index] = new FileOperationInfo();
                fInfos[index].FileName = Path.GetFileName(file);
                fInfos[index].TaskName = Path.GetFileName(Path.GetDirectoryName(file));
                fInfos[index].OperationDate = DateTime.Now;
                File.Delete(file);

                index++;
            }

            return fInfos;
        }

        /// <summary>
        /// Возвращает массив данных содержащих информацию о файлах на отправку 
        /// </summary>
        /// <param name="zipPath">Путь к zip орхиву с файлами на отправку</param>
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
        /// <param name="exception">Содержит информацию о произошедшей ошибке во вермя загрузки архива. При успешном выполнении загрузки имеет значение null</param>
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
        /// <returns></returns>
        public static List<string> GetSkippedZipFiles(string srcPath) {
            Regex regEx = new Regex(@"^\d{6}_\d{2}_\d{2}_\d{4}_\d{2}_\d{2}_\d{2}$");

            return Directory.EnumerateFiles(srcPath, "*.zip", SearchOption.TopDirectoryOnly)
                    .Where(s => regEx.IsMatch(Path.GetFileNameWithoutExtension(s))).ToList();
        }
    }
}
