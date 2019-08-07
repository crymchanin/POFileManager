namespace Configurator {
    partial class TaskForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.NameBox = new System.Windows.Forms.TextBox();
            this.PathBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.RegexBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.IntervalBox = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.AllowDuplicateBox = new System.Windows.Forms.CheckBox();
            this.MoveFileBox = new System.Windows.Forms.CheckBox();
            this.RecursiveBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.UseExternalLibBox = new System.Windows.Forms.CheckBox();
            this.ExternalLibBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ExternalLibParamsView = new System.Windows.Forms.ListView();
            this.AddExternalLibParamsButton = new System.Windows.Forms.Button();
            this.EditExternalLibParamsButton = new System.Windows.Forms.Button();
            this.RemoveExternalLibParamsButton = new System.Windows.Forms.Button();
            this.KeyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ValueHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.OkButton = new System.Windows.Forms.Button();
            this.BrowseMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.FolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MainFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.MainFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.IntervalBox)).BeginInit();
            this.BrowseMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Имя";
            // 
            // NameBox
            // 
            this.NameBox.BackColor = System.Drawing.Color.AliceBlue;
            this.NameBox.Location = new System.Drawing.Point(9, 35);
            this.NameBox.Name = "NameBox";
            this.NameBox.Size = new System.Drawing.Size(279, 23);
            this.NameBox.TabIndex = 0;
            // 
            // PathBox
            // 
            this.PathBox.BackColor = System.Drawing.Color.AliceBlue;
            this.PathBox.Location = new System.Drawing.Point(9, 81);
            this.PathBox.Name = "PathBox";
            this.PathBox.Size = new System.Drawing.Size(328, 23);
            this.PathBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Путь";
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(343, 79);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(92, 26);
            this.BrowseButton.TabIndex = 2;
            this.BrowseButton.Text = "Обзор";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // RegexBox
            // 
            this.RegexBox.BackColor = System.Drawing.Color.AliceBlue;
            this.RegexBox.Location = new System.Drawing.Point(10, 127);
            this.RegexBox.Name = "RegexBox";
            this.RegexBox.Size = new System.Drawing.Size(278, 23);
            this.RegexBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(163, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Регулярное выражение";
            // 
            // IntervalBox
            // 
            this.IntervalBox.BackColor = System.Drawing.Color.AliceBlue;
            this.IntervalBox.Location = new System.Drawing.Point(10, 173);
            this.IntervalBox.Name = "IntervalBox";
            this.IntervalBox.Size = new System.Drawing.Size(161, 23);
            this.IntervalBox.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 153);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(296, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "Отправлять файлы измененные в течении:";
            // 
            // AllowDuplicateBox
            // 
            this.AllowDuplicateBox.AutoSize = true;
            this.AllowDuplicateBox.Location = new System.Drawing.Point(9, 205);
            this.AllowDuplicateBox.Name = "AllowDuplicateBox";
            this.AllowDuplicateBox.Size = new System.Drawing.Size(220, 21);
            this.AllowDuplicateBox.TabIndex = 5;
            this.AllowDuplicateBox.Text = "Повторно отправлять файлы";
            this.AllowDuplicateBox.UseVisualStyleBackColor = true;
            // 
            // MoveFileBox
            // 
            this.MoveFileBox.AutoSize = true;
            this.MoveFileBox.Location = new System.Drawing.Point(9, 232);
            this.MoveFileBox.Name = "MoveFileBox";
            this.MoveFileBox.Size = new System.Drawing.Size(349, 21);
            this.MoveFileBox.TabIndex = 6;
            this.MoveFileBox.Text = "Перемещать файлы из исходного расположения";
            this.MoveFileBox.UseVisualStyleBackColor = true;
            // 
            // RecursiveBox
            // 
            this.RecursiveBox.AutoSize = true;
            this.RecursiveBox.Location = new System.Drawing.Point(9, 259);
            this.RecursiveBox.Name = "RecursiveBox";
            this.RecursiveBox.Size = new System.Drawing.Size(174, 21);
            this.RecursiveBox.TabIndex = 7;
            this.RecursiveBox.Text = "Рекурсивный перебор";
            this.RecursiveBox.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(177, 175);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 17);
            this.label5.TabIndex = 12;
            this.label5.Text = "дней";
            // 
            // UseExternalLibBox
            // 
            this.UseExternalLibBox.AutoSize = true;
            this.UseExternalLibBox.Location = new System.Drawing.Point(9, 296);
            this.UseExternalLibBox.Name = "UseExternalLibBox";
            this.UseExternalLibBox.Size = new System.Drawing.Size(407, 21);
            this.UseExternalLibBox.TabIndex = 8;
            this.UseExternalLibBox.Text = "Использовать внешнюю библиотеку для проверки файла";
            this.UseExternalLibBox.UseVisualStyleBackColor = true;
            this.UseExternalLibBox.CheckedChanged += new System.EventHandler(this.UseExternalLibBox_CheckedChanged);
            // 
            // ExternalLibBox
            // 
            this.ExternalLibBox.BackColor = System.Drawing.Color.AliceBlue;
            this.ExternalLibBox.Enabled = false;
            this.ExternalLibBox.Location = new System.Drawing.Point(8, 343);
            this.ExternalLibBox.Name = "ExternalLibBox";
            this.ExternalLibBox.Size = new System.Drawing.Size(328, 23);
            this.ExternalLibBox.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 323);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(117, 17);
            this.label6.TabIndex = 14;
            this.label6.Text = "Имя библиотеки";
            // 
            // ExternalLibParamsView
            // 
            this.ExternalLibParamsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.KeyHeader,
            this.ValueHeader});
            this.ExternalLibParamsView.Enabled = false;
            this.ExternalLibParamsView.GridLines = true;
            this.ExternalLibParamsView.Location = new System.Drawing.Point(8, 372);
            this.ExternalLibParamsView.Name = "ExternalLibParamsView";
            this.ExternalLibParamsView.Size = new System.Drawing.Size(328, 93);
            this.ExternalLibParamsView.TabIndex = 10;
            this.ExternalLibParamsView.UseCompatibleStateImageBehavior = false;
            this.ExternalLibParamsView.View = System.Windows.Forms.View.Details;
            // 
            // AddExternalLibParamsButton
            // 
            this.AddExternalLibParamsButton.Enabled = false;
            this.AddExternalLibParamsButton.Location = new System.Drawing.Point(342, 372);
            this.AddExternalLibParamsButton.Name = "AddExternalLibParamsButton";
            this.AddExternalLibParamsButton.Size = new System.Drawing.Size(92, 26);
            this.AddExternalLibParamsButton.TabIndex = 11;
            this.AddExternalLibParamsButton.Text = "Добавить";
            this.AddExternalLibParamsButton.UseVisualStyleBackColor = true;
            this.AddExternalLibParamsButton.Click += new System.EventHandler(this.AddExternalLibParamsButton_Click);
            // 
            // EditExternalLibParamsButton
            // 
            this.EditExternalLibParamsButton.Enabled = false;
            this.EditExternalLibParamsButton.Location = new System.Drawing.Point(342, 400);
            this.EditExternalLibParamsButton.Name = "EditExternalLibParamsButton";
            this.EditExternalLibParamsButton.Size = new System.Drawing.Size(92, 26);
            this.EditExternalLibParamsButton.TabIndex = 12;
            this.EditExternalLibParamsButton.Text = "Править";
            this.EditExternalLibParamsButton.UseVisualStyleBackColor = true;
            this.EditExternalLibParamsButton.Click += new System.EventHandler(this.EditExternalLibParamsButton_Click);
            // 
            // RemoveExternalLibParamsButton
            // 
            this.RemoveExternalLibParamsButton.Enabled = false;
            this.RemoveExternalLibParamsButton.Location = new System.Drawing.Point(342, 439);
            this.RemoveExternalLibParamsButton.Name = "RemoveExternalLibParamsButton";
            this.RemoveExternalLibParamsButton.Size = new System.Drawing.Size(92, 26);
            this.RemoveExternalLibParamsButton.TabIndex = 13;
            this.RemoveExternalLibParamsButton.Text = "Удалить";
            this.RemoveExternalLibParamsButton.UseVisualStyleBackColor = true;
            this.RemoveExternalLibParamsButton.Click += new System.EventHandler(this.RemoveExternalLibParamsButton_Click);
            // 
            // KeyHeader
            // 
            this.KeyHeader.Text = "Параметр";
            this.KeyHeader.Width = 142;
            // 
            // ValueHeader
            // 
            this.ValueHeader.Text = "Значение";
            this.ValueHeader.Width = 160;
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(8, 476);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(427, 28);
            this.OkButton.TabIndex = 14;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // BrowseMenuStrip
            // 
            this.BrowseMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FolderMenuItem,
            this.FileMenuItem});
            this.BrowseMenuStrip.Name = "BrowseMenuStrip";
            this.BrowseMenuStrip.Size = new System.Drawing.Size(109, 48);
            // 
            // FolderMenuItem
            // 
            this.FolderMenuItem.Name = "FolderMenuItem";
            this.FolderMenuItem.Size = new System.Drawing.Size(108, 22);
            this.FolderMenuItem.Text = "Папка";
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.Name = "FileMenuItem";
            this.FileMenuItem.Size = new System.Drawing.Size(108, 22);
            this.FileMenuItem.Text = "Файл";
            // 
            // TaskForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(444, 516);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.RemoveExternalLibParamsButton);
            this.Controls.Add(this.EditExternalLibParamsButton);
            this.Controls.Add(this.AddExternalLibParamsButton);
            this.Controls.Add(this.ExternalLibParamsView);
            this.Controls.Add(this.ExternalLibBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.UseExternalLibBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.RecursiveBox);
            this.Controls.Add(this.MoveFileBox);
            this.Controls.Add(this.AllowDuplicateBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.IntervalBox);
            this.Controls.Add(this.RegexBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.PathBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.NameBox);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TaskForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Задача";
            ((System.ComponentModel.ISupportInitialize)(this.IntervalBox)).EndInit();
            this.BrowseMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox NameBox;
        private System.Windows.Forms.TextBox PathBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.TextBox RegexBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown IntervalBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox AllowDuplicateBox;
        private System.Windows.Forms.CheckBox MoveFileBox;
        private System.Windows.Forms.CheckBox RecursiveBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox UseExternalLibBox;
        private System.Windows.Forms.TextBox ExternalLibBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ListView ExternalLibParamsView;
        private System.Windows.Forms.Button AddExternalLibParamsButton;
        private System.Windows.Forms.Button EditExternalLibParamsButton;
        private System.Windows.Forms.Button RemoveExternalLibParamsButton;
        private System.Windows.Forms.ColumnHeader KeyHeader;
        private System.Windows.Forms.ColumnHeader ValueHeader;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.ContextMenuStrip BrowseMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem FolderMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileMenuItem;
        private System.Windows.Forms.OpenFileDialog MainFileDialog;
        private System.Windows.Forms.FolderBrowserDialog MainFolderDialog;
    }
}