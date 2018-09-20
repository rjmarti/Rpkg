using System;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace Rpkg
{
    class Utils
    {
        public static string JoinPath(params string[] paths)
        {
            var p0 = "";
            var p1 = String.Join(@"\", paths);
            var p2 = "";
            if (p1.StartsWith(@"\\"))
            {
                p0 = @"\\";
                p2 = p1.Substring(2);
            }
            else
            {
                p2 = p1;
            }

            while (p2.Contains(@"\\"))
            {
                p2 = p2.Replace(@"\\", @"\");
            }

            return p0 + p2;
        }

        public static void CreateDirSafe(string path)
        {
            Write.Info("Create: {0}", path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static void DeleteDirContentSafe(string path)
        {
            Write.Info("Delete: " + path + @"\*.*");

            foreach (string dir in Directory.GetDirectories(path))
            {
                Directory.Delete(dir, true);
            }

            foreach (string file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }


        }

        public static bool CopyFiles(string origen,
                                     string destino,
                                     bool mirror,
                                     string excludeDir,
                                     string excludeFiles,
                                     string log
                                     )
        {
            var rc = true;
            //--- Copia de archivos....
            Write.Info("");
            Write.Info("Copiando Archivos...");
            Write.Label("Origen", origen);
            Write.Label("Destino", destino);

            var cmdExec = String.Format("robocopy {0} {1} /s ", origen, destino);

            if (mirror)
            {
                cmdExec += "/mir /is /if ";
            }

            if (!String.IsNullOrWhiteSpace(excludeDir))
            {
                cmdExec += "/xd " + excludeDir + " ";
            }

            if (!String.IsNullOrWhiteSpace(excludeFiles))
            {
                cmdExec += "/xf " + excludeFiles + " ";
            }

            if (!String.IsNullOrWhiteSpace(log))
            {
                cmdExec += "/log:\"" + log + "\"";
            }

            Write.Info(cmdExec);

            CmdExec(cmdExec);

            Write.Info("Archivos copiados.");

            return rc;
        }

        public static bool ZipDir(string origen, string destino)
        {
            var rc = false;
            try
            {
                Write.Info("");
                Write.Info("Zipeando Archivos...");
                Write.Label("Origen", origen);
                Write.Label("Destino", destino);
                ZipFile.CreateFromDirectory(origen, destino);
            }
            catch
            {
                rc = false;
            }

            return rc;
        }

        public static bool UnZipPkg(string fileOrigen, string destino)
        {
            var rc = false;
            try
            {
                Write.Info("");
                Write.Info("Desempaquetando...");
                Write.Label("Origen", fileOrigen);
                Write.Label("Destino", destino);
                ZipFile.CreateFromDirectory(fileOrigen, destino);
            }
            catch
            {
                rc = false;
            }

            return rc;
        }

        public static void CmdExec(string cmdExec)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + cmdExec;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
    }
}