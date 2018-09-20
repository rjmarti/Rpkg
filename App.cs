using System;

namespace Rpkg
{
    public class App
    {
        public string Nombre { get; set; }
        public string Version { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string SQL { get; set; }
        public string Scripts { get; set; }
        public string ExcludeFiles { get; set; }
        public string ExcludeDirs { get; set; }
        public string FilePath { get; set; }
        public string PckgPath { get; set; }
        public string ScriptPath { get; set; }
        public string SQLPath { get; set; }
    }
}