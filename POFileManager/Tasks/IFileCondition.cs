using System.Collections.Generic;


namespace POFileManager.Tasks {
    public interface IFileCondition {

        bool HasError { get; set; }

        string ErrorString { get; set; }

        bool Check(Dictionary<string, object> _params);
    }
}
