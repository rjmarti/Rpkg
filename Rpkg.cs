using System;
using System.Configuration;

namespace Rpkg
{
    public class Rpkg
    {
        public string LogPath { get; set; }
        public string BackupPath { get; set; }
        public string OriginPath { get; set; }
        public string TargetPath { get; set; }
        public string TargetSQLServer { get; set; }
        public string FileRpkg { get; set; }
    }
}
