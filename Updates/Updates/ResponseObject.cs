using System.Runtime.Serialization;


namespace Updater.Updates {
    [DataContract]
    public class ResponseObject {
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string result { get; set; }
    }
}
