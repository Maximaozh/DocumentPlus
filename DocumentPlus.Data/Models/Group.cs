using System.ComponentModel.DataAnnotations;

namespace DocumentPlus.Data.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }
        public List<UserGroup> UserGroups { get; set; }
    }
}
