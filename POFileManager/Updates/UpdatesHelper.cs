using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace POFileManager.Updates {
    public static class UpdatesHelper {

        /// <summary>
        /// Проверяет наличие обновлений на сервере
        /// </summary>
        /// <param name="updatesServ">Сервер обновлений</param>
        /// <param name="curVerStr">Текущая версия программы</param>
        /// <param name="productName">Имя продукта</param>
        /// <returns></returns>
        public static bool CheckUpdates(string updatesServ, string curVerStr, string productName) {
            string url = updatesServ + "?method=getVersion&appName=" + productName;
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

        private static PropertyInfo GetProperty(Type type, string propFullName) {
            int length = propFullName.Length;
            int index = propFullName.IndexOf(".");
            string name = propFullName.Substring(0, (index > -1) ? index : length);
            PropertyInfo pi = type.GetProperty(name);

            if (index > 0) {
                string nextName = propFullName.Substring(Math.Max(index + 1, 0), length - index - 1);
                pi = GetProperty(pi.PropertyType, nextName);
            }

            return pi;
        }

        public static void UpdateConfig() {
            Regex regEx = new Regex("^(?<TYPE>.+?(?=:)):(?<NAME>.+?(?==))=(?<VALUE>.*)$");
            string[] confUpdates = File.ReadAllLines(Path.Combine(AppHelper.CurrentDirectory, "settings.upd"));
            foreach (string confUpd in confUpdates) {
                Match match = regEx.Match(confUpd);
                Type valType = Type.GetType(match.Groups["TYPE"].Value);
                string propFullName = match.Groups["NAME"].Value;
                string value = match.Groups["VALUE"].Value;

                Type globalType = typeof(Configuration.Global);
                if (propFullName.Contains(".")) {
                    PropertyInfo pi = GetProperty(globalType, propFullName);
                }
                else {
                    PropertyInfo pi = globalType.GetProperty(propFullName);
                    TypeConverter tc = TypeDescriptor.GetConverter(valType);
                    pi.SetValue(AppHelper.Configuration, tc.ConvertFromString(value));
                }
            }
        }
    }
}
