using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using iSpyApplication;


internal static class Program
{
    public static Mutex Mutex;
    public static string AppPath = "";
    public static string AppDataPath = "";


    private static int _reportedExceptionCount;
    private static ErrorReporting _er;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        //uninstall?
        string[] arguments = Environment.GetCommandLineArgs();

        foreach (string argument in arguments)
        {
            if (argument.Split('=')[0].ToLower() == "/u")
            {
                string guid = argument.Split('=')[1];
                string path = Environment.GetFolderPath(Environment.SpecialFolder.System);
                var si = new ProcessStartInfo(path + "/msiexec.exe", "/x " + guid);
                Process.Start(si);
                Application.Exit();
                return;
            }
        }

        try
        {
            Application.EnableVisualStyles();            
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += ApplicationThreadException;


            bool firstInstance;
            Mutex = new Mutex(false, "iSpy", out firstInstance);

            AppPath = (Application.StartupPath.ToLower());
            AppPath = AppPath.Replace(@"\bin\debug", @"\").Replace(@"\bin\release", @"\");
            AppPath = AppPath.Replace(@"\bin\x86\debug", @"\").Replace(@"\bin\x86\release", @"\");

            AppPath = AppPath.Replace(@"\\", @"\");

            if (!AppPath.EndsWith(@"\"))
                AppPath += @"\";

//#if DEBUG
//        AppDataPath = AppPath;
//#else
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (appPath != null) Directory.SetCurrentDirectory(appPath);
            AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\iSpy\";
            if (!Directory.Exists(AppDataPath))
                EnsureInstall();
//#endif
        bool silentstartup = false;

            string command = "";
            if (args.Length > 0)
            {
                if (args[0].ToLower().Trim() == "-silent" || args[0].ToLower().Trim('\\') == "s")
                {
                    silentstartup = true;
                }
                else
                {
                    for (int index = 0; index < args.Length; index++)
                    {
                        string s = args[index];
                        command += s + " ";
                    }
                }
            }

            if (!firstInstance)
            {
                if (!String.IsNullOrEmpty(command))
                {
                    File.WriteAllText(AppDataPath + "external_command.txt", command);
                    //ensures pickup by filesystemwatcher
                    Thread.Sleep(1000);
                }
                else
                {
                    MessageBox.Show(LocRm.GetString("iSpyRunning"), LocRm.GetString("Note"), MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
                Application.Exit();
                return;
            }
            File.WriteAllText(AppDataPath + "external_command.txt", "");

            //VLC integration
            if (VlcHelper.VlcInstalled)
                VlcHelper.AddVlcToPath();
            var mf = new MainForm(silentstartup, command);
            Application.Run(mf);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            while (ex.InnerException != null)
            {
                MessageBox.Show(ex.InnerException.Message, LocRm.GetString("Error"));
                ex = ex.InnerException;
            }
            throw;
        }
    }

    public static void EnsureInstall()
    {
        string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        //MessageBox.Show(AppPath);

        if (!Directory.Exists(appDataPath + @"\iSpy"))
        {
            Directory.CreateDirectory(appDataPath + @"\iSpy");
        }
        appDataPath += @"\iSpy";
        if (!Directory.Exists(appDataPath + @"\XML"))
        {
            Directory.CreateDirectory(appDataPath + @"\XML");
        }

        var didest = new DirectoryInfo(appDataPath + @"\XML");
        var disource = new DirectoryInfo(appPath + @"\XML");
        File.Copy(disource + @"\PTZ.xml", didest + @"\PTZ.xml", true);
        File.Copy(disource + @"\Translations.xml", didest + @"\Translations.xml", true);
        
        if (!File.Exists(didest + @"\objects.xml"))
            File.Copy(disource + @"\objects.xml", didest + @"\objects.xml");

        if (!File.Exists(didest + @"\config.xml"))
            File.Copy(disource + @"\config.xml", didest + @"\config.xml");

        if (!Directory.Exists(appDataPath + @"\WebServerRoot"))
        {
            Directory.CreateDirectory(appDataPath + @"\WebServerRoot");
        }
        didest = new DirectoryInfo(appDataPath + @"\WebServerRoot");
        disource = new DirectoryInfo(appPath + @"\WebServerRoot");
        CopyAll(disource, didest);

        Directory.SetCurrentDirectory(appPath);
        
    }

    private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        // Check if the target directory exists, if not, create it.
        if (Directory.Exists(target.FullName) == false)
        {
            Directory.CreateDirectory(target.FullName);
        }

        // Copy each file into it’s new directory.
        foreach (FileInfo fi in source.GetFiles())
        {
            //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
            fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }

    private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
    {
        try
        {
            if (e.Exception.Message == "NoDriver calling waveInPrepareHeader")
            {
                //USB audio unplugged (typically the cause) - no other way to catch this exception in the volume level control due to limitation in NAudio
            }
            else
            {
                if (iSpyApplication.MainForm.Conf.Enable_Error_Reporting && _reportedExceptionCount == 0 &&
                    e.Exception != null && e.Exception.Message.Trim() != "")
                {
                    if (_er == null)
                    {
                        _er = new ErrorReporting();
                        _er.UnhandledException = e.Exception;
                        _er.ShowDialog();
                        _er.Dispose();
                        _er = null;
                        _reportedExceptionCount++;
                    }
                }
            }
            MainForm.LogExceptionToFile(e.Exception);
        }
        catch (Exception ex2)
        {
            MainForm.LogExceptionToFile(ex2);
        }
    }
}