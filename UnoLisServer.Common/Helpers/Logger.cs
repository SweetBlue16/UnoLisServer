using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.IO;
using UnoLisServer.Common.Constants;

namespace UnoLisServer.Common.Helpers
{
    public static class Logger
    {
        private static readonly string logFile = Constants.Constants.LogFileName;

        public static void Log(string message)
        {
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(entry);
            try
            {
                File.AppendAllText(logFile, entry + Environment.NewLine);
            }
            catch (IOException)
            {
                Console.WriteLine("⚠️ Error al escribir en el archivo de log.");
            }
        }
    }
}
