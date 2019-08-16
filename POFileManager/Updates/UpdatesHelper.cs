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
            req.Timeout = 20000;
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
        /// Отправляет информацию о текущей версии программы
        /// </summary>
        /// <param name="updatesServ">Сервер обновлений</param>
        /// <param name="zipCode">Индекс отеделения</param>
        /// <param name="productName">Имя продукта</param>
        /// <param name="curVerStr">Текущая версия программы</param>
        /// <returns></returns>
        public static bool SendVersionInformation(string updatesServ, int zipCode, string productName, string curVerStr, out string errorMessage) {
            string url = string.Format("{0}?method=putClientInfo&zipcode={1}&appName={2}&version={3}&machine={4}",
                updatesServ, zipCode, productName, curVerStr, Environment.MachineName);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = 20000;
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
                using (StreamReader stream = new StreamReader(resp.GetResponseStream())) {
                    string json = stream.ReadToEnd();
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ResponseObject));

                        ResponseObject respObj = (ResponseObject)ser.ReadObject(ms);
                        if (respObj.success) {
                            errorMessage = string.Empty;
                            return true;
                        }
                        else {
                            errorMessage = respObj.result;
                            return false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Рекурсивная функция для поиска свойства во вложенных классах
        /// </summary>
        /// <param name="type">Тип класса в котором осуществляется поиск свойства</param>
        /// <param name="propFullName">Имя искомого свойства</param>
        /// <returns></returns>
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

        /// <summary>
        /// Выполняет модификацию конфигурационного файла
        /// </summary>
        public static bool CheckConfigUpdates(string configUpdatePath, object configInstance) {
            // Проверяем наличие обновлений для файла конфигурации
            if (!File.Exists(configUpdatePath)) {
                return false;
            }

            Regex regEx = new Regex("^(?<TYPE>.+?(?=:)):(?<NAME>.+?(?==))=(?<VALUE>.*)$");// System.Boolean:DebuggingEnabled=true
            // Считываем данные из файла
            string[] confUpdates = File.ReadAllLines(configUpdatePath);
            // Удаляем считанный файл
            File.Delete(configUpdatePath);
            foreach (string confUpd in confUpdates) {
                // Парсим данные из строк файла
                Match match = regEx.Match(confUpd);
                Type valType = Type.GetType(match.Groups["TYPE"].Value);
                string propFullName = match.Groups["NAME"].Value;
                string value = match.Groups["VALUE"].Value;

                // Находим свойства класса кофигурации которые необходимо изменить/добавить
                PropertyInfo pi;
                Type globalType = typeof(Configuration.Global);
                if (propFullName.Contains(".")) {
                    // Для свойств вложенных классов выполняем поиск рекурсией
                    pi = GetProperty(globalType, propFullName);
                }
                else {
                    // Для свойства принадлежащего классу Global
                    pi = globalType.GetProperty(propFullName);
                }

                // Присваиваем значение найденному свойству
                TypeConverter tc = TypeDescriptor.GetConverter(valType);
                pi.SetValue(configInstance, tc.ConvertFromString(value));
            }

            return true;
        }
    }
}
