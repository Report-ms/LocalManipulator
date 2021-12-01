using System;
using System.Diagnostics;
using System.IO;

namespace LocalManipulator.Helpers
{
    internal class PythonWrapper
    {
        public string PythonLocation { get; set; }
        public string TempFolder { get; set; }

        public PythonWrapper(Settings settings)
        {
            PythonLocation = settings.PythonLocation;
            TempFolder = settings.TempFolder;
        }

        public string Run(string code)
        {
            var fileName = $"{TempFolder}/{Guid.NewGuid()}.py";
            var streamWriter = File.CreateText(fileName);
            streamWriter.Write(code);
            streamWriter.Close();

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(PythonLocation, fileName)
                {
                    RedirectStandardOutput = true, 
                    RedirectStandardError = true, 
                    UseShellExecute = false, 
                    CreateNoWindow = true,
                }
            };
            p.Start();
            
            p.WaitForExit();
            string output = p.StandardOutput.ReadToEnd();
            string err = p.StandardError.ReadToEnd();

            Console.WriteLine($"{DateTime.UtcNow} [code]: {code}");
            Console.WriteLine($"{DateTime.UtcNow} [output]: {output}");
            Console.WriteLine($"{DateTime.UtcNow} [err]: {err}");
            
            return output + err;
        }
    }
}