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
        private const string PATH_INFO = "_Info";
        private const string PATH_TASK = "_Task";
        private const string PATH_SITE = "_Site";
        private const string PATH_SQL = "_SQL";
        private const string PATH_TEMP = "_Temp";

        private static bool hasSite = false;
        private static bool hasSQL = false;        
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
                    InstallRpkg();
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
            if (File.Exists(rpkg.FileRpkg))  
            {
                Utils.UnZipPkg(rpkg.FileRpkg, @".\" + PATH_TEMP);
            }
            rpkg.AppPath = @".\" + PATH_TEMP;
            ReadAppInfo(); 
            Info();
        }

        static void ExecuteTask()
        {
            var taskPath = Utils.JoinPath(rpkg.AppPath, PATH_TASK);
            var di = new DirectoryInfo(taskPath);
            var files = di.GetFiles("*.ps1").OrderBy(f => f.Name);
            PSConfig.AddVariable("AppPath", rpkg.AppPath);
            PSConfig.AddVariable("AppName", app.Nombre);
            PSConfig.AddVariable("AppVersion", app.Version);
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
            Write.Label("App", app.Nombre);
            Write.Label("Current Version", app.Version);
            Write.Label("Action", action);
            Write.Info("----------------------------------");

            if (action == "create")
            {
            }

            if (action == "install")
            {
                var di = new DirectoryInfo(Utils.JoinPath(rpkg.AppPath, PATH_SITE));
                var filesSites = di.GetFiles("*.*");
                di = new DirectoryInfo(Utils.JoinPath(rpkg.AppPath,PATH_SQL));
                var filesSQL = di.GetFiles("*.*");
                Write.Info("Paquetes para instalar:");

                hasSite = (filesSites.Count()> 0);
                hasSQL = (filesSQL.Count()> 0);

                Write.Info("- Archivos de Site: " + (hasSite ? "[X]":"[ ]"));     
                Write.Info("- Scripts de SQL:   " + (hasSQL ? "[X]":"[ ]"));     

            }
        }
        static bool ReadAppInfo()
        {
            var appInfo = Utils.JoinPath(rpkg.AppPath, PATH_INFO, "AppInfo.xml");
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
                rpkg.FileRpkg = args[1];
            }

            if (args[0].StartsWith("-c"))
            {
                action = "create";
                rpkg.AppPath = args[1];

            }

            //For Debug
            // action = "create";
            // rpkg.AppPath = @"F:\Temp\WSEmpleoAPI";

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