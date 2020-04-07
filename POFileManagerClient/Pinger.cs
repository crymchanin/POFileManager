#region Пространства имен
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
#endregion


namespace POFileManagerClient {
    /// <summary>
    /// Успешность результата ICMP запроса
    /// </summary>
    public enum PingStatus {
        /// <summary>
        /// DNS серверу не удалось разрешить имя
        /// </summary>
        DnsError,
        /// <summary>
        /// Успех
        /// </summary>
        Success,
        /// <summary>
        /// Ошибка
        /// </summary>
        Error,
        /// <summary>
        /// Неизвестно
        /// </summary>
        None
    }

    /// <summary>
    /// Определяет доступность узла, посредством отправки пакета по протоколу ICMP 
    /// </summary>
    public static class Pinger {

        #region Члены и свойства класса
        private static string _host = "google.com";
        private static int _timeout = 1000;
        private static volatile bool _isConnectionChecking = false;
        private static int _checkingInterval = 5000;

        private const int MinimumTimeout = 100;
        private const int MaximumTimeout = 60000;

        private const int MinimumCheckingInterval = 500;
        private const int MaximumCheckingInterval = 60000;

        private static System.Windows.Forms.Timer _mainTimer = new System.Windows.Forms.Timer();

        /// <summary>
        /// Проверяемый хост
        /// </summary>
        public static string Host {
            get {
                return _host;
            }
            set {
                _host = value;
            }
        }

        /// <summary>
        /// Максимальное время (после отправки сообщения проверки связи) ожидания сообщения ответа проверки связи ICMP в миллисекундах (минимально 100, маскимально 60000)
        /// </summary>
        public static int Timeout {
            get {
                return _timeout;
            }
            set {
                _timeout = Math.Min(MaximumTimeout, Math.Max(MinimumTimeout, value));
            }
        }

        /// <summary>
        /// Периодичность проверки доступности хоста в миллисекундах (минимально 500, маскимально 60000)
        /// </summary>
        public static int CheckingInterval {
            get {
                return _checkingInterval;
            }
            set {
                _checkingInterval = Math.Min(MaximumCheckingInterval, Math.Max(MinimumCheckingInterval, value));
                _mainTimer.Interval = _checkingInterval;
            }
        }
        #endregion

        /// <summary>
        /// Представляет метод, который будет обрабатывать событие проверки связи с хостом
        /// </summary>
        /// <param name="status">Успешность результата проверки</param>
        public delegate void PingerEventHandler(PingStatus status);

        private static PingerEventHandler _pingerEvent;

        /// <summary>
        /// Происходит по завершении проверки связи с хостом
        /// </summary>
        public static event PingerEventHandler PingerEvent {
            add {
                _pingerEvent += value;
                _mainTimer.Tick += MainTimerHandler;
                _mainTimer.Interval = _checkingInterval;
                _mainTimer.Enabled = true;
            }
            remove {
                _pingerEvent -= value;
                _mainTimer.Enabled = false;
                _mainTimer.Tick -= MainTimerHandler;
            }
        }

        private static void MainTimerHandler(object s, EventArgs e) {
            if (_isConnectionChecking) {
                return;
            }

            Thread thread = new Thread(delegate () {
                _pingerEvent?.Invoke(CheckConnection());
            });
            thread.Start();
        }

        /// <summary>
        /// Запускает проверку соединения
        /// </summary>
        /// <returns></returns>
        public static PingStatus CheckConnection() {
            if (_isConnectionChecking) return PingStatus.None;

            try {
                _isConnectionChecking = true;

                using (Ping ping = new Ping()) {
                    byte[] buffer = new byte[32];
                    PingOptions options = new PingOptions();
                    PingReply reply = ping.Send(Host, Timeout, buffer, options);
                    try {
                        Dns.GetHostEntry(Host);
                    }
                    catch (SocketException ex) {
                        if (ex.ErrorCode == 11003 || ex.ErrorCode == 11004) {
                            return PingStatus.DnsError;
                        }
                    }

                    return (reply.Status == IPStatus.Success) ? PingStatus.Success : PingStatus.Error;
                }
            }
            catch {
                return PingStatus.Error;
            }
            finally {
                _isConnectionChecking = false;
            }
        }
    }
}
