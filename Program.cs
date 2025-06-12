using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TralaleroTralala
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 browser = new Form1();
            SplashForm splash = new SplashForm();

            Thread splashThread = new Thread(new ThreadStart(() =>
            {
                Application.Run(splash);

                splash.BringToFront();
            }));
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.Start();

            // Simulate loading or initialization
            Thread.Sleep(3000); // Replace with real initialization

            // Once done, close splash and run main form
            splash.Invoke(new Action(() => splash.Close()));
            splashThread.Join();

            Application.Run(browser);
        }
    }
}