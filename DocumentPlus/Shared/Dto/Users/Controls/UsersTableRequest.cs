namespace DocumentPlus.Shared.Dto.Users.Controls
{
    public class UsersTableRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public string? Order { get; set; }
        public int SortDirection { get; set; }
    }
}
