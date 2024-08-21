using System.ComponentModel.DataAnnotations;

namespace DocumentPlus.Data.Models;
public class Document
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string? Desc { get; set; }
    public DateTime Created { get; set; }
    public DateTime ExpireDate { get; set; }
    public User User { get; set; }
}
