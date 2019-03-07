using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace SimpleAuth
{
    public class AuthStorage : IAuthStorage
    {
        public void SetSecured(string identifier, string value, string clientId, string clientSecret, string sharedGroup) => throw new NotImplementedException("Please implement IAuthStorage and register it with SimpleAuth.Resolver");

        public string GetSecured(string identifier, string clientId, string clientSecret, string sharedGroup) => throw new NotImplementedException("Please implement IAuthStorage and register it with SimpleAuth.Resolver");
    }
}
