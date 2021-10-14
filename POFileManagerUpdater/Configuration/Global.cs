using System;
using System.Runtime.Serialization;


namespace POFileManagerUpdater.Configuration {
    [DataContract]
    public class Global {
        [DataMember]
        public bool CheckUpdates { get; set; }

        [DataMember]
        public string ProductName { get; set; }

        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public string UpdateServer { get; set; }

        [DataMember]
        public int UpdateCheckInterval { get; set; }

        [DataMember]
        public int AdditionalTime { get; set; }

        [OnSerializing()]
        internal void OnSerializing(StreamingContext context) {
            int minTaskInterval = 5;
            UpdateCheckInterval = (UpdateCheckInterval <= 0) ? minTaskInterval : UpdateCheckInterval;
            AdditionalTime = (AdditionalTime <= 0) ? 30 : Math.Max(30, AdditionalTime);
        }
    }
}
