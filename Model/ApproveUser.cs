using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class ApproveUser
{
    [Key]
    [JsonProperty("_id")]
    public string _id { get; set; }

    [JsonProperty("_rev")]
    public string _rev { get; set; } = null;

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

    public string status { get; set; } = "Pending";

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool isSuperAdmin { get; internal set; }

    // Constructor with all fields
    public ApproveUser(string id, string rev, string firstName, string lastName, string password, string email, string role, string status, DateTime timestamp, bool isSuperAdmin)
    {
        _id = id;
        _rev = rev;
        this.firstName = firstName;
        this.lastName = lastName;
        this.password = password;
        this.email = email;
        this.role = role;
        this.status = status;
        Timestamp = timestamp;
        this.isSuperAdmin = isSuperAdmin;
    }
}