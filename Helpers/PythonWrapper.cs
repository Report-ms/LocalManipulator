using System;
using System.Diagnostics;
using System.IO;

namespace LocalManipulator.Helpers
{
    internal class PythonWrapper
    {
        public static string Run(string code)
        {
            var fileName = @"C:\temp\" + Guid.NewGuid().ToString()  + ".py";
            var streamWriter = File.CreateText(fileName);
            streamWriter.Write(code);
            streamWriter.Close();

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(@"C:\Python38\python.exe", fileName)
                {
                    RedirectStandardOutput = true, 
                    UseShellExecute = false, 
                    CreateNoWindow = true
                }
            };
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            Console.WriteLine(output);
            
            return output;
        }
    }
}