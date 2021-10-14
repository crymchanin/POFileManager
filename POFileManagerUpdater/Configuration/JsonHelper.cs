using System.Linq;
using System.Text;


namespace POFileManagerUpdater.Configuration {
    /// <summary>
    /// Класс предоставляющий методы для работы с JSON
    /// </summary>
    public class JsonHelper {

        // Строка отступов
        private const string INDENT_STRING = "    ";


        /// <summary>
        /// Форматирует указанную строку JSON в читабельную
        /// </summary>
        /// <param name="jsonStr">Исходная строка для форматирования</param>
        /// <returns></returns>
        public static string FormatJson(string jsonStr) {
            int indent = 0;
            bool quoted = false;
            StringBuilder sb = new StringBuilder();

            for (var i = 0; i < jsonStr.Length; i++) {
                var ch = jsonStr[i];
                switch (ch) {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted) {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted) {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var j = i;
                        while (j > 0 && jsonStr[--j] == '\\') {
                            escaped = !escaped;
                        }
                        if (!escaped) {
                            quoted = !quoted;
                        }
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted) {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}