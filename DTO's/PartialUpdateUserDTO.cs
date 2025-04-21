namespace User_Authapi.DTO_s
{
    public record PartialUpdateUserDTO
    {  
        // i made this nullable so my user can select the field they want
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
