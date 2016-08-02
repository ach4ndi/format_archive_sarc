using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace unSARC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ToolStripManager.Renderer = new Win8MenuStripRenderer();
            Application.Run(new MainForm());
        }
    }
}
