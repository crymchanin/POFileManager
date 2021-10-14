#region Пространства имен
using Feodosiya.Lib.Logs;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
#endregion


namespace POFileManagerService.Updates {
    public static class UpdateHelper {

        /// <summary>
        /// Выполняет установку полученного обновления
        /// </summary>
        /// <param name="fileName"></param>
        private static void InstallUpdate(string fileName) {
            using (ZipArchive archive = ZipFile.Open(fileName, ZipArchiveMode.Read)) {
                foreach (ZipArchiveEntry file in archive.Entries) {
                    string completeFileName = Path.Combine(ServiceHelper.CurrentDirectory, file.FullName);
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
        /// Выполняет обновление службы автообновлений
        /// </summary>
        public static void CheckUpdatesForUpdater() {
            try {
                string fileName = Path.Combine(ServiceHelper.CurrentDirectory, ServiceHelper.Configuration.Updates.UpdaterServiceName + ".pkg");
                if (!File.Exists(fileName)) {
                    return;
                }

                ServiceHelper.CreateMessage("Установка обновлений для службы автообновлений...", MessageType.Debug, 1);
                ServiceHelper.CreateMessage("Выполняется: Остановка запущенной службы " + ServiceHelper.Configuration.Updates.UpdaterServiceName + "...", MessageType.Information);
                ServiceController sc = new ServiceController();
                sc.ServiceName = ServiceHelper.Configuration.Updates.UpdaterServiceName;
                if (sc.Status == ServiceControllerStatus.Running) {
                    sc.Stop();
                }
                else if (sc.Status == ServiceControllerStatus.StartPending) {
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    sc.Stop();
                }

                ServiceHelper.CreateMessage("Выполняется: Установка обновлений...", MessageType.Information);
                InstallUpdate(fileName);

                ServiceHelper.CreateMessage("Установка обновлений выполнена!", MessageType.Information);
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
                ServiceHelper.CreateMessage("Ошибка при выполнении обновления. Текст ошибки:\r\n" + error.ToString(), MessageType.Error);
            }
        }

        /// <summary>
        /// Рекурсивная функция для поиска свойства во вложенных классах
        /// </summary>
        /// <param name="type">Тип класса в котором осуществляется поиск свойства</param>
        /// <param name="propFullName">Имя искомого свойства</param>
        /// <param name="instance">Ссылка на экземпляр текущего объекта</param>
        /// <param name="returnInstance">Ссылка на экземпляр объект, свойство которого было найдено</param>
        /// <returns></returns>
        private static PropertyInfo GetProperty(Type type, string propFullName, object instance, out object returnInstance) {
            int length = propFullName.Length;
            int index = propFullName.IndexOf(".");
            string name = propFullName.Substring(0, (index > -1) ? index : length);
            int arrayIndex = -1;
            Regex regEx = new Regex(@"^(?<NAME>.+?(?=\[))\[(?<INDEX>\d+(?=\]))\]$"); // Tasks[0]
            Match match = regEx.Match(name);
            if (match.Success) {
                name = match.Groups["NAME"].Value;
                arrayIndex = int.Parse(match.Groups["INDEX"].Value);
            }
            PropertyInfo pi = type.GetProperty(name);
            Type nextType;

            if (index > 0) {
                string nextName = propFullName.Substring(Math.Max(index + 1, 0), length - index - 1);
                if (arrayIndex >= 0) {
                    dynamic dynamic = pi.GetValue(instance);
                    returnInstance = dynamic[arrayIndex];
                    nextType = returnInstance.GetType();
                }
                else {
                    returnInstance = pi.GetValue(instance);
                    nextType = pi.PropertyType;
                }
                pi = GetProperty(nextType, nextName, returnInstance, out returnInstance);
            }
            else {
                returnInstance = instance;
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

            Regex regEx = new Regex("^(?<TYPE>.+?(?=:)):(?<NAME>.+?(?==))=(?<VALUE>.*)$");// System.Boolean:DebuggingEnabled=true System.String:Ftp.Cwd=/Upload/5_feo/ops/ System.Int32:Tasks[0].DayInterval=7
            // Считываем данные из файла
            string[] confUpdates = File.ReadAllLines(configUpdatePath);
            // Удаляем считанный файл
            File.Delete(configUpdatePath);
            foreach (string confUpd in confUpdates) {
                if (confUpd.StartsWith("#") || confUpd.StartsWith("//") || confUpd.StartsWith(";")) {
                    continue;
                }

                // Парсим данные из строк файла
                Match match = regEx.Match(confUpd);
                Type valType = Type.GetType(match.Groups["TYPE"].Value);
                string propFullName = match.Groups["NAME"].Value;
                string value = match.Groups["VALUE"].Value;

                // Находим свойства класса кофигурации которые необходимо изменить/добавить
                PropertyInfo pi;
                Type globalType = typeof(Configuration.Global);
                object objInstance;
                if (propFullName.Contains(".")) {
                    // Для свойств вложенных классов выполняем поиск рекурсией
                    pi = GetProperty(globalType, propFullName, configInstance, out objInstance);
                }
                else {
                    // Для свойства принадлежащего классу Global
                    pi = globalType.GetProperty(propFullName);
                    objInstance = configInstance;
                }

                // Присваиваем значение найденному свойству
                TypeConverter tc = TypeDescriptor.GetConverter(valType);
                pi.SetValue(objInstance, tc.ConvertFromString(value));
            }

            return true;
        }

        public static void CheckAndInstallConfigUpdate() {
            try {
                if (CheckConfigUpdates(Path.Combine(ServiceHelper.CurrentDirectory, "settings.upd"), ServiceHelper.Configuration)) {
                    ServiceHelper.CreateMessage("Проверка обновлений конфигурационного файла...", MessageType.Debug, 1);
                    // Сохраняем изменения в конфигурационный файл
                    ServiceHelper.ConfHelper.SaveConfig(ServiceHelper.Configuration, Encoding.UTF8, true);
                    ServiceHelper.Configuration = ServiceHelper.ConfHelper.LoadConfig<Configuration.Global>();
                    ServiceHelper.CreateMessage("Выполнено обновление конфигурационного файла", MessageType.Debug, 1);
                }
            }
            catch (Exception ex) {
                ServiceHelper.CreateMessage("Ошибка при установке обновлений конфигурационного файла:\r\n" + ex.ToString(), MessageType.Error, true, true);
            }
        }
    }
}
