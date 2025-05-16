using System.ComponentModel.DataAnnotations;

namespace User_Authapi.DTO_s
{
    public class LogoutDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
