using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LocalManipulator.Helpers;

namespace LocalManipulator
{
    class Program
    {
        static void Main()
        {
            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("appsettings.json"));
            var pythonWrapper = new PythonWrapper(settings);
            using (var app = new ClusterManagementAppConnector(settings))
            {
                while (true)
                {
                    try
                    {
                        var tasks = app.GetTasks();
                        foreach (var task in tasks)
                        {
                            app.SetRunning(task);
                            var result = pythonWrapper.Run(task.Code);
                            app.SaveResult(task, result);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        //app.SetError(task, e);
                    }
                    Thread.Sleep(1000);
                }
            }
        }
    }
}