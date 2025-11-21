using System;
using System.Windows.Forms;

namespace DDLParserWV
{
    static class Program
    {
        /// <summary>
        /// The entry point for the app.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new DDLParserForm());
        }
    }
}
