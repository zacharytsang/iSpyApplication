using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace OffLine.Installer
{
    // Taken from:http://msdn2.microsoft.com/en-us/library/system.configuration.configurationmanager.aspx
    // Set 'RunInstaller' attribute to true.

    [RunInstaller(true)]
    public class InstallerClass : System.Configuration.Install.Installer
    {
        public InstallerClass()
        {
            // Attach the 'Committed' event.
            Committed += MyInstaller_Committed;
            // Attach the 'Committing' event.
            Committing += MyInstaller_Committing;
        }

        // Event handler for 'Committing' event.
        private void MyInstaller_Committing(object sender, InstallEventArgs e)
        {
            //Console.WriteLine("");
            //Console.WriteLine("Committing Event occured.");
            //Console.WriteLine("");

            //add registry entries for handling URLs
            //RegistryKey rkApp = Registry.ClassesRoot.CreateSubKey("ispy");
            //rkApp.SetValue("URL Protocol", "");
            //rkApp.SetValue("","URL:ispy Protocol");

            //RegistryKey rkAppIcon = rkApp.CreateSubKey("DefaultIcon");
            //rkAppIcon.SetValue("", "\""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\iSpy.exe,1\"");

            //RegistryKey rkShell = rkApp.CreateSubKey("shell");
            //rkShell = rkShell.CreateSubKey("open");
            //rkShell = rkShell.CreateSubKey("command");
            //rkShell.SetValue("", "\""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\iSpy.exe\" \"%1\"");

            //rkShell.Close();
            //rkAppIcon.Close();
            //rkApp.Close();            
        }

        // Event handler for 'Committed' event.
        private void MyInstaller_Committed(object sender, InstallEventArgs e)
        {
            Program.EnsureInstall();
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process.Start(appPath + @"\iSpy.exe");
        }



        // Override the 'Install' method.
        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);
        }

        // Override the 'Commit' method.
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        // Override the 'Rollback' method.
        public override void Rollback(IDictionary savedState)
        {
            //try
            //{
            //    Registry.ClassesRoot.DeleteSubKey("ispy");
            //}
            //catch { }
            base.Rollback(savedState);
        }

        public override void Uninstall(IDictionary savedState)
        {
            string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (Directory.Exists(AppDataPath + @"\iSpy\"))
            {
                Directory.Delete(AppDataPath + @"\iSpy\", true);
            }

            //try
            //{
            //    Registry.ClassesRoot.DeleteSubKey("ispy");
            //}
            //catch { }
            base.Uninstall(savedState);
        }
    }
}