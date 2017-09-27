using System;
using IDS.Banking.Abstraction;
using Newtonsoft.Json;
using AgileFusion.Banking.Services.Utils;
using System.Security.Cryptography;
using IDS.ComponentModel;

namespace AgileFusion.Banking.Services.Authentication
{
    public class SecurityTokenService : ISecurityTokenService
    {
        private const string HEADER = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";

        [ComponentSetting("User provider for Host System", "Provider to retrieve user form host system", "Services", WellKnownComponent = true)]
        public IHostSystemUserProvider UserProvider { get; set; }

        [ComponentSetting("Token Db Provider", "Db Provider for security tokens for AgileFusion apps", "Services", WellKnownComponent = true)]
        public IUserTokenDbProvider TokenProvider { get; set; }

        [ComponentSetting("Token Lifetime", "Tokens lifetime in days, after which it's expired", "Settings")]
        public int TokenLifetime { get; set; } = 30;
        public void DeleteKeyForUser(string userId)
        {
            TokenProvider.DeleteUserKey(userId);
        }

        public string GenerateToken(string userId)
        {
            var token = new SecurityTokenPayload { Exp = DateTime.UtcNow.AddDays(TokenLifetime), Sub = userId }.ToString();
            var key = GetUsersKey(userId);
            var tokenBase64 = token.ToBase64();
            var stringToken = HEADER.ToBase64() + '.' + tokenBase64 + '.' + HashWithKey(tokenBase64, key);
            return stringToken;
        }

        public IAccountHolder GetUser(string token)
        {
            string userId = ExtractUserId(token);
            if(string.IsNullOrEmpty(userId))
            {
                return null;
            }
            var loginName = TokenProvider.GetLoginName(userId);
            return UserFromHostSystem(loginName, userId);
        }

        public string ExtractUserId(string token)
        {
            string userId = null;
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }
            var parts = token.Split('.');
            if(parts.Length != 3)
            {
                return null;
            }
            try
            {
                var securityToken = JsonConvert.DeserializeObject<SecurityTokenPayload>(parts[1].FromBase64());
                if (securityToken.Exp < DateTime.UtcNow)
                {
                    return null;
                }
                userId = securityToken.Sub;
            }
            catch (Exception ex)
            {
                return null;
            }
            var userKey = GetUsersKey(userId);
            var hashed = HashWithKey(parts[1], userKey);
            if(hashed != parts[2])
            {
                return null;
            }
            return userId;
        }

        private byte[] GetUsersKey(string userId)
        {
            var key = TokenProvider.GetUserKey(userId);
            if (key != null) { return key; }
            key = GenerateKey();
            TokenProvider.InsertUserKey(userId, key);
            return key;
        }

        private byte[] GenerateKey()
        {
            using(var generator = new RNGCryptoServiceProvider())
            {
                var key = new byte[64];
                generator.GetBytes(key);
                return key;
            }
        }

        private IAccountHolder UserFromHostSystem(string loginName, string userId)
        {
            return UserProvider.GetUser(loginName, userId);
        }

        private static string HashWithKey(string str, byte[] key)
        {
            using (var hmac = new HMACSHA256(key))
            {
                var bytes = hmac.ComputeHash(str.ToBytes());
                return Convert.ToBase64String(bytes);
            }
        }
        private class SecurityTokenPayload
        {
            public DateTime Exp { get; set; }
            public string Sub { get; set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
    }
    
}
