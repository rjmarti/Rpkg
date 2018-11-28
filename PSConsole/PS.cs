using System.IO;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace Rpkg.PSConsole
{
    public static class PS
    {
        public static string ExecutePS(this string script)
        {
            script = PreProcess(script);
            var sb = new StringBuilder(string.Format("> {0}\r", script));

            powerShell.Commands.Clear();
            powerShell.AddScript(script);
            powerShell.AddCommand("Out-String");
            powerShell.AddParameter("Width", 133);

            try
            {
                var results = powerShell.Invoke();
                if (powerShell.Streams.Error.Count > 0)
                {
                    foreach (var err in powerShell.Streams.Error)
                    {
                        AddErrorInfo(sb, err);
                    }
                    powerShell.Streams.Error.Clear();
                }
                else
                {
                    foreach (var item in results)
                    {
                        sb.Append(item);
                    }
                }
            }
            catch (System.Exception ex)
            {
                sb.Append(ex.Message);
            }

            powerShell.Commands.Clear();
            return sb.ToString();
        }

        private static PowerShell _powerShell;

        private static PowerShell powerShell
        {
            get
            {
                if (_powerShell == null)
                {
                    _powerShell = PowerShell.Create();
                    powerShell.Runspace = PSConfig.GetPSConfig;
                    if (!string.IsNullOrEmpty(PSConfig.Profile) && File.Exists(PSConfig.Profile))
                    {
                        var script = File.ReadAllText(PSConfig.Profile);
                        _powerShell.AddScript(script);
                        _powerShell.Invoke();
                        powerShell.Commands.Clear();
                    }
                }

                return _powerShell;
            }
        }

        private static void AddErrorInfo(StringBuilder sb, ErrorRecord err)
        {
            sb.Append(err.ToString());
            sb.AppendFormat("\r\n   +{0}", err.InvocationInfo.PositionMessage);
            sb.AppendFormat("\r\n   + CategoryInfo          :{0}", err.CategoryInfo);
            sb.AppendFormat("\r\n   + FullyQualifiedErrorId :{0}", err.FullyQualifiedErrorId.ToString());
            sb.AppendLine();
        }

        private static string PreProcess(string script)
        {
            string[] lines = Regex.Split(script, "\r\n");
            StringBuilder sb = new StringBuilder();

            foreach (string line in lines)
            {
                var l = line;
                if (line.Contains("Read-Host") && !line.Contains("#"))
                {
                    var idxEqual = line.IndexOf('=');
                    var idxReadHost = line.IndexOf("Read-Host");
                    var idxLabel = (line.IndexOf("'") > 0) ? line.IndexOf("'") : 0;
                    if (idxLabel == 0)
                    {
                        idxLabel = (line.IndexOf('"') > 0) ? line.IndexOf('"') : 0;
                    }
                    var q = line.Substring(idxLabel, line.Length - idxLabel);
                    var rc = Write.Question(q);
                    sb.AppendLine(line.Substring(0, idxEqual) + " = '" + rc + "'");
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }
    }
}
