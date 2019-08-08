#region Пространства имен
using Feodosiya.Lib.IO;
using Feodosiya.Lib.Logs;
using System;
using System.Diagnostics;
using System.Globalization;
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
        /// Минимально необходимая версия сборки Feodosiya.Lib
        /// </summary>
        private const string MinFeodosiyaLibVer = "1.2.2.212";
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

                return new Version(fileInfo.FileVersion) >= new Version(MinFeodosiyaLibVer);
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
                if (!AppHelper.PreInit()) {
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Assembly execAssembly = Assembly.GetExecutingAssembly();
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
                                if (AppHelper.ARGS.FirstOrDefault(x => x.ToLower() == "-f") != null) {
                                    AppHelper.CreateMessage("Выполнен запуск по требованию", MessageType.Information);
                                    NamedPipeListener<string>.SendMessage(AppHelper.ProductName, "force");
                                }
                                else {
                                    MessageBox.Show("Программа уже запущена", AppHelper.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                
                                return;
                            }
                        }
                        catch (AbandonedMutexException err) {
                            AppHelper.CreateMessage("Ошибка синхронизации Mutex: " + err.ToString(), MessageType.Error, true);
                            hasHandle = true;
                        }

                        if (CheckDependencies()) {
                            if (!AppHelper.IsAdministrator()) {
                                AppHelper.CreateMessage("Программу необходимо запускать от имени администратора", MessageType.Error, true);
                                return;
                            }

                            Application.Run(new MainForm());
                        }
                        else {
                            AppHelper.CreateMessage("Неверная версия Feodosiya.Lib.dll. Необходимая версия: >=" + MinFeodosiyaLibVer, MessageType.Error, true);

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
            catch (Exception ex) {
                AppHelper.CreateMessage(ex.ToString(), MessageType.Error, true);
            }
        }
        #endregion
    }
}
