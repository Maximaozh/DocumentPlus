using DocumentPlus.Shared.Dto.Groups;

namespace DocumentPlus.Shared.Dto.Users.Controls
{
    public class GroupsByUsersResponse
    {
        public List<GroupInfo> Groups { get; set; }
        public int TotalCount { get; set; }
    }
}
