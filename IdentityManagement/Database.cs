using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IdentityManagement
{
    public class Database
    {
        // GET USER HASH
        private static string UserHash(string username)
        {
            return Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(username)));
        }

        // GET USER FROM FILE
        public async Task<User?> GetUserAsync(string username)
        {
            var hash = UserHash(username);
            if (!File.Exists(hash))
            {
                return null;
            }

            await using var reader = File.OpenRead(hash);

            //var options = new JsonSerializerOptions
            //{
            //    ReadCommentHandling = JsonCommentHandling.Skip,
            //    AllowTrailingCommas = true,
            //};

            // Reads UTF-8 encoded text representing single JSON value
            // into instance of type specified by generic type parameter
            //return await JsonSerializer.DeserializeAsync<User>(reader, options);
            return await JsonSerializer.DeserializeAsync<User>(reader);
        }

        // WRITE USER TO FILE
        public async Task PutAsync(User user)
        {
            var hash = UserHash(user.Username);
            await using var writer = File.OpenWrite(hash);

            // Converts value of type specified by generic type parameter
            // to UTF-8 encoded JSON text and write it to stream
            await JsonSerializer.SerializeAsync(writer, user);
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public List<UserClaim> Claims { get; set; } = new();
    }

    public class UserClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
