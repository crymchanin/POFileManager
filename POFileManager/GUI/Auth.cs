using System;
using System.Windows.Forms;


namespace POFileManager.GUI {
    public partial class Auth : Form {

        // Пароль
        public string Password { get; set; }

        public Auth() {
            InitializeComponent();
        }

        private void ConfirmPass() {
            string pwd = PasswordBox.Text;
            if (string.IsNullOrWhiteSpace(pwd) || string.IsNullOrWhiteSpace(Password)) {
                System.Media.SystemSounds.Beep.Play();
                return;
            }
            if (pwd != Password) {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e) {
            ConfirmPass();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                ConfirmPass();
            }
        }
    }
}
