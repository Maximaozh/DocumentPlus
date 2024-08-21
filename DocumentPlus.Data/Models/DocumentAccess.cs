using System.ComponentModel.DataAnnotations;

namespace DocumentPlus.Data.Models
{
    public class DocumentAccess
    {
        [Key]
        public int Id { get; set; }
        public User? User { get; set; }
        public Document Document { get; set; }
        public Group? Group { get; set; }
        public int Access_level { get; set; }
    }
}
