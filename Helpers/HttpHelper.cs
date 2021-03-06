﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private string _viewName;
        private static string TaskDictionary = "Tasks";
        private string AppUrl { get; }

        public ClusterManagementAppConnector(Settings settings)
        {
            var uri = new Uri(settings.TasksUrl);
            AppUrl = settings.TasksUrl.Replace(uri.AbsolutePath, "");
            _viewName  = uri.PathAndQuery.Split("View")[1];
            
            
            _token = Get<GetTokenModelResult>($"{AppUrl}/api/identity/getToken?login={settings.UserName}&password={settings.UserPassword}").Data.AccessToken;
            TaskDictionary = Get<AppConfig>(_token, $"{AppUrl}/api/configuration")
                .Views
                .Single(x=>x.ViewName == _viewName)
                .DictionaryStrictName;
            _baseUrl = $"{AppUrl}/api/{TaskDictionary}";
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
            Console.WriteLine($"{DateTime.UtcNow}: {url}");
            Console.WriteLine($"{DateTime.UtcNow}: {token}");
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
                var deserialize = JsonSerializer.Deserialize<T>(responseFromServer, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                return deserialize;
            }
        }

        public static string Post<T>(string token, string url, T obj)
        {
            Console.WriteLine($"{DateTime.UtcNow}: {url}");
            Console.WriteLine($"{DateTime.UtcNow}: {token}");
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
                Console.WriteLine($"{DateTime.UtcNow}: {responseFromServer}");
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
            actualTask.State = "FinishedWithSuccess";
            
            Post(_token, $"{_baseUrl}/createOrUpdate", actualTask);
        }

        public IEnumerable<TaskForRobot> GetTasks()
        {
            var tasks = Get<IEnumerable<TaskForRobot>>(_token, $"{_baseUrl}/getListForView/{_viewName}");
            return tasks;
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