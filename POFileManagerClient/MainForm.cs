#region Пространства имен
using Feodosiya.Lib.IO.Pipes;
using Feodosiya.Lib.Logs;
using Feodosiya.Lib.Threading;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
#endregion


namespace POFileManagerClient {
    public partial class MainForm : Form {

        #region Приватные члены и свойства
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


        public MainForm() {
            InitializeComponent();

            AppHelper.CreateMessage("Выполнен запуск приложения", MessageType.Information);

            AppHelper.CreateMessage("Инициализация конфигурации приложения...", MessageType.Debug);
            if (!AppHelper.InitConfiguration()) {
                Load += (s, e) => Application.Exit();
                return;
            }

            AppHelper.CreateMessage("Инициализация основных параметров...", MessageType.Debug);
            if (!AppHelper.InitEngine(this)) {
                Load += (s, e) => Application.Exit();
                return;
            }

            AppHelper.CreateMessage("Инициализация проверки связи...", MessageType.Debug);
            if (!InitPinger()) {
                Load += (s, e) => Application.Exit();
                return;
            }

            #if DEBUG
                Text = Text + " (DEBUG)";
                ForceRunButton.Text = ForceRunButton.Text + " (DEBUG)";
            #endif
        }

        /// <summary>
        /// Показывает главное окно программы
        /// </summary>
        public void ShowWindow() {
            try {
                if (WindowState == FormWindowState.Minimized) {
                    Show();
                    WindowState = FormWindowState.Normal;
                }
                else {
                    Activate();
                }
            }
            catch { }
        }

        private bool InitPinger() {
            try {
                MainNotifyIcon.Visible = true;
                MainNotifyIcon.Icon = Properties.Resources.red;
                MainNotifyIcon.Text = disabledText;
                PingBox.Image = disabledImage;
                PingLabel.Text = disabledText;

                Pinger.CheckingInterval = AppHelper.Configuration.Pinger.TimerInterval * 1000;
                Pinger.Timeout = AppHelper.Configuration.Pinger.PingTimeout * 1000;
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
                        AppHelper.CreateMessage("Ошибка:\r\n" + ex.ToString(), MessageType.Error);
                    }
                };

                return true;
            }
            catch (Exception ex) {
                AppHelper.CreateMessage("Ошибка:\r\n" + ex.ToString(), MessageType.Error);
                Load += (s, e) => Application.Exit();
                return false;
            }
        }

        private void ForceRunButton_Click(object sender, EventArgs e) {
            try {
                AppHelper.CreateMessage("Запущена ручная выгрузка файлов", MessageType.Information);
                ForceRunButton.Enabled = false;
                Thread thread = new Thread(delegate() {
                    try {
                        NamedPipeListener<string>.SendMessage("POFileManagerService", "force", 10000);
                        ForceRunButton.InvokeIfRequired(() => ForceRunButton.Enabled = true);
                        this.InvokeIfRequired(() => AppHelper.CreateMessage("Файлы успешно выгружены!", MessageType.Information, true));
                    }
                    catch(Exception ex) {
                        ForceRunButton.InvokeIfRequired(() => ForceRunButton.Enabled = true);
                        this.InvokeIfRequired(() => AppHelper.CreateMessage("Ошибка при выгрузке файлов:\r\n" + ex.ToString(), MessageType.Error, true));
                    }
                });
                thread.Start();
            }
            catch (Exception ex) {
                ForceRunButton.Enabled = true;
                AppHelper.CreateMessage("Ошибка при выгрузке файлов:\r\n" + ex.ToString(), MessageType.Error, true);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
            }
            try {
                WindowState = FormWindowState.Minimized;
                Hide();
            }
            catch { }
        }

        private void MainNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) {
                return;
            }

            ShowWindow();
        }
    }
}
