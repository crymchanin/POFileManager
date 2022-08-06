#region Пространства имен
using Feodosiya.Lib.IO;
using Feodosiya.Lib.IO.Pipes;
using Feodosiya.Lib.Logs;
using Feodosiya.Lib.Math;
using POFileManagerService.Configuration;
using POFileManagerService.Net;
using POFileManagerService.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
#endregion


namespace POFileManagerService.Tasks {
    public static class TasksHelper {

        /// <summary>
        /// Выполняет задачи
        /// </summary>
        public static void RunTasks() {
            if (ServiceHelper.IsRunning) {
                return;
            }
            ServiceHelper.IsRunning = true;
            ServiceHelper.CreateMessage("Начало выполнения задач...", MessageType.Debug, 1);

            try { // Исчем и выполняем имеющиеся SQL файлы
                ServiceHelper.CreateMessage("Поиск и выполнение SQL скриптов...", MessageType.Debug, 1);
                SQLHelper.ExecuteSqlScripts();
            }
            catch (Exception ex) { // При ошибке продолжаем работу метода
                ServiceHelper.CreateMessage("Ошибка во время выполнения SQL скрипта: " + ex.ToString(), MessageType.Error, true, true);
            }
            //ServiceHelper.CreateMessage("", MessageType.Debug);
            try {
                // Выполняем перебор задач
                foreach (Task task in ServiceHelper.Configuration.Tasks) {
                    ServiceHelper.CreateMessage(string.Format("Выполнение задачи {0}...", task.Name), MessageType.Debug, 1);

                    bool isDirectory = false;
                    // Если папка или файл назначения не существует, то выбросится исключение, и итерация цикла будет пропущена
                    try {
                        isDirectory = IOHelper.IsPathDirectory(task.Source);
                    }
                    catch {
                        ServiceHelper.CreateMessage(string.Format("Папка или файл '{0}' для задачи {1} не существует", task.Source, task.Name), MessageType.Debug, 1);
                        continue;
                    }

                    StringBuilder log = new StringBuilder();

                    // Регулярное выражение, в соответствии с которым будет выполнена обработка файлов
                    if (string.IsNullOrWhiteSpace(task.Regex)) {
                        task.Regex = ".*";
                    }
                    Regex regEx = new Regex(task.Regex);

                    // Выполнять обработку файлов в папке рекурсивно, если задано
                    SearchOption sOpt = (task.Recursive) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                    // Обработка папки/папок
                    if (isDirectory) {
                        ServiceHelper.CreateMessage(string.Format("Задача {0} настроена на работу с папкой '{1}'", task.Name, task.Source), MessageType.Debug, 1);
                        ServiceHelper.CreateMessage(string.Format("Обработка файлов в папке '{0}'...", task.Source), MessageType.Debug, 1);
                        foreach (string file in Directory.GetFiles(task.Source, "*", sOpt)) {
                            string filename = Path.GetFileName(file);

                            // Выбираем файлы имя которых строго соответствует регулярному выражению
                            if (regEx.IsMatch(filename)) {
                                ServiceHelper.CreateMessage(string.Format("Файл '{0}' прошел проверку регулярным выражением", filename), MessageType.Debug, 2);

                                // Создаем временную папку для копирования/перемещения файлов если она не существует
                                string newDir = Path.Combine(ServiceHelper.TempTasksPath, task.Name);
                                if (!Directory.Exists(newDir)) {
                                    Directory.CreateDirectory(newDir);
                                }

                                // Путь файла для копируемого/перемещаемого файла
                                string newPath = Path.Combine(newDir, filename);
                                try {
                                    // Интервал в днях, за который будет выполнена обработка файлов
                                    if (task.DayInterval > 0) {
                                        ServiceHelper.CreateMessage(string.Format("Выполняется проверка даты изменения файла '{0}'", filename), MessageType.Debug, 2);

                                        DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                                        date = date.AddDays(task.DayInterval.ToNegative());
                                        FileInfo info = new FileInfo(file);
                                        if (info.LastWriteTime < date) {
                                            ServiceHelper.CreateMessage(string.Format("Файл {0} не будет обработан из за сроков его давности", filename), MessageType.Debug, 2);
                                            continue;
                                        }
                                    }

                                    // Разрешить повторную отправку файлов, если задано
                                    if (!task.AllowDuplicate) {
                                        ServiceHelper.CreateMessage(string.Format("Проверка файла '{0}' на факт предыдущей его отправки", filename), MessageType.Debug, 2);
                                        // Проверяем файл на факт отправки ранее
                                        bool fileExists = SQLHelper.CheckForDuplicate(task.Name, file);
                                        if (fileExists) {
                                            ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как он был уже ранее отправлен", filename), MessageType.Debug, 2);
                                            continue;
                                        }

                                        // Добавляем файл в список обработанных
                                        SQLHelper.CreateFileFingerprint(task.Name, file);
                                    }

                                    // Внешняя библиотека для дополнительных условий обработки файлов
                                    try {
                                        if (!string.IsNullOrWhiteSpace(task.ExternalLib) && task.ExternalLibAsm != null) {
                                            ServiceHelper.CreateMessage(string.Format("Выполняется проверка дополнительных условий для файла '{0}'...", filename), MessageType.Debug, 2);

                                            Type type = task.ExternalLibAsm.GetType("POFileManagerTask.FileCondition");
                                            IFileCondition condition = (IFileCondition)Activator.CreateInstance(type);
                                            if (task.ExternalLibParams.ContainsKey("filename")) {
                                                task.ExternalLibParams.Remove("filename");
                                            }
                                            task.ExternalLibParams.Add("filename", file);
                                            if (!condition.Check(task.ExternalLibParams)) {
                                                string error = "Неизвестная ошибка";
                                                if (condition.HasError) {
                                                    error = condition.ErrorString;
                                                }
                                                log.AppendLine("Ошибка при проверке файла '" + file + "':\r\n" + error);
                                                ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как в процессе его проверки произошла ошибка", filename), MessageType.Debug, 1);
                                                continue;
                                            }
                                        }
                                    }
                                    catch (Exception ex) {
                                        ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как в процессе его проверки произошла ошибка", filename), MessageType.Debug, 1);
                                        log.AppendLine("Ошибка: " + ex.ToString());
                                        continue;
                                    }

                                    // Удаление файла во временной папке, если он существует
                                    if (File.Exists(newPath)) {
                                        File.Delete(newPath);
                                    }

                                    // Перемещать файл из исходного расположения если задано
                                    if (task.MoveFile) {
                                        File.Move(file, newPath);
                                        ServiceHelper.CreateMessage(string.Format("Файл '{0}' был перемещен", filename), MessageType.Debug, 1);
                                    }
                                    else {
                                        File.Copy(file, newPath);
                                        ServiceHelper.CreateMessage(string.Format("Файл '{0}' был скопирован", filename), MessageType.Debug, 1);
                                    }
                                }
                                catch (Exception ex) { // При ошибке переходим к следующему файлу. Не обработанный файл остается в старом расположении
                                    ServiceHelper.CreateMessage(string.Format("В процессе обработки файла '{0}' произошла ошибка", filename), MessageType.Debug, 1);
                                    log.AppendLine("Ошибка при перемещении файла '" + file + "' -> '" + newPath + "'.\r\n" + ex.ToString());
                                }
                            }
                        }
                    }
                    else { // Обработка файла
                        string filename = Path.GetFileName(task.Source);
                        ServiceHelper.CreateMessage(string.Format("Задача {0} настроена на работу с файлом '{1}'", task.Name, task.Source), MessageType.Debug, 1);
                        ServiceHelper.CreateMessage(string.Format("Обработка файла '{0}'...", task.Source), MessageType.Debug, 1);

                        // Проверяем соответствие имени файла регулярному выражению
                        if (regEx.IsMatch(filename)) {
                            ServiceHelper.CreateMessage(string.Format("Файл '{0}' прошел проверку регулярным выражением", filename), MessageType.Debug, 2);

                            // Создаем временную папку для копирования/перемещения файлов если она не существует
                            string newDir = Path.Combine(ServiceHelper.TempTasksPath, task.Name);
                            if (!Directory.Exists(newDir)) {
                                Directory.CreateDirectory(newDir);
                            }

                            // Путь файла для копируемого/перемещаемого файла
                            string newPath = Path.Combine(newDir, filename);
                            try {
                                // Интервал в днях, за который будет выполнена обработка файлов
                                if (task.DayInterval > 0) {
                                    ServiceHelper.CreateMessage(string.Format("Выполняется проверка даты изменения файла '{0}'", filename), MessageType.Debug, 2);
                                    DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                                    date = date.AddDays(task.DayInterval.ToNegative());
                                    FileInfo info = new FileInfo(task.Source);
                                    if (info.LastWriteTime < date) {
                                        ServiceHelper.CreateMessage(string.Format("Файл {0} не будет обработан из за сроков его давности", filename), MessageType.Debug, 2);
                                        continue;
                                    }
                                }

                                // Разрешить повторную отправку файлов, если задано
                                if (!task.AllowDuplicate) {
                                    ServiceHelper.CreateMessage(string.Format("Проверка файла '{0}' на факт предыдущей его отправки", filename), MessageType.Debug, 2);
                                    bool fileExists = SQLHelper.CheckForDuplicate(task.Name, task.Source);
                                    if (fileExists) {
                                        ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как он был уже ранее отправлен", filename), MessageType.Debug, 2);
                                        continue;
                                    }

                                    SQLHelper.CreateFileFingerprint(task.Name, task.Source);
                                }

                                // Внешняя библиотека для дополнительных условий обработки файлов
                                try {
                                    if (!string.IsNullOrWhiteSpace(task.ExternalLib) && task.ExternalLibAsm != null) {
                                        ServiceHelper.CreateMessage(string.Format("Выполняется проверка дополнительных условий для файла '{0}'...", filename), MessageType.Debug, 2);

                                        Type type = task.ExternalLibAsm.GetType("POFileManagerTask.FileCondition");
                                        IFileCondition condition = (IFileCondition)Activator.CreateInstance(type);
                                        if (task.ExternalLibParams.ContainsKey("filename")) {
                                            task.ExternalLibParams.Remove("filename");
                                        }
                                        task.ExternalLibParams.Add("filename", task.Source);
                                        if (!condition.Check(task.ExternalLibParams)) {
                                            string error = "Неизвестная ошибка";
                                            if (condition.HasError) {
                                                error = condition.ErrorString;
                                            }
                                            ServiceHelper.CreateMessage("Ошибка при проверке файла '" + task.Source + "':\r\n" + error, MessageType.Error, true, true);
                                            ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как в процессе его проверки произошла ошибка", filename), MessageType.Debug, 1);
                                            continue;
                                        }
                                    }
                                }
                                catch (Exception ex) {
                                    ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как в процессе его проверки произошла ошибка", filename), MessageType.Debug, 1);
                                    ServiceHelper.CreateMessage("Ошибка: " + ex.ToString(), MessageType.Error, true, true);
                                    continue;
                                }

                                // Удаление файла во временной папке, если он существует
                                if (File.Exists(newPath)) {
                                    File.Delete(newPath);
                                }

                                // Перемещать файл из исходного расположения если задано
                                if (task.MoveFile) {
                                    File.Move(task.Source, newPath);
                                    ServiceHelper.CreateMessage(string.Format("Файл '{0}' был перемещен", filename), MessageType.Debug, 1);
                                }
                                else {
                                    File.Copy(task.Source, newPath, true);
                                    ServiceHelper.CreateMessage(string.Format("Файл '{0}' был скопирован", filename), MessageType.Debug, 1);
                                }
                            }
                            catch (Exception ex) {
                                ServiceHelper.CreateMessage(string.Format("В процессе обработки файла '{0}' произошла ошибка", filename), MessageType.Debug, 1);
                                ServiceHelper.CreateMessage("Ошибка при перемещении файла '" + task.Source + "' -> '" + newPath + "'.\r\n" + ex.ToString(), MessageType.Error, true, true);
                                continue;
                            }
                        }
                    }

                    if (log.Length > 0) {
                        ServiceHelper.CreateMessage(log.ToString(), MessageType.Error, true, true);
                    }
                }

                Exception uploadEx = null;
                FileOperationInfo[] fInfos = null;
                // Проверяем наличие не отправленных архивов во временной папке и отправляем их
                ServiceHelper.CreateMessage("Поиск ранее не отправленных архивов...", MessageType.Debug, 1);
                List<string> zipArchives = FtpHelper.GetSkippedZipFiles(ServiceHelper.TempFtpPath, out uploadEx);
                if (zipArchives.Count > 0) {
                    ServiceHelper.CreateMessage("Найдены ранее не отправленные архивы. Попытка повторной отправки...", MessageType.Debug, 1);
                    int errorCount = 0;
                    foreach (string zipArchive in zipArchives) {
                        bool flag = FtpHelper.TestArchive(zipArchive);
                        if (flag) {
                            try {
                                fInfos = FtpHelper.GetFilesOperationInfoFromZip(zipArchive).ToArray();

                                flag = FtpHelper.UploadArchive(zipArchive, ServiceHelper.Configuration.ZipCode, out uploadEx);
                                if (flag) {
                                    SQLHelper.WriteFileOperationInfo(fInfos);
                                }
                                else {
                                    errorCount++;
                                }
                            }
                            catch (Exception ex) {
                                errorCount++;
                                ServiceHelper.CreateMessage($"Ошибка при получении списка файлов архива '{zipArchive}': " + ex.ToString(), MessageType.Error, true);
                            }
                        }
                        else {
                            errorCount++;
                        }
                        if (uploadEx != null) {
                            ServiceHelper.CreateMessage($"Ошибка при повторной отправке архива '{zipArchive}': " + uploadEx.ToString(), MessageType.Error, true);
                        }
                    }
                    ServiceHelper.CreateMessage((errorCount > 0) ? $"{errorCount} архивов не было отправлено" : "Все архивы отправлены", MessageType.Debug, 1);
                }
                else {
                    if (uploadEx != null) {
                        ServiceHelper.CreateMessage("Ошибка при поиске архивов: " + uploadEx.ToString(), MessageType.Error, true, true);
                    }
                    else {
                        ServiceHelper.CreateMessage("Архивов ожидающих отправки не найдено", MessageType.Debug, 1);
                    }
                }

                // Запаковываем файлы в архив
                ServiceHelper.CreateMessage("Запаковка файлов в архив...", MessageType.Debug, 1);
                PackageName pn = ServiceHelper.GeneratePackageName(ServiceHelper.TempFtpPath);
                string zipName = pn.Path;
                fInfos = FtpHelper.PackFiles(ServiceHelper.TempTasksPath, zipName);
                if (fInfos != null) {
                    PackageMeta pkgMeta = new PackageMeta() {
                        CreateDate = DateTime.Now,
                        FilesCount = fInfos.Length,
                        FilesList = fInfos.Select(f => $"{f.TaskName}\\{f.FileName}").ToList(),
                        MachineName = ServiceHelper.MachineName,
                        UserName = Feodosiya.Lib.OS.SystemHelper.CurrentUserName(),
                        Name = pn.Id,
                        ZipCode = ServiceHelper.Configuration.ZipCode
                    };
                    FtpHelper.AddPackageMeta(zipName, pkgMeta);

                    if (ServiceHelper.Configuration.DebuggingLevel == 2) {
                        foreach (FileOperationInfo fInfo in fInfos) {
                            ServiceHelper.CreateMessage(string.Format("Упакован файл {0} из задачи {1} в {2}", fInfo.FileName, fInfo.TaskName, fInfo.OperationDate),
                                MessageType.Debug, 2);
                        }
                    }

                    ServiceHelper.CreateMessage("Отправка архива на FTP...", MessageType.Debug, 1);
                    // Закачиваем архив на ftp
                    bool flag = FtpHelper.UploadArchive(zipName, ServiceHelper.Configuration.ZipCode, out uploadEx);
                    if (flag) {
                        ServiceHelper.CreateMessage("Запись информации об отправленных файлах в БД...", MessageType.Debug, 1);
                        SQLHelper.WriteFileOperationInfo(fInfos);
                    }
                    if (uploadEx != null) {
                        throw uploadEx;
                    }
                }
                else {
                    ServiceHelper.CreateMessage("Нет файлов для отправки", MessageType.Debug, 1);
                }
            }
            catch (Exception ex) {
                ServiceHelper.CreateMessage("Ошибка: " + ex.ToString(), MessageType.Error, true, true);
            }
            finally {
                try {
                    ServiceHelper.CreateMessage("Отправка сигнала о завершении выполнения задач...", MessageType.Debug, 1);
                    NamedPipeListener<string>.SendMessage("POFileManagerClient", "complete", 10000);
                }
                catch { }
                ServiceHelper.IsRunning = false;
            }
        }

        /// <summary>
        /// Запускает выполнение задач в отдельном потоке
        /// </summary>
        public static void RunTasksThread() {
            if (ServiceHelper.IsRunning) {
                return;
            }

            try {
                Thread thread = new Thread(RunTasks);
                thread.Start();
            }
            catch (Exception ex) {
                ServiceHelper.CreateMessage("Ошибка: " + ex.ToString(), MessageType.Error, true, true);
            }
        }
    }
}
