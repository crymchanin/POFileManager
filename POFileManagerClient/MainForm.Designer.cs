namespace POFileManagerClient {
    partial class MainForm {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.PingLabel = new System.Windows.Forms.Label();
            this.PingBox = new System.Windows.Forms.PictureBox();
            this.ForceRunButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PingBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MainNotifyIcon
            // 
            this.MainNotifyIcon.Visible = true;
            this.MainNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MainNotifyIcon_MouseDoubleClick);
            // 
            // PingLabel
            // 
            this.PingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.PingLabel.Location = new System.Drawing.Point(50, 21);
            this.PingLabel.Name = "PingLabel";
            this.PingLabel.Size = new System.Drawing.Size(352, 18);
            this.PingLabel.TabIndex = 3;
            // 
            // PingBox
            // 
            this.PingBox.Location = new System.Drawing.Point(12, 12);
            this.PingBox.Name = "PingBox";
            this.PingBox.Size = new System.Drawing.Size(32, 32);
            this.PingBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PingBox.TabIndex = 2;
            this.PingBox.TabStop = false;
            // 
            // ForceRunButton
            // 
            this.ForceRunButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ForceRunButton.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ForceRunButton.Location = new System.Drawing.Point(12, 83);
            this.ForceRunButton.Name = "ForceRunButton";
            this.ForceRunButton.Size = new System.Drawing.Size(390, 27);
            this.ForceRunButton.TabIndex = 0;
            this.ForceRunButton.Text = "Выгрузка файлов";
            this.ForceRunButton.UseVisualStyleBackColor = true;
            this.ForceRunButton.Click += new System.EventHandler(this.ForceRunButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(414, 122);
            this.Controls.Add(this.ForceRunButton);
            this.Controls.Add(this.PingLabel);
            this.Controls.Add(this.PingBox);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Обмен файлами ОПС";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.PingBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon MainNotifyIcon;
        private System.Windows.Forms.Label PingLabel;
        private System.Windows.Forms.PictureBox PingBox;
        public System.Windows.Forms.Button ForceRunButton;
    }
}

