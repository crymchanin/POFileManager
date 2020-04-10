namespace POFileManagerUpdater {
    partial class MainService {
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent() {
            this.MainEventLog = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.MainEventLog)).BeginInit();
            // 
            // MainEventLog
            // 
            this.MainEventLog.Log = "Application";
            this.MainEventLog.Source = "POFileManagerUpdater";
            // 
            // MainService
            // 
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.MainEventLog)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog MainEventLog;
    }
}
