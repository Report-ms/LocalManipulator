using System.Net.Http;
using System.Text.Json;

namespace LocalManipulator
{
    internal class HttpHelper
    {
        public static T Get<T>(string url)
        {
            return JsonSerializer.Deserialize<T>(new HttpClient()
                .GetStringAsync(url)
                .GetAwaiter()
                .GetResult());
        }

        public static string Post<T>(string url, T obj)
        {
            var serialize = JsonSerializer.Serialize<T>(obj);
            
            return serialize;
        }
    }
}