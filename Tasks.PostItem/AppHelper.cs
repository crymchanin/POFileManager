using FirebirdSql.Data.FirebirdClient;


namespace Tasks {
    public static class AppHelper {
        public static T GetSafeValue<T>(this FbDataReader reader, string fieldName) {
            return (reader.IsDBNull(reader.GetOrdinal(fieldName))) ? default(T) : (T)reader[fieldName];
        }
    }
}
