using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using LocalManipulator.Models;

namespace LocalManipulator.Helpers
{
    public class ClusterManagementAppConnector : IDisposable
    {
        private string _token;
        private DateTime LastTokenRefresh { get; set; }
        private string _baseUrl;
        private string _viewName;
        private Settings settings;
        private static string TaskDictionary = "Tasks";
        private string AppUrl { get; }


        public ClusterManagementAppConnector(Settings settings)
        {
            this.settings = settings;
            var uri = new Uri(settings.TasksUrl);
            AppUrl = settings.TasksUrl.Replace(uri.AbsolutePath, "");
            _viewName  = uri.PathAndQuery.Split("/getListForView/")[1];

            var tokenIsReceive = false;
            var attemptCount = 0;
            while (!tokenIsReceive)
            {
                try
                {
                    attemptCount++;
                    RefreshToken();
                    TaskDictionary = Get<AppConfig>(_token, $"{AppUrl}/api/configuration")
                        .Views
                        .Single(x=>x.ViewName == _viewName)
                        .DictionaryStrictName;
                    tokenIsReceive = true;
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    var delay = 10000;
                    
                    if (attemptCount > 10)
                        delay = 30000;
                    
                    if (attemptCount > 20)
                        delay = 90000;
                    
                    Thread.Sleep(delay);
                }
            }
            _baseUrl = $"{AppUrl}/api/{TaskDictionary}";
        }

        public void RefreshToken()
        {
            Console.WriteLine($"{AppUrl}/api/identity/getToken?login={settings.UserName}&password={settings.UserPassword}");
            _token = Get<GetTokenModelResult>($"{AppUrl}/api/identity/getToken?login={settings.UserName}&password={settings.UserPassword}").Data.AccessToken;
            LastTokenRefresh = DateTime.Now;
        }

        public static T Get<T>(string url)
        {
            var result = new HttpClient()
                .GetStringAsync(url)
                .GetAwaiter()
                .GetResult();
            Console.WriteLine($"{DateTime.UtcNow}: {url}");
            Console.WriteLine($"{DateTime.UtcNow}: {result}");
            var deserialize = JsonSerializer.Deserialize<T>(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (deserialize == null)
            {
                throw new Exception("Error Deserialize");
            }
            return deserialize;
        }

        public static T Get<T>(string token, string url)
        {
            //Console.WriteLine($"{DateTime.UtcNow}: {url}");
            //Console.WriteLine($"{DateTime.UtcNow}: {token}");
            var webRequest = HttpWebRequest.Create(url);
            webRequest.Headers.Add("Authorization", "Bearer " + token);
            
            var webResponse = webRequest.GetResponse();
            using (var dataStream = webResponse.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                //Console.WriteLine($"{DateTime.UtcNow} {responseFromServer}");
                var deserialize = JsonSerializer.Deserialize<T>(responseFromServer, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                return deserialize;
            }
        }

        public static string Post<T>(string token, string url, T obj)
        {
            //Console.WriteLine($"{DateTime.UtcNow}: {url}");
            //Console.WriteLine($"{DateTime.UtcNow}: {token}");
            var webRequest = HttpWebRequest.Create(url);
            webRequest.Headers.Add("Authorization", "Bearer " + token);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            
            var serialize = JsonSerializer.Serialize<T>(obj);
            
            Stream requestStream = webRequest.GetRequestStream();
            var data = Encoding.ASCII.GetBytes(serialize);
            requestStream.Write(data);

            var webResponse = webRequest.GetResponse();
            
            using (var dataStream = webResponse.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                //Console.WriteLine($"{DateTime.UtcNow}: {responseFromServer}");
            }
            
            return serialize;
        }

        public void Dispose()
        {
        }

        public void SaveResult(TaskForRobot task, string result)
        {
            var actualTask = Get<TaskForRobot>(_token, $"{_baseUrl}/getById/{task.Id}");
            actualTask.Result = (actualTask.Result ?? "") +  result;
            actualTask.State = "FinishedWithSuccess";
            
            Post(_token, $"{_baseUrl}/createOrUpdate", actualTask);
        }

        public IEnumerable<TaskForRobot> GetTasks()
        {
            if((DateTime.Now - LastTokenRefresh) > new TimeSpan(10,0,0))
                RefreshToken();
            var tasks = Get<GetItemsOf<TaskForRobot>>(_token, $"{_baseUrl}/getListForView/{_viewName}/0/30");
            return tasks.Rows;
        }

        public void SetRunning(TaskForRobot task)
        {
            var actualTask = Get<TaskForRobot>(_token, $"{_baseUrl}/getById/{task.Id}");
            actualTask.State = "Running";
            
            Post(_token, $"{_baseUrl}/createOrUpdate", actualTask);
        }

        public void SetError(TaskForRobot task, Exception exception)
        {
            var actualTask = Get<TaskForRobot>(_token, $"{_baseUrl}/getById/{task.Id}");
            actualTask.Result = $"Exception: {exception}";
            actualTask.State = "FinishedWithError";
            
            Post(_token, $"{_baseUrl}/createOrUpdate", actualTask);
        }
    }
}