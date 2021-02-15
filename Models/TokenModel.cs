using System.Collections.Generic;

namespace LocalManipulator.Models
{
    public class TokenModel
    {
        public string AccessToken { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public IEnumerable<int> Permissions { get; set; }
    }
}