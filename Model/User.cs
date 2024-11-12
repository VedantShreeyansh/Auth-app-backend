using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class User
{
    [JsonProperty("_id")]
    public string _id { get; set; }

    [Newtonsoft.Json.JsonIgnore]

    [JsonProperty("_rev")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Specify System.Text.Json for JsonIgnore
    public string _rev { get; set; } = null; // Default to null for new documents

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

    public string status { get; set; } = "Pending"; // Default value for new users

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

}

   

