using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace auth_app_backend
{
    public class CouchDbService
    {
        private readonly string _dbUrl;
        private readonly string _dbName;
        private readonly HttpClient _httpClient;

        public CouchDbService(IConfiguration configuration)
        {
            _dbUrl = "http://localhost:5984/"; // CouchDB server URL
            _dbName = "users"; // CouchDB database name
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_dbUrl)
            };

            // Set the authentication header
            var byteArray = Encoding.ASCII.GetBytes("admin:password"); // Update with actual credentials
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        // Method to Add User to CouchDB
        public async Task AddUserAsync(Model.User user)
        {
            var jsonContent = JsonConvert.SerializeObject(user);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync($"{_dbName}/{user.Email}", content);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw; // Rethrow to handle it higher up, if needed
            }
        }

        public class CouchDbFindResponse<T>
        {
            public List<T> Docs { get; set; }
        }

        // Method to Retrieve User by Email
        public async Task<Model.User> GetUserByEmailAsync(string email)
        {
            var findQuery = new
            {
                selector = new
                {
                    Email = email
                },
                fields = new[] { "_id", "_rev", "FirstName", "LastName", "Role", "Password", "Email", "IsApproved" },
                limit = 1,
                execution_stats = true
            };

            var jsonQuery = JsonConvert.SerializeObject(findQuery);
            var content = new StringContent(jsonQuery, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_dbName}/_find", content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error retrieving user: " + response.StatusCode);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CouchDbFindResponse<Model.User>>(jsonResponse);

                return result?.Docs?.FirstOrDefault();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }
    }
}
