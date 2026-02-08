using System.ComponentModel.DataAnnotations;
namespace task.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = "";

        public DateTime? LastLoginTime { get; set; }

        public enum UserStatus
        {
            Unverified = 0,
            Active = 1,
            Blocked = 2
        }
        public UserStatus Status { get; set; } = UserStatus.Unverified;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; //gets the current time and date

        public string? EmailVerificationToken { get; set; } 

        public DateTime? EmailVerificationTokenExpiresAt { get; set; }

        public string? EmailVerifiedAt { get; set; }
    }
}