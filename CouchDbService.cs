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
                var user = result.Docs.FirstOrDefault();

                // Ensure user has the necessary fields
                if (user != null)
                {
                    user._id = user._id; // Ensure we are using _id from CouchDB
                }

                return user;
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
            // Try updating up to 3 times
            for (int attempt = 0; attempt < 3; attempt++)
            {
                // Fetch the latest version of the user to get the correct _rev
                var currentUser = await GetUserByIdAsync(user._id);

                if (currentUser == null)
                {
                    throw new Exception("User not found.");
                }

                // Ensure that we're using the latest revision
                user._rev = currentUser._rev;

                // Serialize the updated user and send the PUT request
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/users/{user._id}", content);

                // Check if the response was successful
                if (response.IsSuccessStatusCode)
                {
                    return; // Exit if update is successful
                }

                // If there's a conflict, we retry
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    if (attempt == 2) // Final attempt
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to update user after {attempt + 1} attempts: {response.ReasonPhrase}, Details: {errorContent}");
                    }

                    // Wait briefly before retrying
                    await Task.Delay(500); // Wait 500 ms before retry
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to update user: {response.ReasonPhrase}, Details: {errorContent}");
                }
            }
        }



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
