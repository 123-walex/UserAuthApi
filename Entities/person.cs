using User_Authapi.Entities;

namespace User_Authapi.DTO_s
{
    public class Person
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateTime? UpdatedAt { get ; set; }
        public DateTime? CreatedAt { get; set; } 
        public DateTime? DeletedAt { get; set; }
        public DateTime? RestoredAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<UserSessions> Sessions { get; set; } = new List<UserSessions>();
        public ICollection<RefreshTokens> RefreshTokens { get; set; } = new List<RefreshTokens>();

    }
}
