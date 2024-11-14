using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column("sender")]
        public string Sender { get; set; } // Store the _id value of the sender

        [Required]
        [Column("receiver")]
        public string Receiver { get; set; } // Store the _id value of the receiver

        [Required]
        [Column("text")]
        public string Text { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}






















































//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;

//namespace auth_app_backend.Model
//{
//    [Table("messages")]
//    public class Message
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [Column("context")]
//        public string Text { get; set; }  // Change from 'Context' to 'Text' for alignment

//        [Required]
//        [Column("sender_id")]
//        public int SenderId { get; set; }

//        [Required]
//        [MaxLength(50)]
//        [Column("role")]
//        public string Role { get; set; }

//        [Column("timestamp")]
//        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
//    }
//}
