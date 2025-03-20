using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RepositoryLayer.Entity
{
    public class UserEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Fullname { get; set; }

        [Required]
        public string Email { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; } //hashed password


        // ✅ Forgot Password ke liye naya field

        [JsonIgnore]
        public string? ResetToken { get; set; }

        [JsonIgnore]
        public DateTime? ResetTokenExpiry { get; set; }

        // Yeh field sirf response me dikhne ke liye hai
        [NotMapped]
        public string Source { get; set; }
    }
}
