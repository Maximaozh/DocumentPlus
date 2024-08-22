using DocumentPlus.Shared.Dto.Users;

namespace DocumentPlus.Shared.Dto.Groups
{
    public class DeleteUserGroupsRequest
    {
        public HashSet<GroupInfo> Groups { get; set; }
        public HashSet<UserInfo> Users { get; set; }
    }
}
