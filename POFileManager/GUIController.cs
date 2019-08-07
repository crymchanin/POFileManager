using Feodosiya.Lib.Threading;


namespace POFileManager {
    public static class GUIController {

        #region Члены класса
        private static MainForm _this { get; set; }
        private static volatile bool _loadingState = false;
        #endregion

        /// <summary>
        /// Выполняет инициализацию свойств данного класса
        /// </summary>
        /// <param name="form">Главная форма приложения</param>
        public static void Init(MainForm form) {
            _this = form;
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
    }
}
