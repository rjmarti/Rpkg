using System;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml;
using Rpkg.PSConsole;

namespace Rpkg
{
    class Program
    {
        private static string action;
        private static Rpkg rpkg;
        private static App app;
        static void Main(string[] args)
        {
            rpkg = new Rpkg();
            app = new App();

            ExitOnError(ParseArgs(args));

            //Se especificó acción
            if (!String.IsNullOrWhiteSpace(action))
            {
                if (action == "create")
                {

                    CreateRpkg();
                }

                if (action == "install")
                {
                }
            }
        }

        static void CreateRpkg()
        {
            ExitOnError(ReadAppInfo());
            Info();
            ExecuteTask();
        }

        static void InstallRpkg()
        {

        }

        static void ExecuteTask()
        {
            var taskPath = Utils.JoinPath(rpkg.AppPath, "_Task");
            var di = new DirectoryInfo(taskPath);
            var files = di.GetFiles("*.ps1").OrderBy(f => f.Name);
            PSConfig.AddVariable("FileName", "\\kk.txt");
            PSConfig.AddVariable("AppPath", rpkg.AppPath);
            foreach (var file in files)
            {
                Write.Label("Ejecutando: ", file.FullName);
                var script = File.ReadAllText(file.FullName);
                //script = script.Replace("{rpkg.AppPath}",rpkg.AppPath);
                Console.WriteLine(PS.ExecutePS(script));
            }

        }
        static void Info()
        {
            Write.Default();
            //Console.Clear();

            Write.Info("==================================");
            Write.Label("Rpkg: ", Assembly.GetEntryAssembly().GetName().Version.ToString());
            Write.Info("==================================");


            if (action == "create")
            {
                Write.Label("App", app.Nombre);
                Write.Label("Current Version", app.Version);
                Write.Label("Action", action);
                Write.Info("----------------------------------");
            }

            if (action == "install")
            {
                Write.Label("Install Path", rpkg.InstallPath);
            }
        }
        static bool ReadAppInfo()
        {
            var appInfo = Utils.JoinPath(rpkg.AppPath, "_Info", "AppInfo.xml");
            Write.Label("AppInfo: ", appInfo);

            if (!File.Exists(appInfo))
            {
                Write.Error("Archivo: {0} no encontrado.", appInfo);
                return false;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(appInfo);

                app.Nombre = doc.SelectSingleNode("//appSettings/add[@key='AppName']/@value").Value;
                app.Version = doc.SelectSingleNode("//appSettings/add[@key='CurrentVersion']/@value").Value;
            }
            catch (Exception ex)
            {
                Write.Error("Error: {0}", ex.Message);
                return false;
            }
            return true;
        }
        static bool ParseArgs(string[] args)
        {

            if (args.Length < 2)
            {
                Write.Error("Faltan parametros.");
                Write.Info("Uso: -create (-c) path_to_app_deploy");
                Write.Info("   : -install (-i) package.rpkg");
                return false;
            }

            if (args[0].StartsWith("-i"))
            {
                action = "install";
            }

            if (args[0].StartsWith("-c"))
            {
                action = "create";
                rpkg.AppPath = args[1];
                
            }

            //For Debug
            // action = "create";
            // rpkg.AppPath = @"E:\Temp\WSEmpleoAPI";

            return true;
        }
        static void ReadAllSettings()
        {
            try
            {
                rpkg.LogPath = ConfigurationManager.AppSettings["LogPath"];
                rpkg.BackupPath = ConfigurationManager.AppSettings["BackupPath"];
                rpkg.AppPath = ConfigurationManager.AppSettings["AppPath"];
                rpkg.InstallPath = ConfigurationManager.AppSettings["InstallPath"];
            }
            catch (ConfigurationException)
            {
                Console.WriteLine("Error reading app settings");
            }
        }
        static private void ExitOnError(bool ok)
        {
            if (!ok)
            {
                System.Environment.Exit(-1);
            }
        }
    }
}