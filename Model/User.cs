using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace auth_app_backend.Model
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string firstName { get; set; }  // Changed to camelCase

        [Required(ErrorMessage = "Last Name is required")]
        public string lastName { get; set; }   // Changed to camelCase

        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; }    // Changed to camelCase

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string email { get; set; }       // Changed to camelCase

        [Required(ErrorMessage = "Role is required")]
        public string role { get; set; }        // Changed to camelCase

        public string status { get; set; }      // Changed to camelCase

        [JsonIgnore] // This will prevent _rev from being serialized to JSON
        public string? _rev { get; set; }        // Added _rev as a private field

        public User() { }

        public User(string firstName, string lastName, string email, string password, string role, string status)
        {
            this.firstName = firstName;  // Changed to camelCase
            this.lastName = lastName;    // Changed to camelCase
            this.email = email;          // Changed to camelCase
            this.password = password;    // Changed to camelCase
            this.role = role;            // Changed to camelCase
            this.status = status;        // Changed to camelCase
        }

        public override string ToString()
        {
            return $"User: {firstName} {lastName}, Email: {email}, Role: {role}, Status: {status}, Id: {Id}";
        }
    }
}
