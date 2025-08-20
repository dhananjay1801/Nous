using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nous.Utils
{
    internal class Logger
    {
            public static string Path = @"E:\College Project\Min_Nous\Nous\Log.txt";

            public static void SWrite(string message)
            {
                var culture = new CultureInfo("en-GB");
                try
                {
                    if (!File.Exists(Path))
                        File.Create(Path).Dispose();

                    using (StreamWriter sw = File.AppendText(Path))
                    {
                        sw.WriteLine($"[+] [{DateTime.Now.ToString(culture)}] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Logger Error: {ex.Message}");
                }
            }
            public static void EWrite(string message)
            {
                var culture = new CultureInfo("en-GB");
                try
                {
                    if (!File.Exists(Path))
                        File.Create(Path).Dispose();

                    using (StreamWriter sw = File.AppendText(Path))
                    {
                        sw.WriteLine($"[-] [{DateTime.Now.ToString(culture)}] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Logger Error: {ex.Message}");
                }
            }
        }




}

