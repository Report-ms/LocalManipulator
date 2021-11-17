using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LocalManipulator.Helpers;
using LocalManipulator.Models;

namespace LocalManipulator
{
    public class ThreadWork
    {
        public static void DoWork(object context)
        {
            var app = ((Context)context).App;
            var task = ((Context)context).Task;
            var settings = ((Context)context).Settings;
            var pythonWrapper = new PythonWrapper(settings);

            var result = pythonWrapper.Run(task.Code);
            app.SaveResult(task, result);
        }
    }
    
    class Program
    {
        static void Main()
        {
            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("appsettings.json"));
            TaskForRobot lastTask = null;
            using (var app = new ClusterManagementAppConnector(settings))
            {
                while (true)
                {
                    try
                    {
                        var tasks = app.GetTasks().Where(x=>x.State == "Created");
                        foreach (var task in tasks) 
                            app.SetRunning(task);
                        foreach (var task in tasks)
                        {
                            new Thread(ThreadWork.DoWork).Start(new Context(settings, app, task));
                            lastTask = task;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        if(lastTask != null)
                            try
                            {
                                app.SetError(lastTask, e);
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine("Error on set error");
                            }
                    }
                    Thread.Sleep(1000);
                }
            }
        }
    }

    public class Context
    {
        public Settings Settings { get; }
        public ClusterManagementAppConnector App { get; }
        public TaskForRobot Task { get; }

        public Context(Settings settings, ClusterManagementAppConnector app, TaskForRobot task)
        {
            Settings = settings;
            App = app;
            Task = task;
        }
    }
}