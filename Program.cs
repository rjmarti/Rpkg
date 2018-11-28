using System;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml;
using Rpkg.PSConsole;
using System.Collections.Generic;

namespace Rpkg
{
    class Program
    {
        private const string PATH_INFO = "_Info";
        private const string PATH_TASK = "_Task";
        private const string PATH_SITE = "_Site";
        private const string PATH_SQL = "_SQL";
        private const string PATH_CONFIG = "_Config";
        private const string PATH_TEMP = "_Temp";
        private const string AMB_DESA = "DESA";
        private const string AMB_TEST = "TEST";
        private const string AMB_HOMO = "HOMO";
        private const string AMB_PROD = "PROD";
        private static bool hasSite = false;
        private static bool hasSQL = false;
        private static bool hasConfig = false;
        private static string action;
        private static Rpkg rpkg;
        private static App app;
        static void Main(string[] args)
        {
            rpkg = new Rpkg();
            app = new App();

            ReadAllSettings();
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
            ExecuteTask(Utils.JoinPath(rpkg.OriginPath, PATH_TASK));
        }

        static void InstallRpkg()
        {
            if (!File.Exists(rpkg.FileRpkg))
            {
                Write.Error("Archivo {0} no encontrado.",rpkg.FileRpkg );
                return;
            }
            rpkg.OriginPath = @".\" + PATH_TEMP;
            Utils.CreateDirSafe(rpkg.OriginPath);
            Utils.UnZipPkg(rpkg.FileRpkg, rpkg.OriginPath);
            
            Write.Info("");
            ReadAppInfo();
            Info();
            SelectTargetAmb();

            if (hasSite)
            {
                if(Write.QuestionSN("Desea instalar el sitio? Destino: " + rpkg.TargetPath))
                {
                    ExecuteTask(Utils.JoinPath(rpkg.OriginPath, PATH_TASK, PATH_SITE));
                }
            }

            if (hasSQL)
            {
                if(Write.QuestionSN("Desea ejecutar scripts SQL?"))
                {
                     ExecuteTask(Utils.JoinPath(rpkg.OriginPath, PATH_TASK, PATH_SQL));
                }
            }

            if (hasConfig)
            {
                if(Write.QuestionSN("Desea ejecutar scripts de configuracion?"))
                {
                     ExecuteTask(Utils.JoinPath(rpkg.OriginPath, PATH_TASK, PATH_CONFIG));
                }
            }
        }
        static void ExecuteTask(string taskPath)
        {
            
            var di = new DirectoryInfo(taskPath);
            var files = di.GetFiles("*.ps1").OrderBy(f => f.Name);
            Write.Label("TargetPath: ", rpkg.TargetPath);
            Write.Label("OriginPath: ", rpkg.OriginPath);
            PSConfig.AddVariable("TargetSQLServer", rpkg.TargetSQLServer);
            PSConfig.AddVariable("TargetPath", rpkg.TargetPath);
            PSConfig.AddVariable("OriginPath", rpkg.OriginPath);
            PSConfig.AddVariable("AppName", app.Nombre);
            PSConfig.AddVariable("AppVersion", app.Version);
            foreach (var file in files)
            {
                Write.Label("Ejecutando: ", file.FullName);
                var script = File.ReadAllText(file.FullName);
                //script = script.Replace("{rpkg.OriginPath}",rpkg.OriginPath);
                Console.WriteLine(PS.ExecutePS(script));
            }
        }
        static void SelectTargetAmb()
        {
            XmlDocument doc = OpenFileAppInfo();
            Write.Info("Ambientes disponibles:");
            Dictionary<string, string> dictAmbs = new Dictionary<string, string>();
            var ambs = new string[] { AMB_DESA, AMB_TEST, AMB_HOMO, AMB_PROD};
            var rc_values = "";
            foreach (var amb in ambs) {
                if (doc.SelectSingleNode("//appSettings/add[@key='SitePath_"+amb+"']/@value").Value.Length > 0) 
                {
                    var key = amb.Substring(0,1).ToLower();

                    dictAmbs.Add(key, amb);
                    Write.Label(amb, key);
                    rc_values += key +"/";
                }
            }
            var rc = Write.QuestionMulti("Seleccione el ambiente destino:", rc_values.Substring(0,rc_values.Length-1));
            Write.Label("seleccionó: ", dictAmbs[rc]);

            rpkg.TargetPath =  doc.SelectSingleNode("//appSettings/add[@key='SitePath_"+dictAmbs[rc]+"']/@value").Value;
            rpkg.TargetSQLServer =  doc.SelectSingleNode("//appSettings/add[@key='SQLServer_"+dictAmbs[rc]+"']/@value").Value;

        }
        static void Info()
        {
            Write.Default();
            Console.Clear();
            // Write.Info("==================================");
            // Write.Info(@"   ___    ___           ___      ");
            // Write.Info(@"  |   \  |   \  |   /  /   \     ");
            // Write.Info(@"  |___/  |___/  |__/   |  _      "); 
            // Write.Info(@"  |  \   |      |  \   |   |     "); 
            // Write.Info(@"  |   \  |      |   \  \___/     "); 
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
                var pathSite = Utils.JoinPath(rpkg.OriginPath, PATH_SITE);
                if (Directory.Exists(pathSite)){
                    var di = new DirectoryInfo(pathSite);
                    var filesSites = di.GetFiles("*.*");
                    hasSite = (filesSites.Count() > 0);
                }

                var pathSQL = Utils.JoinPath(rpkg.OriginPath, PATH_SQL);
                if (Directory.Exists(pathSQL)){
                    var di = new DirectoryInfo(pathSQL);
                    var filesSQL = di.GetFiles("*.*");
                    hasSQL = (filesSQL.Count() > 0);
                }

                var pathConfig = Utils.JoinPath(rpkg.OriginPath, PATH_TASK, PATH_CONFIG);
                if (Directory.Exists(pathConfig)){
                    var di = new DirectoryInfo(pathConfig);
                    var filesSQL = di.GetFiles("*.*");
                    hasConfig = (filesSQL.Count() > 0);
                }


                Write.Info("Paquetes para instalar:");

                Write.Info("- Archivos de Site:  " + (hasSite ? "[X]" : "[ ]"));
                Write.Info("- Scripts de SQL:    " + (hasSQL ? "[X]" : "[ ]"));
                Write.Info("- Scripts de Config.:" + (hasConfig ? "[X]" : "[ ]"));

            }
            Write.Info("----------------------------------");
        }
        static bool ReadAppInfo()
        {
            XmlDocument doc = OpenFileAppInfo();

            if (doc == null)
            {
                Write.Error("No se pudo abrir AppInfo.");
                return false;
            }

            try
            {
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
                rpkg.FileRpkg = Utils.JoinPath(Directory.GetCurrentDirectory(), args[1]);
            }

            if (args[0].StartsWith("-c"))
            {
                action = "create";
                rpkg.OriginPath = Utils.JoinPath(rpkg.OriginPath, args[1]);

            }

            return true;
        }
        static void ReadAllSettings()
        {
            try
            {
                rpkg.LogPath = ConfigurationManager.AppSettings["LogPath"];
                rpkg.BackupPath = ConfigurationManager.AppSettings["BackupPath"];
                rpkg.OriginPath = ConfigurationManager.AppSettings["OriginPath"];
                
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
        static private XmlDocument OpenFileAppInfo()
        {
            var appInfo = Utils.JoinPath(rpkg.OriginPath, PATH_INFO, "AppInfo.xml");
            
            if (!File.Exists(appInfo))
            {
                Write.Error("Archivo: {0} no encontrado.", appInfo);
                return null;
            }

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(appInfo);

            }
            catch (Exception ex)
            {
                Write.Error("Error: {0}", ex.Message);
            }
            return doc;
        }
    }
}
