using Feodosiya.Lib.Threading;
using System.Windows.Forms;


namespace POFileManager {
    public static class GUIController {

        #region Члены класса
        private static MainForm _this { get; set; }
        private static volatile bool _loadingState = false;
        private static bool _isFormLoaded = false;
        #endregion

        /// <summary>
        /// Выполняет инициализацию свойств данного класса
        /// </summary>
        /// <param name="form">Главная форма приложения</param>
        public static void Init(MainForm form) {
            _this = form;

            _this.Load += (s, e) => _isFormLoaded = true;
        }

        /// <summary>
        /// Устанавливает либо отменяет режим загрузки
        /// </summary>
        public static bool LoadingState {
            get {
                return _loadingState;
            }
            set {
                _loadingState = value;
                _this.ForceRunButton.InvokeIfRequired(() => _this.ForceRunButton.Enabled = !_loadingState);
            }
        }

        /// <summary>
        /// Выполняет завершение работы приложения после загрузки главной формы
        /// </summary>
        public static void ExitOnLoaded() {
            if (_isFormLoaded) {
                Application.Exit();
            }
            else {
                _this.Load += (s, e) => Application.Exit();
            }
        }
    }
}
