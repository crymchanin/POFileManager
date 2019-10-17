#region Пространства имен
using Feodosiya.Lib.Logs;
using Feodosiya.Lib.Threading;
using POFileManager.GUI;
using POFileManager.Net;
using System;
using System.Drawing;
using System.Windows.Forms;
#endregion


namespace POFileManager {
    public partial class MainForm : Form {

        #region Члены и свойства класса
        private static string enabledText = "Подключен";
        private static string disabledText = "Отключен";
        private static string dnsErrorText = "Ошибка DNS";
        private static Icon enabledIcon = Properties.Resources.green;
        private static Icon disabledIcon = Properties.Resources.red;
        private static Icon dnsErrorIcon = Properties.Resources.yellow;
        private static Bitmap enabledImage = enabledIcon.ToBitmap();
        private static Bitmap disabledImage = disabledIcon.ToBitmap();
        private static Bitmap dnsErrorImage = dnsErrorIcon.ToBitmap();
        #endregion



        // Инициализация пингера
        private bool InitPinger() {
            try {
                MainNotifyIcon.Visible = true;
                MainNotifyIcon.Icon = Properties.Resources.red;
                MainNotifyIcon.Text = disabledText;
                PingBox.Image = disabledImage;
                PingLabel.Text = disabledText;

                // + Проверка подключения к сети интернет
                Pinger.CheckingInterval = AppHelper.Configuration.Pinger.TimerInterval;
                Pinger.Timeout = AppHelper.Configuration.Pinger.PingTimeout;
                Pinger.Host = AppHelper.Configuration.Pinger.HostIP;
                Pinger.PingerEvent += delegate (PingStatus status) {
                        try {
                            if (status == PingStatus.Success) {
                                MainNotifyIcon.Icon = enabledIcon;
                                MainNotifyIcon.Text = enabledText;
                                PingBox.InvokeIfRequired(() => PingBox.Image = enabledImage);
                                PingLabel.InvokeIfRequired(() => PingLabel.Text = enabledText);
                            }
                            else if (status == PingStatus.Error) {
                                MainNotifyIcon.Icon = disabledIcon;
                                MainNotifyIcon.Text = disabledText;
                                PingBox.InvokeIfRequired(() => PingBox.Image = disabledImage);
                                PingLabel.InvokeIfRequired(() => PingLabel.Text = disabledText);
                            }
                            else if (status == PingStatus.DnsError) {
                                MainNotifyIcon.Icon = dnsErrorIcon;
                                MainNotifyIcon.Text = dnsErrorText;
                                PingBox.InvokeIfRequired(() => PingBox.Image = dnsErrorImage);
                                PingLabel.InvokeIfRequired(() => PingLabel.Text = dnsErrorText);
                            }
                        }
                        catch (Exception ex) {
                            AppHelper.CreateMessage("Ошибка:\r\n" + ex.ToString(), MessageType.Error, false, true, true);
                        }
                    };

                return true;
                // - Проверка подключения к сети интернет
            }
            catch (Exception ex) {
                AppHelper.CreateMessage("Ошибка:\r\n" + ex.ToString(), MessageType.Error, false, true, true);
                Load += (s, e) => Application.Exit();
                return false;
            }
        }

        // Конструктор
        public MainForm() {
            InitializeComponent();

            GUIController.Init(this);

            AppHelper.CreateMessage("Инициализация конфигурации приложения...", MessageType.Information, false, false, true);
            if (!AppHelper.InitConfiguration()) {
                GUIController.ExitOnLoaded();
                return;
            }

            AppHelper.CreateMessage("Инициализация основных параметров...", MessageType.Information, false, false, true); 
            if (!AppHelper.InitEngine()) {
                GUIController.ExitOnLoaded();
                return;
            }
            AppHelper.MainTimer.Tick += delegate(object s, EventArgs e) {
                TasksHelper.RunTasksThread();
            };
            AppHelper.CreateMessage("Запуск планировщика...", MessageType.Information, false, false, true);
            AppHelper.MainTimer.Start();

            AppHelper.CreateMessage("Инициализация проверки связи...", MessageType.Information, false, false, true);
            if (!InitPinger()) {
                return;
            }
        }

        // Обработчик события закрытия главной формы приложения
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
            }
            try {
                WindowState = FormWindowState.Minimized;
                //Hide();
            }
            catch (Exception ex) {
                AppHelper.CreateMessage("Ошибка: " + ex.ToString(), MessageType.Error, false, false, true);
            }
        }

        // Обработчик события двойного клика мыши по значку в области уведомлений
        private void MainNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) {
                return;
            }

            try {
                if (WindowState == FormWindowState.Minimized) {
                    Show();
                    WindowState = FormWindowState.Normal;
                }
                else {
                    Activate();
                }
            }
            catch (Exception ex) {
                AppHelper.CreateMessage("Ошибка: " + ex.ToString(), MessageType.Error, false, false, true);
            }
        }

        // Обработчик события нажатия кнопки Выход
        private void ExitButton_Click(object sender, EventArgs e) {
            while (true) {
                if (AppHelper.IsRunning) {
                    DialogResult result = MessageBox.Show("В данный момент выполняется обработка файлов, повторите позже", "Внимание", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Retry) {
                        continue;
                    }
                    else {
                        return;
                    }
                }
                else {
                    break;
                }
            }

            Auth form = new Auth();
            form.Password = AppHelper.Configuration.ZipCode.ToString();
            if (form.ShowDialog() == DialogResult.OK) {
                Application.Exit();
            }
        }

        // Обработчик события нажатия кнопки Запуск по требованию
        private void ForceRunButton_Click(object sender, EventArgs e) {
            AppHelper.CreateMessage("Выполнен запуск по требованию", MessageType.Information);
            TasksHelper.RunTasksThread();
        }
    }
}
