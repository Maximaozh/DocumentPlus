namespace DocumentPlus.Shared.Dto.Users
{
    public class UserInfo
    {
        public required string Id { get; set; }
        public required string Login { get; set; }
        public required string Role { get; set; }
        public string? Surname { get; set; }
        public string? Name { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Login, Role, Surname, Name);
        }

        public override bool Equals(object? obj)
        {
            return obj is UserInfo other
                ? Id == other.Id &&
                       Login == other.Login &&
                       Role == other.Role &&
                       Surname == other.Surname &&
                       Name == other.Name
                : false;
        }
    }
}
