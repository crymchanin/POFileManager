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
            ServiceHelper.CreateMessage("Начало выполнения задач...", MessageType.Debug);

            try { // Исчем и выполняем имеющиеся SQL файлы
                ServiceHelper.CreateMessage("Поиск и выполнение SQL скриптов...", MessageType.Debug);
                SQLHelper.ExecuteSqlScripts();
            }
            catch (Exception ex) { // При ошибке продолжаем работу метода
                ServiceHelper.CreateMessage("Ошибка во время выполнения SQL скрипта: " + ex.ToString(), MessageType.Error, true, true);
            }
            //ServiceHelper.CreateMessage("", MessageType.Debug);
            try {
                // Выполняем перебор задач
                foreach (Task task in ServiceHelper.Configuration.Tasks) {
                    ServiceHelper.CreateMessage(string.Format("Выполнение задачи {0}...", task.Name), MessageType.Debug);

                    bool isDirectory = false;
                    // Если папка или файл назначения не существует, то выбросится исключение, и итерация цикла будет пропущена
                    try {
                        isDirectory = IOHelper.IsPathDirectory(task.Source);
                    }
                    catch {
                        ServiceHelper.CreateMessage(string.Format("Папка или файл '{0}' для задачи {1} не существует", task.Source, task.Name), MessageType.Debug);
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
                        ServiceHelper.CreateMessage(string.Format("Задача {0} настроена на работу с папкой '{1}'", task.Name, task.Source), MessageType.Debug);
                        ServiceHelper.CreateMessage(string.Format("Обработка файлов в папке '{0}'...", task.Source), MessageType.Debug);
                        foreach (string file in Directory.GetFiles(task.Source, "*", sOpt)) {
                            string filename = Path.GetFileName(file);

                            // Выбираем файлы имя которых строго соответствует регулярному выражению
                            if (regEx.IsMatch(filename)) {
                                if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                    ServiceHelper.CreateMessage(string.Format("Файл '{0}' прошел проверку регулярным выражением", filename), MessageType.Debug);
                                }

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
                                        if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                            ServiceHelper.CreateMessage(string.Format("Выполняется проверка даты изменения файла '{0}'", filename), MessageType.Debug);
                                        }
                                        DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                                        date = date.AddDays(task.DayInterval.ToNegative());
                                        FileInfo info = new FileInfo(file);
                                        if (info.LastWriteTime < date) {
                                            if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                                ServiceHelper.CreateMessage(string.Format("Файл {0} не будет обработан из за сроков его давности", filename), MessageType.Debug);
                                            }
                                            continue;
                                        }
                                    }

                                    // Разрешить повторную отправку файлов, если задано
                                    if (!task.AllowDuplicate) {
                                        if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                            ServiceHelper.CreateMessage(string.Format("Проверка файла '{0}' на факт предыдущей его отправки", filename), MessageType.Debug);
                                        }
                                        // Проверяем файл на факт отправки ранее
                                        bool fileExists = SQLHelper.CheckForDuplicate(task.Name, file);
                                        if (fileExists) {
                                            if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                                ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как он был уже ранее отправлен", filename), MessageType.Debug);
                                            }
                                            continue;
                                        }

                                        // Добавляем файл в список обработанных
                                        SQLHelper.CreateFileFingerprint(task.Name, file);
                                    }

                                    // Внешняя библиотека для дополнительных условий обработки файлов
                                    try {
                                        if (!string.IsNullOrWhiteSpace(task.ExternalLib) && task.ExternalLibAsm != null) {
                                            if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                                ServiceHelper.CreateMessage(string.Format("Выполняется проверка дополнительных условий для файла '{0}'...", filename), MessageType.Debug);
                                            }

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
                                                ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как в процессе его проверки произошла ошибка", filename), MessageType.Debug);
                                                continue;
                                            }
                                        }
                                    }
                                    catch (Exception ex) {
                                        ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как в процессе его проверки произошла ошибка", filename), MessageType.Debug);
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
                                        ServiceHelper.CreateMessage(string.Format("Файл '{0}' был перемещен", filename), MessageType.Debug);
                                    }
                                    else {
                                        File.Copy(file, newPath);
                                        ServiceHelper.CreateMessage(string.Format("Файл '{0}' был скопирован", filename), MessageType.Debug);
                                    }
                                }
                                catch (Exception ex) { // При ошибке переходим к следующему файлу. Не обработанный файл остается в старом расположении
                                    ServiceHelper.CreateMessage(string.Format("В процессе обработки файла '{0}' произошла ошибка", filename), MessageType.Debug);
                                    log.AppendLine("Ошибка при перемещении файла '" + file + "' -> '" + newPath + "'.\r\n" + ex.ToString());
                                }
                            }
                        }
                    }
                    else { // Обработка файла
                        string filename = Path.GetFileName(task.Source);
                        ServiceHelper.CreateMessage(string.Format("Задача {0} настроена на работу с файлом '{1}'", task.Name, task.Source), MessageType.Debug);
                        ServiceHelper.CreateMessage(string.Format("Обработка файла '{0}'...", task.Source), MessageType.Debug);

                        // Проверяем соответствие имени файла регулярному выражению
                        if (regEx.IsMatch(filename)) {
                            if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                ServiceHelper.CreateMessage(string.Format("Файл '{0}' прошел проверку регулярным выражением", filename), MessageType.Debug);
                            }

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
                                    if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                        ServiceHelper.CreateMessage(string.Format("Выполняется проверка даты изменения файла '{0}'", filename), MessageType.Debug);
                                    }
                                    DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                                    date = date.AddDays(task.DayInterval.ToNegative());
                                    FileInfo info = new FileInfo(task.Source);
                                    if (info.LastWriteTime < date) {
                                        if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                            ServiceHelper.CreateMessage(string.Format("Файл {0} не будет обработан из за сроков его давности", filename), MessageType.Debug);
                                        }
                                        continue;
                                    }
                                }

                                // Разрешить повторную отправку файлов, если задано
                                if (!task.AllowDuplicate) {
                                    if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                        ServiceHelper.CreateMessage(string.Format("Проверка файла '{0}' на факт предыдущей его отправки", filename), MessageType.Debug);
                                    }
                                    bool fileExists = SQLHelper.CheckForDuplicate(task.Name, task.Source);
                                    if (fileExists) {
                                        if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                            ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как он был уже ранее отправлен", filename), MessageType.Debug);
                                        }
                                        continue;
                                    }

                                    SQLHelper.CreateFileFingerprint(task.Name, task.Source);
                                }

                                // Внешняя библиотека для дополнительных условий обработки файлов
                                try {
                                    if (!string.IsNullOrWhiteSpace(task.ExternalLib) && task.ExternalLibAsm != null) {
                                        if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                                            ServiceHelper.CreateMessage(string.Format("Выполняется проверка дополнительных условий для файла '{0}'...", filename), MessageType.Debug);
                                        }

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
                                            ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как в процессе его проверки произошла ошибка", filename), MessageType.Debug);
                                            continue;
                                        }
                                    }
                                }
                                catch (Exception ex) {
                                    ServiceHelper.CreateMessage(string.Format("Файл '{0}' не будет обработан, так как в процессе его проверки произошла ошибка", filename), MessageType.Debug);
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
                                    ServiceHelper.CreateMessage(string.Format("Файл '{0}' был перемещен", filename), MessageType.Debug);
                                }
                                else {
                                    File.Copy(task.Source, newPath, true);
                                    ServiceHelper.CreateMessage(string.Format("Файл '{0}' был скопирован", filename), MessageType.Debug);
                                }
                            }
                            catch (Exception ex) {
                                ServiceHelper.CreateMessage(string.Format("В процессе обработки файла '{0}' произошла ошибка", filename), MessageType.Debug);
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
                FileOperationInfo[] fInfos;
                // Проверяем наличие не отправленных архивов во временной папке и отправляем их
                ServiceHelper.CreateMessage("Поиск ранее не отправленных архивов...", MessageType.Debug);
                List<string> zipArchives = FtpHelper.GetSkippedZipFiles(ServiceHelper.TempFtpPath, out uploadEx);
                if (zipArchives.Count > 0) {
                    ServiceHelper.CreateMessage("Найдены ранее не отправленные архивы. Попытка повторной отправки...", MessageType.Debug);
                    int errorCount = 0;
                    foreach (string zipArchive in zipArchives) {
                        bool flag = FtpHelper.TestArchive(zipArchive);
                        if (flag) {
                            flag = FtpHelper.UploadArchive(zipArchive, ServiceHelper.Configuration.ZipCode, out uploadEx);
                            if (flag) {
                                try {
                                    fInfos = FtpHelper.GetFilesOperationInfoFromZip(zipArchive).ToArray();
                                    SQLHelper.WriteFileOperationInfo(fInfos);
                                }
                                catch(Exception ex) {
                                    ServiceHelper.CreateMessage("Ошибка при записи информации об отправленных файлах в БД: " + ex.ToString(), MessageType.Error, true);
                                }
                            }
                            else {
                                errorCount++;
                            }
                        }
                        else {
                            errorCount++;
                        }
                        if (uploadEx != null) {
                            ServiceHelper.CreateMessage($"Ошибка при повторной отправке архива '{zipArchive}': " + uploadEx.ToString(), MessageType.Error, true);
                        }
                    }
                    ServiceHelper.CreateMessage((errorCount > 0) ? $"{errorCount} архивов не было отправлено" : "Все архивы отправлены", MessageType.Debug);
                }
                else {
                    if (uploadEx != null) {
                        ServiceHelper.CreateMessage("Ошибка при поиске архивов: " + uploadEx.ToString(), MessageType.Error, true, true);
                    }
                    else {
                        ServiceHelper.CreateMessage("Архивов ожидающих отправки не найдено", MessageType.Debug);
                    }
                }

                // Запаковываем файлы в архив
                ServiceHelper.CreateMessage("Запаковка файлов в архив...", MessageType.Debug);
                string zipName = Path.Combine(ServiceHelper.TempFtpPath, string.Format("{0}_{1}.zip", ServiceHelper.Configuration.ZipCode, DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")));
                fInfos = FtpHelper.PackFiles(ServiceHelper.TempTasksPath, zipName);
                if (fInfos != null) {
                    if (ServiceHelper.Configuration.DebuggingLevel >= 2) {
                        foreach (FileOperationInfo fInfo in fInfos) {
                            ServiceHelper.CreateMessage(string.Format("Упакован файл {0} из задачи {1} в {2}", fInfo.FileName, fInfo.TaskName, fInfo.OperationDate),
                                MessageType.Debug);
                        }
                    }

                    ServiceHelper.CreateMessage("Отправка архива на FTP...", MessageType.Debug);
                    // Закачиваем архив на ftp
                    bool flag = FtpHelper.UploadArchive(zipName, ServiceHelper.Configuration.ZipCode, out uploadEx);
                    if (flag) {
                        ServiceHelper.CreateMessage("Запись информации об отправленных файлах в БД...", MessageType.Debug);
                        SQLHelper.WriteFileOperationInfo(fInfos);
                    }
                    if (uploadEx != null) {
                        throw uploadEx;
                    }
                }
                else {
                    ServiceHelper.CreateMessage("Нет файлов для отправки", MessageType.Debug);
                }
            }
            catch (Exception ex) {
                ServiceHelper.CreateMessage("Ошибка: " + ex.ToString(), MessageType.Error, true, true);
            }
            finally {
                try {
                    ServiceHelper.CreateMessage("Отправка сигнала о завершении выполнения задач...", MessageType.Debug);
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
