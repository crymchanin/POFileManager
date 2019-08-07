using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;


namespace Updater.Updates {
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
    }
}
