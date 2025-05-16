

using User_Authapi.DTO_s;

namespace User_Authapi.Entities
{
    public class UserSessions
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Person User { get; set; } = null!;
        public DateTime LoggedInAt { get; set; }
        public DateTime? LoggedOutAt { get; set; }
        public string? AccessSessionToken { get; set; }
    }
}
