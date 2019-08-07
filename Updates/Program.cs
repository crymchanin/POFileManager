using System;
using System.Windows.Forms;


namespace Updater {
    static class Program {

        public const int VERSION = 0;
        public const int SERVER = 1;
        public const int PRODUCT_NAME = 2;

        public static string[] Args { get; set; }


        [STAThread]
        static void Main(string[] args) {
            if (args.Length < 3) {
                return;
            }
            Args = args;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
