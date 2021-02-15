using System;
using System.Collections;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading;
using LocalManipulator.Models;

namespace LocalManipulator
{
    class Program
    {
        private static readonly string TaskDictionary = "TasksForRobots";

        static void Main(string[] args)
        {
            string url;
            string user;
            string password;
            
            if(args.Length != 3)
                throw new ArgumentException("Parameters Url, User, Password is required!");
            url = args[0];
            user = args[1];
            password = args[2];

            var tokenResult = HttpHelper.Get<TokenModel>($"https://{url}/api/identity/getToken?login={user}&password={password}");

            if(string.IsNullOrEmpty(tokenResult.AccessToken))
                throw new AuthenticationException("Error login");

            while (true)
            {
                var tasks = HttpHelper.Get<GetTasksResult>($"https://{url}/api/{TaskDictionary}")
                    .Tasks;

                foreach (var task in tasks)
                {
                    var result = PythonWrapper.Run(task.Code);
                    var actualTask = HttpHelper.Get<TaskForRobot>($"https://{url}/api/{TaskDictionary}/getById/{task.Id}");
                    actualTask.Result = result;
                    HttpHelper.Post($"https://{url}/api/{TaskDictionary}/createOrUpdate", actualTask);
                }
                
                Thread.Sleep(10000);
            }
        }
    }
}
