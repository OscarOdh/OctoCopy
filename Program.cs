using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace App
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
                mutex.ReleaseMutex();
            }
            else
            {
                // App is already running. We could send a message to bring it to front, but it's already in the tray or taskbar.
                MessageBox.Show("OctoCopy is already running! Check your system tray down by the clock.", "OctoCopy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
