namespace DocumentPlus.Shared.Dto
{
    public class UserRegistrate
    {
        public required string Login { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public string? Surname { get; set; }
        public string? Name { get; set; }
    }
}
