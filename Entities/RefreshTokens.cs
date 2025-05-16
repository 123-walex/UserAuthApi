using User_Authapi.DTO_s;

namespace User_Authapi.Entities
{
    public class RefreshTokens
    {       
        // the primary key on the db
            public int Id { get; set; }
        // the token itself
            public string RefreshToken { get; set; } = null!;
            public DateTime Expires { get; set; }
            public bool IsExpired => DateTime.UtcNow >= Expires;
            public DateTime Created { get; set; }
        // ip adress creating the token
            public string CreatedByIp { get; set; } = null!;
            public DateTime? Revoked { get; set; }
        // the revoking ip adress   
            public string? RevokedByIp { get; set; }
        // new refresh token  
            public string? ReplacedByToken { get; set; }
            public bool IsActive => Revoked == null && !IsExpired;

            // Foreign key reference to user
            public int UserId { get; set; }
            public Person User { get; set; } = null!;
        }
}
