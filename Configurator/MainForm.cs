using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Feodosiya.Lib.Conf;
using POFileManager.Configuration;
using System.IO;
using System.Reflection;
using Feodosiya.Lib.IO;


namespace Configurator {
    public partial class MainForm : Form {

        private static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        private static string CD = IOHelper.GetCurrentDir(ExecutingAssembly);

        private static ConfHelper ConfHelper;
        private static Global Configuration;


        private void InitConfiguration() {
            try {
                string confPath = Path.Combine(CD, "settings.conf");
                if (!File.Exists(confPath)) {
                    throw new Exception("Файл конфигурации приложения не был обнаружен");
                }
                ConfHelper = new ConfHelper(confPath);
                Configuration = ConfHelper.LoadConfig<Global>();
                if (!ConfHelper.Success) {
                    throw new Exception(ConfHelper.LastError.ToString());
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Ошибка при загрузке конфигурации:\r\n" + ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Load += (s, e) => Application.Exit();
            }
        }

        public MainForm() {
            InitializeComponent();

            InitConfiguration();
        }

        private void SaveButton_Click(object sender, EventArgs e) {
            ConfHelper.SaveConfig(Configuration, Encoding.UTF8, true);
            if (!ConfHelper.Success) {
                MessageBox.Show("Ошибка при сохранении конфигурации:\r\n" + ConfHelper.LastError.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddTaskButton_Click(object sender, EventArgs e) {
            TaskForm form = new TaskForm();
            form.ShowDialog();
        }

        private void EditTaskButton_Click(object sender, EventArgs e) {
            TaskForm form = new TaskForm();
            form.ShowDialog();
        }

        private void DeleteTaskButton_Click(object sender, EventArgs e) {

        }
    }
}
