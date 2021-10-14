using System;
using System.Collections.Generic;


namespace POFileManagerUpdater.Configuration {
    public static class CollectionExtensions {

        /// <summary>
        /// Выполняет перебор коллекции выполняя указанный метод при каждой итерации
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ie">Коллекция</param>
        /// <param name="action">Делегат с необходимым действием</param>
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action) {
            foreach (T i in ie) {
                action(i);
            }
        }
    }
}
