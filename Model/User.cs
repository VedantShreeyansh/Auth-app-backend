using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace auth_app_backend.Model
{
    public class User
    {
        [JsonProperty("_id")]
        public string _id { get; set; }

        [JsonIgnore]
        public string _rev { get; set; }

        [JsonProperty("Id")]
        public string Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string firstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string lastName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string email { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string role { get; set; }

        public string status { get; set; }

        public User() { }

        public User(string firstName, string lastName, string email, string password, string role, string status, string id = null)
        {
            this.firstName = firstName;  // Changed to camelCase
            this.lastName = lastName;    // Changed to camelCase
            this.email = email;          // Changed to camelCase
            this.password = password;    // Changed to camelCase
            this.role = role;            // Changed to camelCase
            this.status = status;        // Changed to camelCase
            this.Id = id;                // Optional, for frontend use
        }

        public override string ToString()
        {
            return $"User: {firstName} {lastName}, Email: {email}, Role: {role}, Status: {status}, Id: {Id}";
        }
    }
}
