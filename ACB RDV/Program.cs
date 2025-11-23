using System;
using System.Windows.Forms;

namespace AcbRdv
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new BackendApp());
        }
    }
}
