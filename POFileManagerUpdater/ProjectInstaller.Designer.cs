namespace POFileManagerUpdater {
    partial class ProjectInstaller {
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
            this.MainServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.MainServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // MainServiceProcessInstaller
            // 
            this.MainServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.MainServiceProcessInstaller.Password = null;
            this.MainServiceProcessInstaller.Username = null;
            // 
            // MainServiceInstaller
            // 
            this.MainServiceInstaller.DisplayName = "Обновление пакета программ POFileManager";
            this.MainServiceInstaller.ServiceName = "POFileManagerUpdater";
            this.MainServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.MainServiceProcessInstaller,
            this.MainServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller MainServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller MainServiceInstaller;
    }
}