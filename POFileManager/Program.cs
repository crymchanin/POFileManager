#region Пространства имен
using Feodosiya.Lib.IO;
using Feodosiya.Lib.Logs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
#endregion


namespace POFileManager {
    static class Program {
        #region Члены и свойства класса
        /// <summary>
        /// Имя данного продукта
        /// </summary>
        public static string ProductName { get; set; }

        /// <summary>
        /// Текущая рабочая папка
        /// </summary>
        public static string CurrentDirectory { get; set; }

        /// <summary>
        /// Путь к текущему запущенному экземпляру программы
        /// </summary>
        public static string ApplicationPath { get; set; }

        /// <summary>
        /// Аргументы программы
        /// </summary>
        public static string[] ARGS { get; set; }

        /// <summary>
        /// Версия данного приложения
        /// </summary>
        public static string Version { get; set; }

        /// <summary>
        /// Необходимая версия сборки Feodosiya.Lib
        /// </summary>
        private const string FeodosiyaLibVer = "1.2.2.212";
        #endregion

        #region Методы
        /// <summary>
        /// Проверяем правильность версий необходимых сборок
        /// </summary>
        /// <returns>Результат проверки</returns>
        private static bool CheckDependencies() {
            try {
                AppDomain domain = AppDomain.CreateDomain("temporary");
                Assembly assembly = domain.Load("Feodosiya.Lib");
                FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                AppDomain.Unload(domain);

                return fileInfo.FileVersion == FeodosiyaLibVer;
            }
            catch {
                return false;
            }
        }
        #endregion

        #region Main
        [STAThread]
        private static void Main(string[] argc) {
            try {
                EventLog eLog = new EventLog();
                ARGS = argc;
                Assembly execAssembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(execAssembly.Location);
                ProductName = fileVersionInfo.ProductName;
                CurrentDirectory = IOHelper.GetCurrentDir(execAssembly);
                ApplicationPath = execAssembly.Location;
                Version = fileVersionInfo.FileVersion;

                AppHelper.Log = new Log(Path.Combine(CurrentDirectory, ProductName + ".log")) { InsertDate = true, AutoCompress = true };
                AppHelper.Log.ExceptionThrownEvent += (e) => MessageBox.Show(e.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                AppHelper.GUID = ((GuidAttribute)execAssembly.GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
                string mutexId = string.Format("Global\\{{{0}}}", AppHelper.GUID);
                bool createdNew;
                MutexAccessRule allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    MutexRights.FullControl, AccessControlType.Allow);
                MutexSecurity securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);

                using (Mutex mutex = new Mutex(false, mutexId, out createdNew, securitySettings)) {
                    bool hasHandle = false;

                    try {
                        try {
                            hasHandle = mutex.WaitOne(3000, false);
                            if (hasHandle == false) {
                                if (ARGS.Contains("-f")) {
                                    NamedPipeListener<string>.SendMessage(ProductName, "force");
                                }
                                else {
                                    MessageBox.Show("Программа уже запущена", ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                
                                return;
                            }
                        }
                        catch (AbandonedMutexException err) {
                            AppHelper.Log.Write("Ошибка синхронизации Mutex: " + err.ToString(), MessageType.Error);
                            hasHandle = true;
                        }

                        if (CheckDependencies()) {
                            if (!AppHelper.IsAdministrator()) {
                                MessageBox.Show("Программу необходимо запускать от имени администратора", ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            Application.Run(new MainForm());
                        }
                        else {
                            MessageBox.Show("Неверная версия Feodosiya.Lib.dll. Необходимая версия: " + FeodosiyaLibVer, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return;
                        }
                    }
                    finally {
                        if (hasHandle) {
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
            catch (Exception error) {
                MessageBox.Show(error.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
