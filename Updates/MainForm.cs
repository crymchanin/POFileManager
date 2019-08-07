using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Updater.Threading;
using Updater.Updates;


namespace Updater {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();

            try {
                Thread thread = new Thread(RunUpdate);
                thread.Start();
            }
            catch (Exception error) {
                MessageBox.Show(error.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Load += (s, e) => Application.Exit();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
            }
        }

        private void RunUpdate() {
            try {
                try {
                    string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Program.Args[Program.PRODUCT_NAME] + ".pkg");
                    if (File.Exists(file)) {
                        File.Delete(file);
                    }
                }
                catch { }

                if (!UpdatesHelper.CheckUpdates(Program.Args[Program.VERSION], Program.Args[Program.SERVER], Program.Args[Program.PRODUCT_NAME])) {
                    return;
                }

                InfoLabel.InvokeIfRequired(() => InfoLabel.Text = "Выполняется: Закрытие запущенной программы...");
                ProcessStartInfo Info = new ProcessStartInfo();
                Info.Arguments = string.Format("/F /IM {0}.exe", Program.Args[Program.PRODUCT_NAME]);
                Info.WindowStyle = ProcessWindowStyle.Hidden;
                Info.CreateNoWindow = true;
                Info.FileName = "taskkill.exe";
                Process.Start(Info).WaitForExit();

                InfoLabel.InvokeIfRequired(() => InfoLabel.Text = "Выполняется: Загрузка обновлений...");
                string fileName = UpdatesHelper.DownloadPackage(Program.Args[Program.SERVER], Program.Args[Program.PRODUCT_NAME]);

                InfoLabel.InvokeIfRequired(() => InfoLabel.Text = "Выполняется: Установка обновлений...");
                UpdatesHelper.InstallUpdate(fileName);

                InfoLabel.InvokeIfRequired(() => InfoLabel.Text = "Готово!");
                Info = new ProcessStartInfo();
                Info.Arguments = "-u";
                Info.FileName = string.Format("{0}.exe", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Program.Args[Program.PRODUCT_NAME]));
                Process.Start(Info);

                File.Delete(fileName);
            }
            catch (Exception error) {
                MessageBox.Show(error.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                Application.Exit();
            }
        }
    }
}
