using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;


namespace POFileManagerUpdater.Configuration {
    /// <summary>
    /// Предоставляет методы для работы с файлом конфигурации в формате JSON
    /// </summary>
    public class ConfHelper {

        internal bool _isSuccess;
        internal string _confPath;
        internal Exception _lastError;
        internal object _fileLock = new object();


        /// <summary>
        /// Инициализирует пустой экземпляр класса Feodosiya.Lib.Conf.ConfHelper с заданным путем конфигурационного файла
        /// </summary>
        /// <param name="confPath">Путь к конфигурационному файлу</param>
        public ConfHelper(string confPath) {
            _isSuccess = false;
            _confPath = confPath;
            _lastError = null;
        }

        /// <summary>
        /// Загружает файл конфигурации и десериализует его содержимое
        /// </summary>
        /// <returns></returns>
        public T LoadConfig<T>() {
            try {
                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(_confPath))) {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));

                    T result = (T)ser.ReadObject(ms);
                    _isSuccess = true;

                    return result;
                }
            }
            catch (Exception error) {
                _isSuccess = false;
                System.Diagnostics.Debug.WriteLine(error.ToString());
                _lastError = error;

                return default(T);
            }
        }

        /// <summary>
        /// Выполняет сериализацию объекта конфигурации и сохраняет его содержимое в файл конфигурации
        /// </summary>
        /// <param name="configuration">Объект конфигурации</param>
        /// <returns></returns>
        public void SaveConfig(object configuration) {
            SaveConfig(configuration, Encoding.Default, false);
        }

        /// <summary>
        /// Выполняет сериализацию объекта конфигурации и сохраняет его содержимое в файл конфигурации
        /// </summary>
        /// <param name="configuration">Объект конфигурации</param>
        /// <param name="formatJson">Если имеет значение true, то файл конфигурации будет приведен к читаемому виду</param>
        /// <returns></returns>
        public void SaveConfig(object configuration, bool formatJson) {
            SaveConfig(configuration, Encoding.Default, formatJson);
        }

        /// <summary>
        /// Выполняет сериализацию объекта конфигурации и сохраняет его содержимое в файл конфигурации
        /// </summary>
        /// <param name="configuration">Объект конфигурации</param>
        /// <param name="encoding">Кодировка в которой будет сохранен файл конфигурации</param>
        /// <returns></returns>
        public void SaveConfig(object configuration, Encoding encoding) {
            SaveConfig(configuration, encoding, false);
        }

        /// <summary>
        /// Выполняет сериализацию объекта конфигурации и сохраняет его содержимое в файл конфигурации
        /// </summary>
        /// <param name="configuration">Объект конфигурации</param>
        /// <param name="encoding">Кодировка в которой будет сохранен файл конфигурации</param>
        /// <param name="formatJson">Если имеет значение true, то файл конфигурации будет приведен к читаемому виду</param>
        /// <returns></returns>
        public void SaveConfig(object configuration, Encoding encoding, bool formatJson) {
            try {
                using (MemoryStream ms = new MemoryStream()) {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(configuration.GetType());

                    ser.WriteObject(ms, configuration);
                    string json = encoding.GetString(ms.ToArray());
                    if (formatJson) {
                        json = JsonHelper.FormatJson(json);
                    }
                    lock (_fileLock) {
                        File.WriteAllText(_confPath, json);
                    }
                }
                _isSuccess = true;
            }
            catch (Exception error) {
                System.Diagnostics.Debug.WriteLine(error.ToString());
                _isSuccess = false;
                _lastError = error;
            }
        }

        /// <summary>
        /// Возвращает путь к конфигурационному файлу
        /// </summary>
        public string Path {
            get {
                return _confPath;
            }
        }

        /// <summary>
        /// Получает значение, указывающее на то, успешно ли загружен файл конфигурации 
        /// </summary>
        public bool Success {
            get {
                return _isSuccess;
            }
        }

        /// <summary>
        /// Возвращает ошибку возникшую в ходе загрузки файла конфигурации
        /// </summary>
        public Exception LastError {
            get {
                return _lastError;
            }
        }
    }
}