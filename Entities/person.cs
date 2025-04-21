namespace User_Authapi.DTO_s
{
    public class Person
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public DateTime? UpdatedAt { get ; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? DeletedAt { get; set; }
        public DateTime? RestoredAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
