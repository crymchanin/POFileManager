using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.ServiceProcess;
using System.Text;


namespace POFileManagerUpdater.Updates {
    public static class UpdatesHelper {

        /// <summary>
        /// Проверяет наличие обновлений на сервере
        /// </summary>
        /// <param name="curVerStr">Текущая версия программы</param>
        /// <returns></returns>
        public static bool CheckUpdates(string curVerStr, string serverName, string productName) {
            string url = serverName + "?method=getVersion&appName=" + productName;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
                using (StreamReader stream = new StreamReader(resp.GetResponseStream())) {
                    string json = stream.ReadToEnd();
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ResponseObject));

                        ResponseObject respObj = (ResponseObject)ser.ReadObject(ms);
                        if (respObj.success) {
                            Version newVersion = new Version(respObj.result);
                            Version currentVersion = new Version(curVerStr);

                            if (newVersion.CompareTo(currentVersion) > 0) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Загружает пакет обновлений с сервера
        /// </summary>
        public static string DownloadPackage(string serverName, string productName) {
            string url = serverName + "?method=getPackage&appName=" + productName;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
                using (Stream stream = resp.GetResponseStream()) {
                    if (resp.Headers[HttpResponseHeader.ContentType] == "application/octet-stream") {
                        string fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), productName + ".pkg");
                        using (FileStream file = File.OpenWrite(fileName)) {
                            stream.CopyTo(file);

                            return fileName;
                        }
                    }
                    else {
                        using (MemoryStream ms = new MemoryStream()) {
                            stream.CopyTo(ms);
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ResponseObject));

                            ResponseObject respObj = (ResponseObject)ser.ReadObject(ms);
                            throw new Exception(respObj.result);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Выполняет установку полученного обновления
        /// </summary>
        /// <param name="fileName"></param>
        public static void InstallUpdate(string fileName) {
            using (ZipArchive archive = ZipFile.Open(fileName, ZipArchiveMode.Read)) {
                foreach (ZipArchiveEntry file in archive.Entries) {
                    string completeFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), file.FullName);
                    if (file.Name == "") {
                        try {
                            Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                        }
                        catch { }
                        continue;
                    }
                    try {
                        file.ExtractToFile(completeFileName, true);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Выполняет загрузку и установку обновления
        /// </summary>
        public static void RunUpdate() {
            try {
                if (ServiceHelper.IsRunning) {
                    return;
                }
                ServiceHelper.IsRunning = true;

                try {
                    string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ServiceHelper.Configuration.ProductName + ".pkg");
                    if (File.Exists(file)) {
                        File.Delete(file);
                    }
                }
                catch { }

                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(ServiceHelper.CurrentDirectory, ServiceHelper.Configuration.ProductName + ".exe"));
                if (!CheckUpdates(fileVersionInfo.FileVersion, ServiceHelper.Configuration.UpdateServer, ServiceHelper.Configuration.ProductName)) {
                    return;
                }

                ServiceHelper.CreateMessage("Выполняется: Остановка запущенной службы " + ServiceHelper.Configuration.ProductName + "...", ServiceHelper.MessageType.Information);
                ServiceController sc = new ServiceController();
                sc.ServiceName = ServiceHelper.Configuration.ProductName;
                if (sc.Status == ServiceControllerStatus.Running) {
                    sc.Stop();
                }
                else if (sc.Status == ServiceControllerStatus.StartPending) {
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    sc.Stop();
                }
                ServiceHelper.CreateMessage("Выполняется: Остановка запущенного клиента " + ServiceHelper.Configuration.ClientName + "...", ServiceHelper.MessageType.Information);
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.Arguments = string.Format("/F /IM {0}.exe", ServiceHelper.Configuration.ClientName);
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.FileName = "taskkill.exe";
                Process.Start(processStartInfo).WaitForExit();

                ServiceHelper.CreateMessage("Выполняется: Загрузка обновлений...", ServiceHelper.MessageType.Information);
                string fileName = DownloadPackage(ServiceHelper.Configuration.UpdateServer, ServiceHelper.Configuration.ProductName);

                ServiceHelper.CreateMessage("Выполняется: Установка обновлений...", ServiceHelper.MessageType.Information);
                InstallUpdate(fileName);

                ServiceHelper.CreateMessage("Установка обновлений выполнена!", ServiceHelper.MessageType.Information);
                if (sc.Status == ServiceControllerStatus.Stopped) {
                    sc.Start();
                }
                else if (sc.Status == ServiceControllerStatus.StopPending) {
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Start();
                }

                File.Delete(fileName);
            }
            catch (Exception error) {
                ServiceHelper.CreateMessage("Ошибка при выполнении обновления. Текст ошибки:\r\n" + error.ToString(), ServiceHelper.MessageType.Error);
            }
            finally {
                ServiceHelper.IsRunning = false;
            }
        }
    }
}
