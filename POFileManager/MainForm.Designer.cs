namespace POFileManager {
    partial class MainForm {
        /// <summary>
        /// Требуется переменная конструктора.
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
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.PingBox = new System.Windows.Forms.PictureBox();
            this.PingLabel = new System.Windows.Forms.Label();
            this.ExitButton = new System.Windows.Forms.Button();
            this.ForceRunButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PingBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MainNotifyIcon
            // 
            this.MainNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MainNotifyIcon_MouseDoubleClick);
            // 
            // PingBox
            // 
            this.PingBox.Location = new System.Drawing.Point(12, 12);
            this.PingBox.Name = "PingBox";
            this.PingBox.Size = new System.Drawing.Size(24, 24);
            this.PingBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PingBox.TabIndex = 0;
            this.PingBox.TabStop = false;
            // 
            // PingLabel
            // 
            this.PingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.PingLabel.Location = new System.Drawing.Point(42, 18);
            this.PingLabel.Name = "PingLabel";
            this.PingLabel.Size = new System.Drawing.Size(280, 18);
            this.PingLabel.TabIndex = 1;
            // 
            // ExitButton
            // 
            this.ExitButton.Location = new System.Drawing.Point(170, 47);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(152, 23);
            this.ExitButton.TabIndex = 1;
            this.ExitButton.Text = "Выйти из программы";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // ForceRunButton
            // 
            this.ForceRunButton.Location = new System.Drawing.Point(12, 47);
            this.ForceRunButton.Name = "ForceRunButton";
            this.ForceRunButton.Size = new System.Drawing.Size(152, 23);
            this.ForceRunButton.TabIndex = 0;
            this.ForceRunButton.Text = "Запуск по требованию";
            this.ForceRunButton.UseVisualStyleBackColor = true;
            this.ForceRunButton.Click += new System.EventHandler(this.ForceRunButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(334, 82);
            this.Controls.Add(this.ForceRunButton);
            this.Controls.Add(this.ExitButton);
            this.Controls.Add(this.PingLabel);
            this.Controls.Add(this.PingBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Файлы ОПС";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.PingBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon MainNotifyIcon;
        private System.Windows.Forms.PictureBox PingBox;
        private System.Windows.Forms.Label PingLabel;
        private System.Windows.Forms.Button ExitButton;
        public System.Windows.Forms.Button ForceRunButton;
    }
}

