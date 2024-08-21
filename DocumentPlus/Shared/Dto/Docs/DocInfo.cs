namespace DocumentPlus.Shared.Dto.Docs;

    //Информация о документе
    public class DocInfo
    {
        public required string Name { get; set; }
        public required string Path { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }
    public class DocInfoGet
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public int UserId { get; set; }
        public required string Path { get; set; }
    }
    public class DocInfoGetId
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public required string Path { get; set; }
        public required string AuthorName { get; set; }
        public required string AuthorSurname { get; set; }
}
public class TreeItem
    {
        public string? Name { get; set; }
        public List<TreeItem> Children { get; set; } = new List<TreeItem>();
    }

    public class Folder
    {
        public string? Name { get; set; }
        public List<Folder> SubFolders { get; set; } = new List<Folder>();
        public List<DocInfoGet> Documents { get; set; } = new List<DocInfoGet>();
    }


