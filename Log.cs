using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TShockAPI;

namespace ChestControl
{
    static class Log
    {
        private static string filename;
        private static StreamWriter logWriter;

        public static void Initialize(string filename, bool clear)
        {
            filename = filename;
            logWriter = new StreamWriter(filename, !clear);
        }

        public static void Write(String message, LogLevel level, Boolean consolewrite = true)
        {
            string caller = "ChestControl";

            StackFrame frame = new StackTrace().GetFrame(2);
            if (frame != null)
            {
                var meth = frame.GetMethod();
                if (meth != null)
                    if (meth.DeclaringType != null) 
                        caller = meth.DeclaringType.Name;
            }

            logWriter.WriteLine(string.Format("{0} - {1}: {2}: {3}",
                                               DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                                               caller, level.ToString().ToUpper(), message));
            logWriter.Flush();
            if (consolewrite)
                Console.WriteLine(string.Format("{0}: {1}: {2}",
                    caller, level.ToString().ToUpper(), message));
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Data
    }
}
