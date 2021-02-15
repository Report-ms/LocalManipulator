using System;
using System.Security.Authentication;
using System.Threading;
using LocalManipulator.Helpers;
using LocalManipulator.Models;

namespace LocalManipulator
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 3)
                throw new ArgumentException("Parameters Url, User, Password is required!");
            
            var url = args[0];
            var user = args[1];
            var password = args[2];

            using (var app = new ClusterManagementAppConnector(url, user, password))
            {
                while (true)
                {
                    var tasks = app.GetTasks();
                    foreach (var task in tasks)
                    {
                        var result = PythonWrapper.Run(task.Codetoexecute);
                        app.SaveResult(task, result);
                    }
                    Thread.Sleep(10000);
                }
            }
        }
    }
}