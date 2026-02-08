using System.ComponentModel.DataAnnotations;
namespace task.Models
{
    public class RegisterVm
    {
        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(1)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

    }
    
}