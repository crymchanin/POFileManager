using System.Runtime.Serialization;


namespace POFileManagerUpdater.Updates {
    [DataContract]
    public class ResponseObject {
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string result { get; set; }
    }
}
