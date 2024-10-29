using System.ComponentModel.DataAnnotations;

namespace auth_app_backend.Model
{
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        public string Role { get; set; }
        public bool IsApproved { get; set; }

        public User() { }

        public User(string firstName, string lastName, string email, string password, string role, bool isApproved)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
            Role = role;
            IsApproved = isApproved;
        }

        public override string ToString()
        {
            return $"User: {FirstName} {LastName}, Email: {Email}, Role: {Role}";
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
