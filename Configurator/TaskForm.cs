using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Configurator {
    public partial class TaskForm : Form {
        public TaskForm() {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, EventArgs e) {

        }

        private void AddExternalLibParamsButton_Click(object sender, EventArgs e) {

        }

        private void EditExternalLibParamsButton_Click(object sender, EventArgs e) {

        }

        private void RemoveExternalLibParamsButton_Click(object sender, EventArgs e) {

        }

        private void UseExternalLibBox_CheckedChanged(object sender, EventArgs e) {
            bool flag = UseExternalLibBox.Checked;

            ExternalLibBox.Enabled = flag;
            ExternalLibParamsView.Enabled = flag;
            AddExternalLibParamsButton.Enabled = flag;
            EditExternalLibParamsButton.Enabled = flag;
            RemoveExternalLibParamsButton.Enabled = flag;
        }

        private void OkButton_Click(object sender, EventArgs e) {

        }
    }
}
