using System.Windows.Forms;

namespace _4prak
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            String Path = Form1.Path;
            String Key = Form1.Key;
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
            if (File.Exists(Path))
            {
                Form1.DecryptFile(Path, Key);
            }
        }
    }
}