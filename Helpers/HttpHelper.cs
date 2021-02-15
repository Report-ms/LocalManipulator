using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using LocalManipulator.Models;

namespace LocalManipulator.Helpers
{
    internal class ClusterManagementAppConnector : IDisposable
    {
        private string _token;
        private string _baseUrl;
        private static readonly string TaskDictionary = "Tasks";
        private string AppUrl { get; }

        public ClusterManagementAppConnector(string appUrl, string user, string password)
        {
            AppUrl = appUrl;
            
            _token = Get<GetTokenModelResult>($"http://{AppUrl}/api/identity/getToken?login={user}&password={password}").Data.AccessToken;
            _baseUrl = $"http://{appUrl}/api/{TaskDictionary}";

        }
        public static T Get<T>(string url)
        {
            var result = new HttpClient()
                .GetStringAsync(url)
                .GetAwaiter()
                .GetResult();
            Console.WriteLine(url);
            Console.WriteLine(result);
            var deserialize = JsonSerializer.Deserialize<T>(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (deserialize == null)
            {
                throw new Exception("Error Deserialize");
            }
            return deserialize;
        }
        public static T Get<T>(string token, string url)
        {
            Console.WriteLine(token);
            Console.WriteLine(url);
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
                Console.WriteLine(responseFromServer);
                return JsonSerializer.Deserialize<T>(responseFromServer, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
        }

        public static string Post<T>(string token, string url, T obj)
        {
            Console.WriteLine(token);
            Console.WriteLine(url);
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
                Console.WriteLine(responseFromServer);
            }
            
            return serialize;
        }

        public void Dispose()
        {
        }

        public void SaveResult(TaskForRobot task, string result)
        {
            var actualTask = Get<TaskForRobot>(_token, $"{_baseUrl}/getById/{task.Id}");
            actualTask.Result = result;
            
            Post(_token, $"{_baseUrl}/createOrUpdate", actualTask);
        }

        public IEnumerable<TaskForRobot> GetTasks()
        {
            var tasks = Get<IEnumerable<TaskForRobot>>(_token, $"{_baseUrl}/getList");
            return tasks;
        }
    }
}