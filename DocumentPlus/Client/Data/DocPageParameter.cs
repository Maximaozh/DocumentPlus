namespace DocumentPlus.Client.Data
{
    public class DocPageParameter
    {
        public string NameDoc { get; set; }
        public string AuthorDoc { get; set; }
        public string? DescriptionDoc { get; set; }
        public DateTime? CreatedDateDoc { get; set; } = DateTime.Today;
        public DateTime? ExpireDateDoc { get; set; }
        public required string PathDoc { get; set; }
        public bool ReadOnlyForm { get; set; }
        public bool ReadOnlyText { get; set; }
    }
}
