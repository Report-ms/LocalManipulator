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
                    UseShellExecute = false, 
                    CreateNoWindow = true
                }
            };
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            Console.WriteLine($"{DateTime.UtcNow}: {output}");
            
            return output;
        }
    }
}