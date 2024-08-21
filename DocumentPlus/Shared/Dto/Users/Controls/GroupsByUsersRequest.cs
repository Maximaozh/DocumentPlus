namespace DocumentPlus.Shared.Dto.Users.Controls
{
    // Запрос на получения списка групп, включающих всех выделенных юзеров
    public class GroupsByUsersRequest
    {
        public List<UserInfo> Users { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
