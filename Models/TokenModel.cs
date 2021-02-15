using System;
using System.Collections.Generic;

namespace LocalManipulator.Models
{
    public class GetTokenModelResult
    {
        public int Result { get; set; }
        public TokenModel Data { get; set; }
    }
    public class TokenModel
    {
        public string AccessToken { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public IEnumerable<int> Permissions { get; set; }
    }
}