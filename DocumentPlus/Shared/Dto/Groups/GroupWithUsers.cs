using DocumentPlus.Shared.Dto.Users;

namespace DocumentPlus.Shared.Dto.Groups
{
    public class GroupWithUsers : GroupInfo
    {
        public HashSet<UserInfo> Users { get; set; }
    }
}
