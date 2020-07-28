using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iRadio
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>

        public static FormShow FormShow = new FormShow();
        public static Form1 form;
        public static FormLogging formLogging = new FormLogging();

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            formLogging.listBox1.Items.Clear(); 
            Application.Run(form = new Form1());
        }
    }
}
