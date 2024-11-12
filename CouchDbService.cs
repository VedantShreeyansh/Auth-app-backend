using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using auth_app_backend.Model;
using System.Net;

namespace auth_app_backend.Services
{
    public class CouchDbService
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        public CouchDbService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = "http://localhost:5984";

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
                    var user = JsonConvert.DeserializeObject<User>(jsonData);
                    return user;
                }
                return null;
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
                throw new Exception("Failed to add user.");
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null || string.IsNullOrEmpty(user._id))
            {
                throw new ArgumentException("User data is invalid or missing.");
            }

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                var currentUser = await GetUserByIdAsync(user._id);

                if (currentUser == null)
                {
                    throw new Exception("User not found.");
                }

<<<<<<< HEAD
                user._rev = currentUser._rev;  // Ensure the latest _rev is used
=======
                user._rev = currentUser._rev;
>>>>>>> 36499e728e35cd3a78ae3c8e08b8183a639edb05

                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/users/{user._id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return;
                }

<<<<<<< HEAD
=======
                // If there’s a conflict, retry after a small delay
>>>>>>> 36499e728e35cd3a78ae3c8e08b8183a639edb05
                if (response.StatusCode == HttpStatusCode.Conflict && attempt < 3)
                {
                    await Task.Delay(200); // Delay between retries
                    continue;
                }

<<<<<<< HEAD
=======
                // Get detailed error message and throw an exception if retries exhausted
>>>>>>> 36499e728e35cd3a78ae3c8e08b8183a639edb05
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update user after {attempt} attempts: {response.ReasonPhrase}, Details: {errorContent}");
            }
        }

<<<<<<< HEAD

=======
>>>>>>> 36499e728e35cd3a78ae3c8e08b8183a639edb05
        public async Task<bool> UserIdExistsAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/users/{userId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<User>> GetPendingUsersAsync()
        {
            var selectorJson = "{\"selector\": {\"status\": \"Pending\"}}";
            var content = new StringContent(selectorJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/users/_find", content);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<FindResult<User>>(jsonData);
                return result.Docs;
            }
            return Enumerable.Empty<User>();
        }
    }

    public class FindResult<T>
    {
        [JsonProperty("docs")]
        public List<T> Docs { get; set; }
    }
}