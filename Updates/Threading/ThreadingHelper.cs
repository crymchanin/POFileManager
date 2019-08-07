using System;
using System.Windows.Forms;


namespace Updater.Threading {
    /// <summary>
    /// Класс содержащий методы для работы с многопоточностью
    /// </summary>
    public static class ThreadingHelper {

        /// <summary>
        /// Вызов делегата через Control.Invoke, если это необходимо
        /// </summary>
        /// <param name="control">Элемент управления</param>
        /// <param name="action">Делегат с необходимым действием</param>
        public static void InvokeIfRequired(this Control control, Action action) {

            if (control.InvokeRequired) {
                control.Invoke(action);
            }
            else {
                action();
            }
        }

        /// <summary>
        /// Вызов делегата через Control.Invoke, если это необходимо
        /// </summary>
        /// <typeparam name="T">Тип параметра делегата</typeparam>
        /// <param name="control">Элемент управления</param>
        /// <param name="action">Делегат с необходимым действием</param>
        /// <param name="arg">Аргумент делагата с действием</param>
        public static void InvokeIfRequired<T>(this Control control, Action<T> action, T arg) {

            if (control.InvokeRequired) {
                control.Invoke(action, arg);
            }
            else {
                action(arg);
            }
        }
    }
}
