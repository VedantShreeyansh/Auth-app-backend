using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace auth_app_backend.Model
{
    [Table("messages")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("context")]
        public string Text { get; set; }  // Change from 'Context' to 'Text' for alignment

        [Required]
        [Column("sender_id")]
        public int SenderId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("role")]
        public string Role { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
