#region Пространства имен
using Feodosiya.Lib.IO;
using Feodosiya.Lib.Logs;
using Feodosiya.Lib.Math;
using POFileManager.Configuration;
using POFileManager.Net;
using POFileManager.SQL;
using POFileManager.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
#endregion


namespace POFileManager {
    public static class TasksHelper {

        /// <summary>
        /// Выполняет задачи
        /// </summary>
        public static void RunTasks() {
            if (AppHelper.IsRunning) {
                return;
            }
            GUIController.LoadingState = true;
            AppHelper.IsRunning = true;

            try { // Исчем и выполняем имеющиеся SQL файлы
                SQLHelper.ExecuteSqlScripts();
            }
            catch (Exception ex) { // При ошибке продолжаем работу метода
                AppHelper.CreateMessage("Ошибка во время выполнения SQL скрипта: " + ex.ToString(), MessageType.Error, false, true, true);
            }

            try {
                // Выполняем перебор задач
                foreach (Task task in AppHelper.Configuration.Tasks) {
                    bool isDirectory = false;
                    // Если папка или файл назначения не существует, то выбросится исключение, и итерация цикла будет пропущена
                    try {
                        isDirectory = IOHelper.IsPathDirectory(task.Source);
                    }
                    catch {
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
                        foreach (string file in Directory.GetFiles(task.Source, "*", sOpt)) {
                            string filename = Path.GetFileName(file);

                            // Выбираем файлы имя которых строго соответствует регулярному выражению
                            if (regEx.IsMatch(filename)) {
                                // Создаем временную папку для копирования/перемещения файлов если она не существует
                                string newDir = Path.Combine(AppHelper.TempTasksPath, task.Name);
                                if (!Directory.Exists(newDir)) {
                                    Directory.CreateDirectory(newDir);
                                }

                                // Путь файла для копируемого/перемещаемого файла
                                string newPath = Path.Combine(newDir, filename);
                                try {
                                    // Интервал в днях, за который будет выполнена обработка файлов
                                    if (task.DayInterval > 0) {
                                        DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                                        date = date.AddDays(task.DayInterval.ToNegative());
                                        FileInfo info = new FileInfo(file);
                                        if (info.LastWriteTime < date) {
                                            continue;
                                        }
                                    }

                                    // Разрешить повторную отправку файлов, если задано
                                    if (!task.AllowDuplicate) {
                                        // Проверяем файл на факт отправки ранее
                                        bool fileExists = SQLHelper.CheckForDuplicate(task.Name, file);
                                        if (fileExists) {
                                            continue;
                                        }

                                        // Добавляем файл в список обработанных
                                        SQLHelper.CreateFileFingerprint(task.Name, file);
                                    }

                                    // Внешняя библиотека для дополнительных условий обработки файлов
                                    try {
                                        if (!string.IsNullOrWhiteSpace(task.ExternalLib) && task.ExternalLibAsm != null) {
                                            Type type = task.ExternalLibAsm.GetType("Tasks.FileCondition");
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
                                                continue;
                                            }
                                        }
                                    }
                                    catch (Exception ex) {
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
                                    }
                                    else {
                                        File.Copy(file, newPath);
                                    }
                                }
                                catch (Exception ex) { // При ошибке переходим к следующему файлу. Не обработанный файл остается в старом расположении
                                    log.AppendLine("Ошибка при перемещении файла '" + file + "' -> '" + newPath + "'.\r\n" + ex.ToString());
                                }
                            }
                        }
                    }
                    else { // Обработка файла
                        string filename = Path.GetFileName(task.Source);

                        // Проверяем соответствие имени файла регулярному выражению
                        if (regEx.IsMatch(filename)) {
                            // Создаем временную папку для копирования/перемещения файлов если она не существует
                            string newDir = Path.Combine(AppHelper.TempTasksPath, task.Name);
                            if (!Directory.Exists(newDir)) {
                                Directory.CreateDirectory(newDir);
                            }

                            // Путь файла для копируемого/перемещаемого файла
                            string newPath = Path.Combine(newDir, filename);
                            try {
                                // Интервал в днях, за который будет выполнена обработка файлов
                                if (task.DayInterval > 0) {
                                    DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                                    date = date.AddDays(task.DayInterval.ToNegative());
                                    FileInfo info = new FileInfo(task.Source);
                                    if (info.LastWriteTime < date) {
                                        continue;
                                    }
                                }

                                // Разрешить повторную отправку файлов, если задано
                                if (!task.AllowDuplicate) {
                                    bool fileExists = SQLHelper.CheckForDuplicate(task.Name, task.Source);
                                    if (fileExists) {
                                        continue;
                                    }

                                    SQLHelper.CreateFileFingerprint(task.Name, task.Source);
                                }

                                // Внешняя библиотека для дополнительных условий обработки файлов
                                try {
                                    if (!string.IsNullOrWhiteSpace(task.ExternalLib) && task.ExternalLibAsm != null) {
                                        Type type = task.ExternalLibAsm.GetType("Tasks.FileCondition");
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
                                            AppHelper.CreateMessage("Ошибка при проверке файла '" + task.Source + "':\r\n" + error, MessageType.Error, false, true, true);
                                            continue;
                                        }
                                    }
                                }
                                catch (Exception ex) {
                                    AppHelper.CreateMessage("Ошибка: " + ex.ToString(), MessageType.Error, false, true, true);
                                    continue;
                                }

                                // Удаление файла во временной папке, если он существует
                                if (File.Exists(newPath)) {
                                    File.Delete(newPath);
                                }

                                // Перемещать файл из исходного расположения если задано
                                if (task.MoveFile) {
                                    File.Move(task.Source, newPath);
                                }
                                else {
                                    File.Copy(task.Source, newPath, true);
                                }
                            }
                            catch (Exception ex) {
                                AppHelper.CreateMessage("Ошибка при перемещении файла '" + task.Source + "' -> '" + newPath + "'.\r\n" + ex.ToString(), MessageType.Error, false, true, true);
                                continue;
                            }
                        }
                    }

                    if (log.Length > 0) {
                        AppHelper.CreateMessage(log.ToString(), MessageType.Error, false, true, true);
                    }
                }

                Exception uploadEx;
                FileOperationInfo[] fInfos;
                // Проверяем наличие неотправленных архивов во временной папке и отправляем их
                try {
                    List<string> zipArchives = FtpHelper.GetSkippedZipFiles(AppHelper.TempFtpPath);
                    if (zipArchives.Count > 0) {
                        foreach (string zipArchive in zipArchives) {
                            fInfos = FtpHelper.GetFilesOperationInfoFromZip(zipArchive).ToArray();
                            bool flag = FtpHelper.UploadArchive(zipArchive, AppHelper.Configuration.ZipCode, out uploadEx);
                            if (flag) {
                                SQLHelper.WriteFileOperationInfo(fInfos);
                            }
                            if (uploadEx != null) {
                                AppHelper.CreateMessage("Ошибка при повторной отправке архива: " + uploadEx.ToString(), MessageType.Error, false, false, true);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    AppHelper.CreateMessage("Ошибка при повторной отправке архива: " + ex.ToString(), MessageType.Error, false, false, true);
                }

                // Запаковываем файлы в архив
                string zipName = Path.Combine(AppHelper.TempFtpPath, string.Format("{0}_{1}.zip", AppHelper.Configuration.ZipCode, DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")));
                fInfos = FtpHelper.PackFiles(AppHelper.TempTasksPath, zipName);
                if (fInfos != null) {
                    foreach (FileOperationInfo fInfo in fInfos) {
                        AppHelper.CreateMessage(string.Format("Упакован файл {0} из задачи {1} в {2}", fInfo.FileName, fInfo.TaskName, fInfo.OperationDate),
                            MessageType.Information);
                    }
                    // Закачиваем архив на ftp
                    bool flag = FtpHelper.UploadArchive(zipName, AppHelper.Configuration.ZipCode, out uploadEx);
                    if (flag) {
                        SQLHelper.WriteFileOperationInfo(fInfos);
                    }
                    if (uploadEx != null) {
                        throw uploadEx;
                    }
                }
            }
            catch (Exception ex) {
                AppHelper.CreateMessage("Ошибка: " + ex.ToString(), MessageType.Error, false, true, true);
            }
            finally {
                AppHelper.IsRunning = false;
                GUIController.LoadingState = false;
            }
        }

        /// <summary>
        /// Запускает выполнение задач в отдельном потоке
        /// </summary>
        
        public static void RunTasksThread() {
            if (AppHelper.IsRunning) {
                return;
            }

            try {
                Thread thread = new Thread(RunTasks);
                thread.Start();
            }
            catch (Exception ex) {
                AppHelper.CreateMessage("Ошибка: " + ex.ToString(), MessageType.Error, false, true, true);
            }
        }
    }
}
