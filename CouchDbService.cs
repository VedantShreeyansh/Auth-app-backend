using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using auth_app_backend.Model;

namespace auth_app_backend.Services
{
    public class CouchDbService
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        public CouchDbService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = "http://localhost:5984"; // Base URL for CouchDB

            // Set basic authentication for CouchDB (username: admin, password: password)
            var byteArray = Encoding.ASCII.GetBytes("admin:password");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task EnsureDatabaseExistsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/users");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var createResponse = await _httpClient.PutAsync($"{_baseUrl}/users", null);
                if (!createResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to create database: " + createResponse.ReasonPhrase);
                }
            }
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<User>(jsonData);
                }
                return null; // User not found
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving user: " + ex.Message);
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var selectorJson = $"{{\"selector\": {{\"email\": \"{email}\"}}}}";
            var content = new StringContent(selectorJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/users/_find", content);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<FindResult<User>>(jsonData);
                return result.Docs.FirstOrDefault();
            }
            return null;
        }

        public async Task AddUserAsync(User user)
        {
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/users", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to add user: {errorContent}");
            }
        }

        public async Task<List<User>> GetPendingUsersAsync()
        {
            var selectorJson = $"{{\"selector\": {{\"status\": \"Pending\"}}}}";
            var content = new StringContent(selectorJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/users/_find", content);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<FindResult<User>>(jsonData);
                return result.Docs;
            }
            return new List<User>();
        }

        public async Task UpdateUserAsync(User user)
        {
            // Fetch the latest _rev if not already set
            if (string.IsNullOrEmpty(user._rev))
            {
                var existingUser = await GetUserByIdAsync(user.Id.ToString());
                if (existingUser != null)
                {
                    user._rev = existingUser._rev;
                }
                else
                {
                    throw new Exception("User not found for update.");
                }
            }

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/users/{user.Id}?rev={user._rev}", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to update user: {errorContent}");
            }
        }

        public async Task<bool> UserIdExistsAsync(string id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/users/{id}");
            return response.IsSuccessStatusCode;
        }
    }

    public class FindResult<T>
    {
        [JsonProperty("docs")]
        public List<T> Docs { get; set; }
    }
}
