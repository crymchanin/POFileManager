#region Пространства имен
using Feodosiya.Lib.IO.Pipes;
using Feodosiya.Lib.Logs;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
#endregion


namespace POFileManagerClient {
    static class Program {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main() {
            try {
                // Предварительная инициализация приложения
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
                            hasHandle = mutex.WaitOne(2000, false);
                            if (hasHandle == false) {
                                NamedPipeListener<string>.SendMessage(AppHelper.ProductName, "maximize", 10000);

                                return;
                            }
                        }
                        catch (AbandonedMutexException err) {
                            AppHelper.CreateMessage("Ошибка синхронизации Mutex: " + err.ToString(), MessageType.Error, false);
                            hasHandle = true;
                        }

                        Application.Run(new MainForm());
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
    }
}
